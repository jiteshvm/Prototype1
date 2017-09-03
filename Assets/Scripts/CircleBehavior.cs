using UnityEngine;

public class CircleBehavior : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("OnTriggerEnter2D : " + collision.gameObject.name);
        PoolManager.Instance.Despawn(collision.gameObject);

    }
}
