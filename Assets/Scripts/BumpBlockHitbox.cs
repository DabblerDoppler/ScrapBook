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
            if (!isServer) { Destroy(gameObject); }
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
