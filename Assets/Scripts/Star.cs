using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Star : NetworkBehaviour {
    ContactFilter2D contactFilter;



    // Start is called before the first frame update
    void Awake() {
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = 3;
    }

    // Update is called once per frame
    void Update() {
    }

    //this might not work networked.
    public void playerTouch() {
        CmdPickNewStar();
    }

    [Command(requiresAuthority=false)] 
    void CmdPickNewStar() {
        StarManager starMgr = GameObject.Find("StarManager").GetComponent<StarManager>();
        Transform temp = starMgr.starList[starMgr.rnd.Next(0, starMgr.starList.Count)].transform;
        /*
        while(starMgr.currentStarTransform.transform.position == starMgr.lastStarTransform.transform.position) {
            starMgr.currentStarTransform = starMgr.starList[starMgr.rnd.Next(0, starMgr.starList.Count)];
        } */
        gameObject.transform.position = temp.position;
    }


}
