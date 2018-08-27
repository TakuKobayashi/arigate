using UnityEngine;
using WebSocket4Net;
using System;
using System.Security.Authentication;
using System.Collections;
using System.Collections.Generic;

public class WebSocketManager : SingletonBehaviour<WebSocketManager>{
	private WebSocket ws = null;
    private bool isSocketOpened = false;
	private List<string> receiveMessageQueue = new List<string>();
    private List<byte[]> receiveDataQueue = new List<byte[]>();
	public Action<string> OnReceiveMessage = null;
    public Action<byte[]> OnReceiveData = null;

    void Update()
	{
		if (receiveMessageQueue.Count > 0)
		{
			for (int i = 0; i < receiveMessageQueue.Count; ++i)
			{
				if (OnReceiveMessage != null)
				{
					OnReceiveMessage(receiveMessageQueue[i]);
				}
			}
            for (int i = 0; i < receiveDataQueue.Count; ++i)
            {
                if (OnReceiveData != null)
                {
                    OnReceiveData(receiveDataQueue[i]);
                }
            }
			receiveMessageQueue.Clear();
            receiveDataQueue.Clear();
		}
	}

	public void Connect(string wsUrl)
	{
		Debug.Log(wsUrl);
		// WebSocketのechoサーバ.
        this.ws = new WebSocket(wsUrl, sslProtocols: SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls);

		// WebSocketをOpen.
        this.ws.Opened += (sender, e) =>
		{
            isSocketOpened = true;
			Debug.Log("[WS] Open");
		};

		// メッセージを受信.
        this.ws.MessageReceived += (sender, e) =>
		{
            receiveMessageQueue.Add(e.Message);
            Debug.Log("[WS]Receive message: " + e.Message);
		};

        // メッセージを受信.
        this.ws.DataReceived += (sender, e) =>
        {
            receiveDataQueue.Add(e.Data);
            Debug.Log("[WS]Receive data: " + e.Data);
        };
        
		// WebSoketにErrorが発生.
        this.ws.Error += (sender, e) =>
		{
            Debug.Log("[WS]Error: " + e.Exception.Message);
		};

		// WebSocketがClose.
        this.ws.Closed += (sender, e) =>
		{
			Debug.Log("[WS]Close");
		};
        
		// WebSocketに接続.
        this.ws.Open();
	}

	public void Send(string message){
        StartCoroutine(SendCoroutine(message));
	}

    private IEnumerator SendCoroutine(string message){
        while(!isSocketOpened){
            yield return null;
        }
        this.ws.Send(message);        
    }
    
	public void Close()
	{
		if (this.ws != null)
		{
			this.ws.Close();
		}
	}
    
	void OnDestroy()
	{
		Close();
	}
}