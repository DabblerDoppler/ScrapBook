using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DroppedStar : NetworkBehaviour {

    public float TimeUntilFall;
    public float TimeUntilUnignore;
    const float OUT_OF_BOUNDS = -200f;
    [SyncVar]
    public GameObject ignoredPlayer;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        TimeUntilUnignore -= Time.deltaTime;
        TimeUntilFall -= Time.deltaTime;

        if (TimeUntilUnignore < 0) {
            gameObject.layer = 7;
        }

        if(TimeUntilFall < 0) {
            gameObject.layer = 10;
        }

        if(transform.position.y < OUT_OF_BOUNDS) {
            Destroy(gameObject);
        }
    }
    
    public void playerTouch() {
        CmdDestroySelf();
        if (!isServer) { Destroy(gameObject); }
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
