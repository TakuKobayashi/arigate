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

    private User myUser;
    private List<User> otherUsers = new List<User>();

    public override void SingleAwake()
    {
        string myId = Guid.NewGuid().ToString();
        CurrentState = State.Waiting;
        Util.InstantiateTo(this.gameObject, websocketManagerObject);
        Util.InstantiateTo(this.gameObject, httprequestManagerObject);
        WebSocketManager.Instance.Connect("wss://websocketserversample.au-syd.mybluemix.net/arigate");
        WebSocketManager.Instance.OnReceiveMessage = OnReceiveMessage;
        JoinMyId(Guid.NewGuid().ToString());
    }

    public void JoinMyId(string userId){
        Dictionary<string, object> messageParams = new Dictionary<string, object>();
        messageParams.Add("action", "init");
        messageParams.Add("room_id", "aaa");
        messageParams.Add("member_count", 2);
        messageParams.Add("user_id", userId);
        string json = JsonConvert.SerializeObject(messageParams);
        WebSocketManager.Instance.Send(json);
        //CheckStartAndChangeState();
    }

    private void OnReceiveMessage(string message){
        Room room = JsonConvert.DeserializeObject<Room>(message);
        UpdateRoomUsers(room);
        if (CurrentState == State.Timeup || CurrentState == State.Finish){
            return;
        }
        if(room.action == "start_count_down"){
            ChangeState(State.CountDown);
        }
        else if (room.action == "appear_object")
        {
            if(room.targets != null){
                for (int i = 0; i < appearSymbols.Count;++i){
                    DeleteSymbol(appearSymbols[i]);
                }
                for (int i = 0; i < room.targets.Count; ++i)
                {
                    TargetObj target = room.targets[i];
                    AppearSymbol(new Vector3(target.x, target.y, target.z), target.asset_index, target.id);
                }
            }
        }
    }

    private void UpdateRoomUsers(Room room){
        if(room.my_user != null){
            myUser = room.my_user;
            CurrentPoint = (int) myUser.point;
        }
        if (room.room_users != null)
        {
            otherUsers = room.room_users;
            OtherPoint = (int) otherUsers[0].point;
        }
    }

    /*
    private void CheckStartAndChangeState(){
        if(!string.IsNullOrEmpty(myId) && !string.IsNullOrEmpty(otherId)){
            ChangeState(State.CountDown);
        }
    }
    */

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
            DeleteSymbol(hitsym);
            ClearTime();

            Dictionary<string, object> messageParams = new Dictionary<string, object>();
            messageParams.Add("action", "contact");
            messageParams.Add("user_id", myUser.user_id);
            messageParams.Add("room_id", myUser.room_id);
            string json = JsonConvert.SerializeObject(messageParams);
            WebSocketManager.Instance.Send(json);

//            RandomAppearObject();
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
//        messageParams.Add("userId", myId);
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
