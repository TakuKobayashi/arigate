﻿using UnityEngine;
using System;

public class TargetSymbol : MonoBehaviour
{
    [SerializeField] private float hitDistance;
    [SerializeField] private UnityScriptableObject symbolAssetDB;
    [SerializeField] private GameObject cursorObject;

    public int AssetIndex { private set; get; }
    public string Uuid { private set; get; }

    public void Init()
    {
        GameObject[] assets = symbolAssetDB.GetObjects<GameObject>();
        System.Random random = new System.Random();
        Init(random.Next(assets.Length), Guid.NewGuid().ToString());
    }

    public void Init(int assetIndex, string uuid)
    {
        Uuid = uuid;
        AssetIndex = assetIndex;
        GameObject[] assets = symbolAssetDB.GetObjects<GameObject>();
        GameObject symbol = assets[assetIndex];
        Util.InstantiateTo(this.gameObject, symbol);
    }

    public bool IsHit(Vector3 targetPosition)
    {
        float distance = Vector3.SqrMagnitude(transform.position - targetPosition);
        return distance < hitDistance;
    }
}
