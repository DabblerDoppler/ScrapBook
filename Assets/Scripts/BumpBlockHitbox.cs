using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class BumpBlockHitbox : NetworkBehaviour {
    
    float framesRemaining;

    // Start is called before the first frame update
    void Start() {
        framesRemaining = 0.1f;
    }

    // Update is called once per frame
    void Update() {
        framesRemaining -= Time.deltaTime;
        if(framesRemaining <= 0) {
            CmdDestroySelf();
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

    [Command(requiresAuthority = false)]
    void CmdDestroySelf() {
        RpcDestroySelf();
    }


    [ClientRpc]
    private void RpcDestroySelf() {
        Destroy(gameObject);
    }


}
