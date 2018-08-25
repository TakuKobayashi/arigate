using UnityEngine;
using System.Collections;

public class TargetSymbol : MonoBehaviour
{
    [SerializeField] private float hitDistance;

    public bool IsHit(Vector3 targetPosition)
    {
        float distance = Vector3.SqrMagnitude(transform.position - targetPosition);
        Debug.Log(distance);
        return distance < hitDistance;
    }
}
