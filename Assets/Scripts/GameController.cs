using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameController : SingletonBehaviour<GameController> {

    public float ElapsedSecond { private set; get; }

    private List<TargetSymbol> appearSymbols = new List<TargetSymbol>();
    [SerializeField] private float baseGivePoint = 100;
    [SerializeField] private UnityScriptableObject symbolAssetDB;
    public float CurrentPoint{ private set; get; }
    public Action OnHit = null;

    public TargetSymbol AppearSymbol(Vector3 appearPoint, GameObject appearedObject){
        TargetSymbol targetSymbol;
#if UNITY_ANDROID || UNITY_EDITOR
        GoogleARCore.Anchor anchor = GoogleARCore.Session.CreateAnchor(Pose.identity);
        anchor.transform.parent = this.transform;
        targetSymbol = Util.InstantiateTo<TargetSymbol>(anchor.gameObject, appearedObject);;
#else
        targetSymbol = Util.InstantiateTo<TargetSymbol>(this.gameObject, appearedObject);
#endif
        appearSymbols.Add(targetSymbol);
        return targetSymbol;
    }

	// Update is called once per frame
	void Update () {
        ElapsedSecond += Time.deltaTime;
        CheckHitAndGetPoint();
	}

    public void CheckHitAndGetPoint(){
        TargetSymbol hitsym = appearSymbols.Find(sym => sym.IsHit(Camera.main.transform.position));
        if(hitsym != null){
            CurrentPoint += Mathf.Max(baseGivePoint - ElapsedSecond, 0);
            appearSymbols.Remove(hitsym);
            ClearTime();
            if(OnHit != null){
                OnHit();
            }
        }
    }

    public void ClearTime(){
        ElapsedSecond = 0f;
    }
}
