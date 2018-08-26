using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ARUICanvas : MonoBehaviour {
    [SerializeField] private Scrollbar timeLimitBar;
    [SerializeField] private Text messageText;
    [SerializeField] private Text scoreText;
    [SerializeField] private int limitTimeSecond = 60;

    private int countdown = 3;
    private DateTime startDateTime;
    private GameController.State prevState = GameController.State.Waiting;

	void Start () {
        startDateTime = DateTime.Now;
        timeLimitBar.value = 0f;
        timeLimitBar.size = 1.0f;
        Input.compass.enabled = true;
        Input.location.Start();
	}
	
	// Update is called once per frame
	void Update () {
        GameController.State currentState = GameController.Instance.CurrentState;
//        scoreText.text = "Score: " + GameController.Instance.CurrentPoint.ToString() + "\n" + Input.compass.magneticHeading.ToString();
        scoreText.text = "MyScore: " + GameController.Instance.CurrentPoint.ToString() + "\n" + 
            "OtherScore: " + GameController.Instance.OtherPoint.ToString();
        if(prevState == GameController.State.Waiting && currentState == GameController.State.CountDown){
            startDateTime = DateTime.Now;
        }
        if (currentState == GameController.State.Waiting)
        {
            messageText.gameObject.SetActive(true);
            int point = (int)(DateTime.Now - startDateTime).TotalSeconds % 3 + 1;
            string message = "Waiting";
            for (int i = 0; i < point; ++i)
            {
                message += ".";
            }
            messageText.text = message;
        }
        else if (currentState == GameController.State.CountDown)
        {
            messageText.gameObject.SetActive(true);
            int remainCount = countdown - (int)(DateTime.Now - startDateTime).TotalSeconds;
            messageText.text = remainCount.ToString();
            if (remainCount == 0){
                messageText.text = "Start!!";
            }else if (remainCount < 0){
                startDateTime = DateTime.Now;
                messageText.gameObject.SetActive(false);
                GameController.Instance.ClearTime();
                GameController.Instance.ChangeState(GameController.State.Playing);
                GameController.Instance.RandomAppearObject();
            }
        }
        else if (currentState == GameController.State.Playing)
        {
            int remainSecond = limitTimeSecond - (int)(DateTime.Now - startDateTime).TotalSeconds;
            timeLimitBar.size = (float)((float)remainSecond / (float)limitTimeSecond);
            if(remainSecond <= 0){
                startDateTime = DateTime.Now;
                GameController.Instance.ChangeState(GameController.State.Timeup);
            }
        }
        else if (currentState == GameController.State.Timeup)
        {
            timeLimitBar.size = 0f;
            messageText.gameObject.SetActive(true);
            int timerSecond = (int)(DateTime.Now - startDateTime).TotalSeconds;
            messageText.text = "Timeup!!!!";
            if (timerSecond > 3)
            {
                messageText.gameObject.SetActive(false);
                GameController.Instance.ChangeState(GameController.State.Finish);
            }
        }
        else if (currentState == GameController.State.Finish)
        {
            messageText.gameObject.SetActive(true);
            string mes = "Result\n" +
                "MyScore: " + GameController.Instance.CurrentPoint.ToString() + "\n" +
                "OtherScore: " + GameController.Instance.OtherPoint.ToString()+ "\n";
            if(GameController.Instance.CurrentPoint > GameController.Instance.OtherPoint){
                mes += "WIN";
            }else if(GameController.Instance.CurrentPoint < GameController.Instance.OtherPoint){
                mes += "LOSE";
            }else{
                mes += "DRAW";
            }
            messageText.text = mes;
        }

        prevState = currentState;
	}
}
