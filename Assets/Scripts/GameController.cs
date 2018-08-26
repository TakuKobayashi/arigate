using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameController : SingletonBehaviour<GameController>
{

    public enum State
    {
        Waiting,
        CountDown,
        Playing,
        Timeup,
        Finish
    }

    public float ElapsedSecond { private set; get; }

    private List<TargetSymbol> appearSymbols = new List<TargetSymbol>();
    [SerializeField] private float baseGivePoint = 100;
    [SerializeField] private GameObject symbolObject;
    [SerializeField] private GameObject websocketManagerObject;
    [SerializeField] private GameObject httprequestManagerObject;

    public State CurrentState { private set; get; }
    public int CurrentPoint { private set; get; }
    public Action OnHit = null;

    private string myId = "";
    private string otherId = "";

    public override void SingleAwake()
    {
        myId = Guid.NewGuid().ToString();
        CurrentState = State.Waiting;
        Util.InstantiateTo(this.gameObject, websocketManagerObject);
        Util.InstantiateTo(this.gameObject, httprequestManagerObject);
        WebSocketManager.Instance.Connect("ws://websocketserversample.au-syd.mybluemix.net/");
        WebSocketManager.Instance.OnReceiveMessage = OnReceiveMessage;

        Dictionary<string, object> messageParams = new Dictionary<string, object>();

    }

    private void OnReceiveMessage(string message){
    }

    public void ChangeState(State state)
    {
        CurrentState = state;
    }

    public TargetSymbol AppearSymbol(Vector3 appearPoint)
    {
        TargetSymbol targetSymbol;
#if UNITY_ANDROID || UNITY_EDITOR
        GoogleARCore.Anchor anchor = GoogleARCore.Session.CreateAnchor(Pose.identity);
        anchor.transform.parent = this.transform;
        targetSymbol = Util.InstantiateTo<TargetSymbol>(anchor.gameObject, symbolObject); ;
#else
        targetSymbol = Util.InstantiateTo<TargetSymbol>(this.gameObject, symbolObject);
#endif
        targetSymbol.Init();
        targetSymbol.transform.position = appearPoint;
        appearSymbols.Add(targetSymbol);
        return targetSymbol;
    }

    // Update is called once per frame
    void Update()
    {
        ElapsedSecond += Time.deltaTime;
        // TODO Reset
        if (CurrentState == State.Waiting && ElapsedSecond > 5)
        {
            ChangeState(State.CountDown);
        }
        CheckHitAndGetPoint();
    }

    public void CheckHitAndGetPoint()
    {
        TargetSymbol hitsym = appearSymbols.Find(sym =>
        {
            return sym.IsHit(Camera.main.transform.position);
        });
        if (hitsym != null)
        {
            CurrentPoint += (int)Mathf.Max(baseGivePoint - ElapsedSecond, 0);
#if UNITY_ANDROID || UNITY_EDITOR
            Destroy(hitsym.transform.parent.gameObject);
#else
            Destroy(hitsym.gameObject);
#endif
            appearSymbols.Remove(hitsym);
            ClearTime();

            RandomAppearObject();
            if(OnHit != null){
                OnHit();
            }
        }
    }

    public void RandomAppearObject(){
        System.Random random = new System.Random();
        float x = UnityEngine.Random.RandomRange(0f, 1.5f);
        float y = UnityEngine.Random.RandomRange(-0.5f, 0.5f);
        float z = UnityEngine.Random.RandomRange(0f, 1.5f);
        AppearSymbol(new Vector3(x, y, z));
    }

    public void ClearTime(){
        ElapsedSecond = 0f;
    }
}
