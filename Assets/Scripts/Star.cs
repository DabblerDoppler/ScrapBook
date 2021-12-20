using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Star : NetworkBehaviour {
    ContactFilter2D contactFilter;

    public GameObject starMapObject;
    private GameObject myStarMapObject;


    // When spawned, start using a layer mask so only players can touch it.
    void Awake() {
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = 3;

        myStarMapObject = Instantiate(starMapObject, new Vector3(0, 0, 0), Quaternion.identity);
        myStarMapObject.GetComponent<MapObject>().associatedTransform = transform;
        
    }

    // Update is called once per frame
    void Update() {
    }

    private void OnDestroy() {
        Destroy(myStarMapObject);
    }

    //When called (from a player), send a command to the server.
    public void playerTouch() {
        if (isServer) {
            GameObject.Find("StarManager").GetComponent<StarManager>().StartCoroutine(GameObject.Find("StarManager").GetComponent<StarManager>().SpawnAfterSeconds(transform.position));
            DestroySelf();
        } else {
            CmdDestroySelf();
            Destroy(gameObject);
        }
    }

    [Command(requiresAuthority =false)]
    public void CmdDestroySelf() {
        GameObject.Find("StarManager").GetComponent<StarManager>().StartCoroutine(GameObject.Find("StarManager").GetComponent<StarManager>().SpawnAfterSeconds(transform.position));
        DestroySelf();
    }


    [ClientRpc]
    private void DestroySelf() {
        Destroy(gameObject);
    }


}
