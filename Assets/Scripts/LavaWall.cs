using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LavaWall : NetworkBehaviour {

    [SyncVar]
    bool isFloor = true;

    [SyncVar]
    float timeUntilChange = 3.0f;

    const float CHANGE_TIME = 3.0f;


    // Update is called once per frame
    void Update() {
        if(isFloor) {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        } else {
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
        }

        if(isServer) {
            timeUntilChange -= Time.deltaTime;
            
            if(timeUntilChange <= 0) {
                timeUntilChange = CHANGE_TIME;
                if(!isFloor) {
                    isFloor = true;
                } else {
                    isFloor = false;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(col.GetComponent<Player>()){
            if(col.GetComponent<Player>().intangibility <= 0.0f) {
                Debug.Log("Trigger");
                if(isServer) {
                    col.GetComponent<Player>().RpcKnockdown_Spike();
                } 
            }
        }
    }
    void OnTriggerStay2D(Collider2D col) {
        if(col.GetComponent<Player>()){
            if(col.GetComponent<Player>().intangibility <= 0.0f) {
                Debug.Log("Trigger");
                if(isServer) {
                    col.GetComponent<Player>().RpcKnockdown_Spike();
                } 
            }
        }
    }

}
