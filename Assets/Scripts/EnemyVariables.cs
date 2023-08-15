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

    void OnTriggerEnter2D(Collider2D col) {
        if(col.GetComponent<Player>()){
            if(col.GetComponent<Player>().intangibility <= 0.0f && spawnIntangibility <= 0.0f) {
                Debug.Log(spawnIntangibility);
                if(isServer) {
                    col.GetComponent<Player>().RpcKnockdown_Spike();
                }
            }
        }
    }
    void OnTriggerStay2D(Collider2D col) {
        if(col.GetComponent<Player>()){
            if(col.GetComponent<Player>().intangibility <= 0.0f  && spawnIntangibility <= 0.0f) {
                Debug.Log(spawnIntangibility);
                if(isServer) {
                    col.GetComponent<Player>().RpcKnockdown_Spike();
                }
            }
        }
    }


}
