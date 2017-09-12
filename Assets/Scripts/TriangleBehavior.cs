using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TriangleBehavior : MonoBehaviour {

	private void Start () {
        Init();
	}


    public void Init()
    {
        gameObject.transform.localScale = new Vector3(5, 5, 1);
        StartCoroutine("MoveTo");
    }

    IEnumerator MoveTo()
    {
        while(true){
            Vector3 screenPosition = Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 10));
            float duration = Random.Range(2.0f, 5.0f);
            gameObject.transform.DOMove(screenPosition, duration);
            yield return new WaitForSeconds(duration);
        }

    }

    private void OnDestroy()
    {
        
    }
}
