using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogHandler : MonoBehaviour {

	private string screenLog;

	private Queue LogQueue = new Queue();

	private GUIStyle guiStyle = new GUIStyle();


	void Start () {
		guiStyle.normal.textColor = Color.black;
		guiStyle.fontSize = 12;
	}
	
	/*
	void Update () {
		
	}
    */

	void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	void HandleLog(string logString, string stackTrace, LogType type)
	{

		if (LogQueue.Count >= 8)
			LogQueue.Dequeue();

		string newString = "\n [" + type + "] : " + logString;
		LogQueue.Enqueue(newString);
		if (type == LogType.Exception)
		{
			newString = "\n" + stackTrace;
			LogQueue.Enqueue(newString);
		}
		screenLog = string.Empty;
		foreach (string mylog in LogQueue)
		{
			screenLog += mylog;
		}
	}

	void OnGUI()
	{
		GUILayout.Label(screenLog, guiStyle);
	}
}
