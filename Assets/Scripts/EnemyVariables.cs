using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyVariables : NetworkBehaviour {

    public float spawnIntangibility;
    public GameObject associatedSpawner;

    private void Awake() {
        spawnIntangibility = 1.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        spawnIntangibility -= Time.deltaTime;
    }

    private void OnDestroy() {
        if (isServer) {
            associatedSpawner.GetComponent<EnemySpawner>().StartCoroutine(associatedSpawner.GetComponent<EnemySpawner>().RespawnAfterWait());
        }
    }
}
