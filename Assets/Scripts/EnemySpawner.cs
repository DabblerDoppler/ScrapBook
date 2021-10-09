using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemySpawner : NetworkBehaviour {

    GameObject associatedEnemy;

    public string enemyName;

    // Start is called before the first frame update
    void Start() {
        SpawnEnemy();
    }


    public IEnumerator RespawnAfterWait() {
        float seconds = Random.RandomRange(3.0f, 5.0f);
        yield return new WaitForSeconds(seconds);
        SpawnEnemy();
    }

    private void SpawnEnemy() {
        associatedEnemy = Instantiate(GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().spawnPrefabs.Find(prefab => prefab.name == enemyName));
        associatedEnemy.transform.position = transform.position;
        associatedEnemy.GetComponent<EnemyMove>().associatedSpawner = gameObject;
        NetworkServer.Spawn(associatedEnemy);
    }


}
