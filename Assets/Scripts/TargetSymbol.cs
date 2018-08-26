using UnityEngine;
using System.Collections;

public class TargetSymbol : MonoBehaviour
{
    [SerializeField] private float hitDistance;
    [SerializeField] private UnityScriptableObject symbolAssetDB;
    [SerializeField] private GameObject cursorObject;

    public void Init()
    {
        GameObject[] assets = symbolAssetDB.GetObjects<GameObject>();
        System.Random random = new System.Random();
        GameObject symbol = assets[random.Next(assets.Length)];
        Util.InstantiateTo(this.gameObject, symbol);
    }

    public bool IsHit(Vector3 targetPosition)
    {
        float distance = Vector3.SqrMagnitude(transform.position - targetPosition);
        return distance < hitDistance;
    }
}
