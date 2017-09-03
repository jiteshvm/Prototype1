using UnityEngine;
using System.Collections;

public class EnemySpawner : Singleton<EnemySpawner> {

    protected EnemySpawner() {} // guarantee this will be always a singleton only - can't use the constructor!

    public GameObject EnemyPrefab;

    public int EnemyPoolSize = 50;

    private void Awake()
    {
        if(EnemyPrefab)
        {
			PoolManager.Instance.CreatePool(EnemyPrefab, EnemyPoolSize);
			Instance.StartCoroutine("SpawnEnemy");    
        }
    }

    IEnumerator SpawnEnemy()
    {
        while(true)
        {
			Vector3 screenPosition = Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 10));
            PoolManager.Instance.Spawn(EnemyPrefab, screenPosition, Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward));
			yield return new WaitForSeconds(Random.Range(2.0f, 5.0f));    
        }		
    }

    void Start () {
		
	}
	
	void Update () {
		
	}
}
