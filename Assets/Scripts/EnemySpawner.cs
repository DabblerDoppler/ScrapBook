using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemySpawner : NetworkBehaviour {

    public GameObject associatedEnemy;

    public Vector3 endPoint;

    public string enemyName;

    // Start is called before the first frame update
    void Start() {
        SpawnEnemy();
    }


    public IEnumerator RespawnAfterWait() {
        float seconds = Random.Range(3.0f, 5.0f);
        yield return new WaitForSeconds(seconds);
        SpawnEnemy();
    }

    private void SpawnEnemy() {
        associatedEnemy = Instantiate(GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().spawnPrefabs.Find(prefab => prefab.name == enemyName));

        if (enemyName == "EnemyFlying") {
            associatedEnemy.GetComponent<EnemyFly>().startWaypoint = transform.position;
            associatedEnemy.GetComponent<EnemyFly>().endWaypoint = endPoint;
        }

        associatedEnemy.GetComponent<EnemyVariables>().associatedSpawner = gameObject;

        associatedEnemy.transform.position = transform.position;

        NetworkServer.Spawn(associatedEnemy);
    }


}
