using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameController : SingletonBehaviour<GameController> {

    public enum State{
        Waiting,
        CountDown,
        Playing,
        Timeup,
        Finish
    }

    public float ElapsedSecond { private set; get; }

    private List<GameObject> appearSymbols = new List<GameObject>();
    [SerializeField] private float baseGivePoint = 100;
    [SerializeField] private UnityScriptableObject symbolAssetDB;
    [SerializeField] private float hitDistance = 0.1f;

    public State CurrentState { private set; get; }
    public float CurrentPoint{ private set; get; }
    public Action OnHit = null;

    private GameObject[] assets;

    public override void SingleAwake(){
        CurrentState = State.Waiting;
        assets = symbolAssetDB.GetObjects<GameObject>();
    }

    public void ChangeState(State state){
        CurrentState = state;
    }

    public GameObject AppearSymbol(Vector3 appearPoint, GameObject appearedObject){
        GameObject targetSymbol;
#if UNITY_ANDROID || UNITY_EDITOR
        GoogleARCore.Anchor anchor = GoogleARCore.Session.CreateAnchor(Pose.identity);
        anchor.transform.parent = this.transform;
        targetSymbol = Util.InstantiateTo(anchor.gameObject, appearedObject);;
#else
        targetSymbol = Util.InstantiateTo(this.gameObject, appearedObject);
#endif
        targetSymbol.transform.position = appearPoint;
        appearSymbols.Add(targetSymbol);
        return targetSymbol;
    }

	// Update is called once per frame
	void Update () {
        ElapsedSecond += Time.deltaTime;
        // TODO Reset
        if(CurrentState == State.Waiting && ElapsedSecond > 5){
            ChangeState(State.CountDown);
        }
        CheckHitAndGetPoint();
	}

    public void CheckHitAndGetPoint(){
        GameObject hitsym = appearSymbols.Find(sym => {
            float distance = Vector3.SqrMagnitude(sym.transform.position - Camera.main.transform.position);
            Debug.Log(distance);
            return distance < hitDistance;
        });
        if(hitsym != null){
            CurrentPoint += Mathf.Max(baseGivePoint - ElapsedSecond, 0);
            Destroy(hitsym);
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
        float x = UnityEngine.Random.RandomRange(0f, 2.0f);
        float y = UnityEngine.Random.RandomRange(-0.5f, 0.5f);
        float z = UnityEngine.Random.RandomRange(0f, 2.0f);
        GameObject symbol = assets[random.Next(assets.Length)];
        AppearSymbol(new Vector3(x, y, z), symbol);
    }

    public void ClearTime(){
        ElapsedSecond = 0f;
    }
}
