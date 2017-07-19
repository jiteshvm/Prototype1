using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleTouchInput : MonoBehaviour {

	float touchDuration = 0.0f;

	private Touch currentTouch;

	public float TimeToTap = 0.2f;

	private string screenLog;

	private Queue LogQueue = new Queue();

	private GUIStyle guiStyle = new GUIStyle();

	public GameObject TapEffect;
	
	GameObject TapEffectRef;

	private Vector3 LastTapPos;

	void Start () {
		guiStyle.normal.textColor = Color.black;
		guiStyle.fontSize = 12;
		if(TapEffect != null) {
			TapEffectRef = Instantiate(TapEffect, Vector3.zero, Quaternion.identity);
			LastTapPos = Vector3.zero;
		}
		Debug.Log("(HandleTouchInput.cs) Start");
	}

	void OnEnable() {
		Application.logMessageReceived +=HandleLog;
	}

	void OnDisable() {
		Application.logMessageReceived -= HandleLog;
	}

	void Update () {
		
		if(Input.GetMouseButtonDown(0))
		{
			if(TapEffectRef)
			{
				Debug.Log("yes");
				LastTapPos.x = Input.mousePosition.x;
				LastTapPos.y = Input.mousePosition.y;
				LastTapPos.z = 10;
				if(TapEffectRef)
					TapEffectRef.transform.position = Camera.main.ScreenToWorldPoint(LastTapPos);
			}
					
		}
		if(Input.touchCount > 0) {
			touchDuration += Time.deltaTime;
			currentTouch = Input.GetTouch(0);
			if(currentTouch.phase == TouchPhase.Ended && touchDuration < TimeToTap) {
				Debug.Log("position : " + currentTouch.position + " , pressure : " + currentTouch.pressure);
				LastTapPos.x = currentTouch.position.x;
				LastTapPos.y = currentTouch.position.y;
				LastTapPos.z = 10;
				if(TapEffectRef)
					TapEffectRef.transform.position = Camera.main.ScreenToWorldPoint(LastTapPos);
				
			}
		}
		else {
			touchDuration = 0.0f;
		}
	}

	void HandleLog(string logString, string stackTrace, LogType type){

		if(LogQueue.Count >= 8)
			LogQueue.Dequeue();
		
		string newString = "\n [" + type + "] : " + logString;
		LogQueue.Enqueue(newString);
		if (type == LogType.Exception)
		{
			newString = "\n" + stackTrace;
			LogQueue.Enqueue(newString);
		}
		screenLog = string.Empty;
		foreach(string mylog in LogQueue){
			screenLog += mylog;
		}
	}

	void OnGUI() {
		GUILayout.Label(screenLog, guiStyle);
	}
}
