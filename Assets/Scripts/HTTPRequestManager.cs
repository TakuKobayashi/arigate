using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HTTPRequestManager : SingletonBehaviour<HTTPRequestManager> {
    public void RequestGet(string url, Action<DownloadHandler> callback)
    {
        StartCoroutine(RequestCorutine(UnityWebRequest.Get(url), callback));
    }

    public void RequestGet(string url, WWWForm requestParams, Action<DownloadHandler> callback)
    {
        string requestURL = url + "?" + Encoding.UTF8.GetString(requestParams.data);
        this.RequestGet(requestURL, callback);
    }

    public void RequestDelete(string url, Action<DownloadHandler> callback)
    {
        StartCoroutine(RequestCorutine(UnityWebRequest.Delete(url), callback));
    }

    public void RequestDelete(string url, WWWForm requestParams, Action<DownloadHandler> callback)
    {
        string requestURL = url + "?" + Encoding.UTF8.GetString(requestParams.data);
        RequestDelete(requestURL, callback);
    }

    public void RequestPut(string url, WWWForm requestParams, Action<DownloadHandler> callback)
    {
        StartCoroutine(RequestCorutine(UnityWebRequest.Put(url, requestParams.data), callback));
    }

    public void RequestPost(string url, WWWForm requestParams, Action<DownloadHandler> callback)
    {
        StartCoroutine(RequestCorutine(UnityWebRequest.Post(url, requestParams), callback));
    }

    public IEnumerator RequestCorutine(UnityWebRequest request, Action<DownloadHandler> callback){
        yield return request.Send();

        if (request.isNetworkError)
        {
            Debug.Log(request.error);
        }
        else
        {
            callback(request.downloadHandler);
        }
    }
}
