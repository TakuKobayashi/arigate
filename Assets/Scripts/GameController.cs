using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;

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
    public int OtherPoint { private set; get; }
    public Action OnHit = null;

    private string myId = "";
    private string otherId = "";

    public override void SingleAwake()
    {
        myId = Guid.NewGuid().ToString();
        CurrentState = State.Waiting;
        Util.InstantiateTo(this.gameObject, websocketManagerObject);
        Util.InstantiateTo(this.gameObject, httprequestManagerObject);
        WebSocketManager.Instance.Connect("wss://websocketserversample.au-syd.mybluemix.net/");
        WebSocketManager.Instance.OnReceiveMessage = OnReceiveMessage;
        JoinMyId(Guid.NewGuid().ToString());
    }

    public void JoinMyId(string userId){
        myId = userId;
        Dictionary<string, string> messageParams = new Dictionary<string, string>();
        messageParams.Add("action", "init");
        messageParams.Add("userId", myId);
        string json = JsonConvert.SerializeObject(messageParams);
        WebSocketManager.Instance.Send(json);
        CheckStartAndChangeState();
    }

    private void OnReceiveMessage(string message){
        Dictionary<string, string> messageDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
        if(messageDic.ContainsKey("action") && messageDic["action"] == "init"){
            if(messageDic.ContainsKey("userId") && messageDic["userId"] != myId){
                otherId = messageDic["userId"];
                CheckStartAndChangeState();
            }
        }
        else if (messageDic.ContainsKey("action") && messageDic["action"] == "appearObject")
        {
            if (messageDic.ContainsKey("userId") && messageDic["userId"] != myId){
                AppearSymbol(new Vector3(
                    float.Parse(messageDic["x"]),
                    float.Parse(messageDic["y"]),
                    float.Parse(messageDic["z"])),
                             int.Parse(messageDic["assetIndex"]),
                             messageDic["uuid"]
                );
            }
        }
        else if (messageDic.ContainsKey("action") && messageDic["action"] == "GetPoint")
        {
            if (messageDic.ContainsKey("userId") && messageDic["userId"] != myId)
            {
                TargetSymbol hitsym = appearSymbols.Find(sym =>
                {
                    return sym.Uuid == messageDic["uuid"];
                });
                OtherPoint = int.Parse(messageDic["point"]);
                if(hitsym != null){
                    DeleteSymbol(hitsym);
                }
            }
        }
    }

    private void CheckStartAndChangeState(){
        if(!string.IsNullOrEmpty(myId) && !string.IsNullOrEmpty(otherId)){
            ChangeState(State.CountDown);
        }
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

    public TargetSymbol AppearSymbol(Vector3 appearPoint, int index, string uuid){
        TargetSymbol targetSymbol;
#if UNITY_ANDROID || UNITY_EDITOR
        GoogleARCore.Anchor anchor = GoogleARCore.Session.CreateAnchor(Pose.identity);
        anchor.transform.parent = this.transform;
        targetSymbol = Util.InstantiateTo<TargetSymbol>(anchor.gameObject, symbolObject); ;
#else
        targetSymbol = Util.InstantiateTo<TargetSymbol>(this.gameObject, symbolObject);
#endif
        targetSymbol.Init(index, uuid);
        targetSymbol.transform.position = appearPoint;
        appearSymbols.Add(targetSymbol);
        return targetSymbol;
    }

    // Update is called once per frame
    void Update()
    {
        ElapsedSecond += Time.deltaTime;
        // TODO Reset
        /*
        if (CurrentState == State.Waiting && ElapsedSecond > 5)
        {
            ChangeState(State.CountDown);
        }
        */
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
            DeleteSymbol(hitsym);
            ClearTime();

            Dictionary<string, string> messageParams = new Dictionary<string, string>();
            messageParams.Add("action", "GetPoint");
            messageParams.Add("userId", myId);
            messageParams.Add("uuid", hitsym.Uuid);
            messageParams.Add("point", CurrentPoint.ToString());
            string json = JsonConvert.SerializeObject(messageParams);
            WebSocketManager.Instance.Send(json);

            RandomAppearObject();
            if(OnHit != null){
                OnHit();
            }
        }
    }

    public void DeleteSymbol(TargetSymbol hitsym)
    {
#if UNITY_ANDROID || UNITY_EDITOR
            Destroy(hitsym.transform.parent.gameObject);
#else
            Destroy(hitsym.gameObject);
#endif
            appearSymbols.Remove(hitsym);
    }

    public void RandomAppearObject(){
        System.Random random = new System.Random();
        float x = UnityEngine.Random.RandomRange(0f, 1.5f);
        float y = UnityEngine.Random.RandomRange(-0.5f, 0.5f);
        float z = UnityEngine.Random.RandomRange(0f, 1.5f);

        Dictionary<string, string> messageParams = new Dictionary<string, string>();
        messageParams.Add("action", "appearObject");
        messageParams.Add("userId", myId);
        messageParams.Add("x", x.ToString());
        messageParams.Add("y", y.ToString());
        messageParams.Add("z", z.ToString());

        var target = AppearSymbol(new Vector3(x, y, z));
        messageParams.Add("assetIndex", target.AssetIndex.ToString());
        messageParams.Add("uuid", target.Uuid);
        string json = JsonConvert.SerializeObject(messageParams);
        WebSocketManager.Instance.Send(json);
    }

    public void ClearTime(){
        ElapsedSecond = 0f;
    }
}
