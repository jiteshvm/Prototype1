using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CircleBehavior : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("OnTriggerEnter2D : " + collision.gameObject.name);
        StartCoroutine(DestroyTriangle(collision.gameObject));


    }

    IEnumerator DestroyTriangle(GameObject obj)
    {
        obj.transform.DOKill();
        Vector3 scaleUp = new Vector3(7,7,1);
        obj.transform.DOScale(scaleUp, 0.1f);
        yield return new WaitForSeconds(0.1f);
        obj.transform.DOScale(Vector3.zero, 0.5f);
        yield return new WaitForSeconds(0.5f);
        PoolManager.Instance.Despawn(obj);
    }
}
