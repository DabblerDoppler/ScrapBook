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
            tag = "Untagged";
            gameObject.layer = LayerMask.NameToLayer("Floor");
            GetComponent<SpriteRenderer>().color = Color.white;
        } else {
            tag = "Spike";
            gameObject.layer = LayerMask.NameToLayer("Spike");
            GetComponent<SpriteRenderer>().color = Color.red;
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
}
