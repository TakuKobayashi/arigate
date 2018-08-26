using UnityEngine;
using WebSocketSharp;
using System;
using System.Collections.Generic;

public class WebSocketManager : SingletonBehaviour<WebSocketManager>{
	private WebSocket ws = null;
	private List<MessageEventArgs> receiveMessageQueue = new List<MessageEventArgs>();
	public Action<string> OnReceiveMessage = null;
    
	void Update()
	{
		if (receiveMessageQueue.Count > 0)
		{
			for (int i = 0; i < receiveMessageQueue.Count; ++i)
			{
				if (OnReceiveMessage != null)
				{
					OnReceiveMessage(receiveMessageQueue[i].Data);
				}
			}
			receiveMessageQueue.Clear();
		}
	}

	public void Connect(string wsUrl)
	{
		Debug.Log(wsUrl);
		// WebSocketのechoサーバ.
		this.ws = new WebSocket(wsUrl);

		// WebSocketをOpen.
		this.ws.OnOpen += (sender, e) =>
		{
			Debug.Log("[WS] Open");
		};

		// メッセージを受信.
		this.ws.OnMessage += (sender, e) =>
		{
			receiveMessageQueue.Add(e);
			Debug.Log("[WS]Receive message: " + e.Data);
		};
        
		// WebSoketにErrorが発生.
		this.ws.OnError += (sender, e) =>
		{
			Debug.Log("[WS]Error: " + e.Message);
		};

		// WebSocketがClose.
		this.ws.OnClose += (sender, e) =>
		{
			Debug.Log("[WS]Close");
		};
        
		// WebSocketに接続.
		this.ws.Connect();
	}

	public void Send(string message){
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