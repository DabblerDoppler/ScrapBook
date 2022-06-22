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
            //make cage
            Debug.Log("PlayerTouch Called for server");
            Cage cage = gameObject.transform.parent.gameObject.transform.Find("Cage").GetComponent<Cage>();
            MakeCage(cage);
            DestroySelf();
        } else {
            //make cage
            Debug.Log("PlayerTouch Called for client");
            CmdMakeCage();
            CmdDestroySelf();
            Destroy(gameObject);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroySelf() {
        DestroySelf();
    }

    [Command(requiresAuthority = false)]
    public void CmdMakeCage() {
        /*
        Debug.Log("Making Cage");
        Cage cage = gameObject.transform.parent.gameObject.transform.Find("Cage").GetComponent<Cage>();
        cage.timeRemaining = cage.timeMaximum;
        cage.starPosition = transform;
        cage.hasDisappeared = false;
        */
        Cage cage = gameObject.transform.parent.gameObject.transform.Find("Cage").GetComponent<Cage>();
        MakeCage(cage);
    }

    [ClientRpc]
    private void MakeCage(Cage cage) {
        Debug.Log("Making Cage");
        cage.timeRemaining = cage.timeMaximum;
        cage.starPosition = transform.position;
        cage.hasDisappeared = false;
    }


    [ClientRpc]
    private void DestroySelf() {
        Debug.Log("Destorying Self");
        Destroy(gameObject);
    }


}
