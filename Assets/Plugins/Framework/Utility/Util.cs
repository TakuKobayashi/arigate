using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Util
{
    /// <summary>
    /// <para>初期化</para>
    /// <para>【第1引数】初期化したいTransform</para>
    /// </summary>
    public static void Normalize(Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localEulerAngles = Vector3.zero;
        t.localScale = Vector3.one;
    }

    /// <summary>
    /// <para>GameObjectを生成</para>
    /// <para>【第1引数】親となるGameObject</para>
    /// <para>【第2引数】生成したいGameObject(外部から持ってきた物)</para>
    /// <para>【戻り値】生成されたGameObject</para>
    /// </summary>
    public static GameObject InstantiateTo(GameObject parent, GameObject go)
    {
        GameObject ins = (GameObject)GameObject.Instantiate(
            go,
            parent.transform.position,
            parent.transform.rotation
        );
        ins.transform.parent = parent.transform;
        Normalize(ins.transform);
        return ins;
    }

    /// <summary>
    /// <para>GameObjectを生成</para>
    /// <para>【第1引数】親となるGameObject</para>
    /// <para>【第2引数】生成したいPrefab(外部から持ってきた物)</para>
    /// <para>【戻り値】生成されたGameObject</para>
    /// </summary>
    public static GameObject InstantiateTo(GameObject parent, Prefab prefab)
    {
        return InstantiateTo(parent, prefab.gameObject);
    }

    /// <summary>
    /// <para>GameObjectを生成し、そのGameObjectに張り付いている、Componentクラスを取得する</para>
    /// <para>【第1引数】親となるGameObject</para>
    /// <para>【第2引数】生成したいGameObject(外部から持ってきた物)</para>
    /// <para>【戻り値】生成されたGameObjectに張り付いてる指定したComponentクラス</para>
    /// </summary>
    public static T InstantiateTo<T>(GameObject parent, GameObject go)
        where T : Component
    {
        return InstantiateTo(parent, go).GetComponent<T>();
    }

    /// <summary>
    /// <para>GameObjectを生成し、そのGameObjectに張り付いている、Componentクラスを取得する</para>
    /// <para>【第1引数】親となるGameObject</para>
    /// <para>【第2引数】生成したいPrefab(外部から持ってきた物)</para>
    /// <para>【戻り値】生成されたGameObjectに張り付いてる指定したComponentクラス</para>
    /// </summary>
    public static T InstantiateTo<T>(GameObject parent, Prefab prefab)
        where T : Component
    {
        return InstantiateTo(parent, prefab.gameObject).GetComponent<T>();
    }

    /// <summary>
    /// <para>Transform以下に紐付いている子のGameObjectを全て削除する</para>
    /// <para>【第1引数】削除したいTransform</para>
    /// </summary>
    public static void DeleteAllChildren(Transform parent)
    {
        List<Transform> transformList = new List<Transform>();
        foreach (Transform child in parent)
        {
            transformList.Add(child);
        }
        parent.DetachChildren();
        foreach (Transform child in transformList)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// <para>指定した秒数だけ遅らせて処理を実行させる</para>
    /// <para>【第1引数】遅らせたい秒数</para>
    /// <para>【第2引数】遅らせた後に実行する処理</para>
    /// </summary>
    public static IEnumerator DelayedAction(float delaySecond, Action action)
    {
        yield return new WaitForSeconds(delaySecond);
        action();
    }

    public static T GetForegroundHitComponent<T>(Vector3 position) where T : Component
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits == null)
        {
            return null;
        }
        float distance = float.MaxValue;
        T result = null;
        for (int i = 0; i < hits.Length; ++i)
        {
            T hit = hits[i].collider.transform.parent.GetComponent<T>();
            if (hit != null)
            {
                if (distance > hits[i].distance)
                {
                    result = hit;
                    distance = hits[i].distance;
                }
            }
        }
        return result;
    }

    public static TimeSpan ExecuteBenchmark(Action process)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        process();
        sw.Stop();
        TimeSpan ts = sw.Elapsed;
        Debug.Log(string.Format("Execute: {0} ms", ts.TotalMilliseconds));
        return ts;
    }

    public static IEnumerator ExecuteBenchmarkCoroutine(Func<IEnumerator> process)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        yield return process();
        sw.Stop();
        TimeSpan ts = sw.Elapsed;
        Debug.Log(string.Format("Execute: {0} ms, Before Frame: {1} ms", ts.TotalMilliseconds, ts.TotalMilliseconds - (Time.deltaTime * 1000)));
        yield return ts;
    }

    public static T FindCompomentInChildren<T>(Transform root) where T : class
    {
        for (int i = 0; i < root.childCount; ++i)
        {
            Transform t = root.GetChild(i);
            T compoment = t.GetComponent<T>();
            if (!compoment.Equals(null))
            {
                return compoment;
            }

            // GetCompomentのnullとreturn nullとは違うらしい...
            T childCompoment = FindCompomentInChildren<T>(t);
            if (childCompoment != null)
            {
                return childCompoment;
            }
        }

        return null;
    }

    public static List<T> FindAllCompomentInChildren<T>(Transform root) where T : class
    {
        List<T> compoments = new List<T>();
        for (int i = 0; i < root.childCount; ++i)
        {
            Transform t = root.GetChild(i);
            T compoment = t.GetComponent<T>();
            if (!compoment.Equals(null))
            {
                compoments.Add(compoment);
            }

            // GetCompomentのnullとreturn nullとは違うらしい...
            List<T> childCompoments = FindAllCompomentInChildren<T>(t);
            compoments.AddRange(childCompoments);
        }

        return compoments;
    }
}