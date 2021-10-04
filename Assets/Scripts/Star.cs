using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Star : NetworkBehaviour {
    ContactFilter2D contactFilter;



    // When spawned, start using a layer mask so only players can touch it.
    void Awake() {
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = 3;
    }

    // Update is called once per frame
    void Update() {
    }

    //When called (from a player), send a command to the server.
    public void playerTouch() {
        CmdPickNewStar();
        if (!isServer) {
            Destroy(gameObject);
        }
    }



    [Command(requiresAuthority=false)] 
    void CmdPickNewStar() {
        StarManager starMgr = GameObject.Find("StarManager").GetComponent<StarManager>();
        Vector3 temp = starMgr.starList[starMgr.rnd.Next(0, starMgr.starList.Count)].transform.position;
        while(transform.position == temp) {
            temp = starMgr.starList[starMgr.rnd.Next(0, starMgr.starList.Count)].transform.position;
        }

        GameObject currentStar = Instantiate(GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().spawnPrefabs.Find(prefab => prefab.name == "Star"));
        NetworkServer.Spawn(currentStar);
        //currentStar.transform.position = temp;

        DestroySelf(currentStar, temp);


    }

    [ClientRpc]
    private void DestroySelf(GameObject nextStar, Vector3 temp) {
        nextStar.transform.position = temp;
        Destroy(gameObject);
    }


}
