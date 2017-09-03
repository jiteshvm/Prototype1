using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandleTouchInput : MonoBehaviour {

	float touchDuration = 0.0f;

	private Touch currentTouch;

	public float TimeToTap = 0.2f;

	public GameObject TapEffect;
	
	GameObject TapEffectRef;

	private Vector3 LastTapPos;

    public Vector3 StartScale = new Vector3(1.0f, 1.0f, 1.0f);

    public Vector3 EndScale = new Vector3(5.0f, 5.0f, 5.0f);

	void Start () {
		
		if(TapEffect != null) {
            TapEffectRef = PoolManager.Instance.Spawn(TapEffect, Vector3.zero, Quaternion.identity);
			//TapEffectRef = Instantiate(TapEffect, Vector3.zero, Quaternion.identity);
			LastTapPos = Vector3.zero;
		}
        //PoolManager.Despawn(TapEffectRef);
		Debug.Log("(HandleTouchInput.cs) Start");
	}
	
	void Update () {
		
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("down");
            TapEffectRef.transform.localScale = StartScale;
			LastTapPos.x = Input.mousePosition.x;
			LastTapPos.y = Input.mousePosition.y;
			LastTapPos.z = 10;
			TapEffectRef.transform.position = Camera.main.ScreenToWorldPoint(LastTapPos);
            DOTween.Clear();
            TapEffectRef.transform.DOScale(EndScale, 1.0f);

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
}
