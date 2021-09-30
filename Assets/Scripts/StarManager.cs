using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class StarManager : NetworkBehaviour {
    public GameObject[] starArray;
    public List<GameObject> starList;

    //when a SyncVar is changed on the server, it also changes for all clients.
    [SyncVar]
    public GameObject currentStar;
    public GameObject lastStar;
    System.Random rnd;

    // Start is called before the first frame update
    void Start() {
        if (isServer) {
            rnd = new System.Random();
            starArray = new GameObject[200];
            starArray = FindObjectsOfType<GameObject>();
            starList = Enumerable.ToList<GameObject>(starArray);
            for (int i = 0; i < starList.Count; i++) {
                if (starList[i].tag != "Star") {
                    Debug.Log("Removed" + starList[i]);
                    starList.RemoveAt(i);
                    i -= 1;
                }
                else {
                    starList[i].SetActive(false);
                }
            }
                currentStar = starList[rnd.Next(0, starList.Count)];
                currentStar.SetActive(true);
        }


    }

    // Update is called once per frame
    void Update() {
        for(int i = 0; i< starList.Count; i++) {
            if(starList[i] != currentStar) {
                starList[i].SetActive(false);
            } else {
                starList[i].SetActive(true);
            }
        }

        if(currentStar == null) {
            pickNewStar();
        }


    }

    //this runs on the server, setting the syncVar currentStar, which then changes it for all the clients.
    [Command(requiresAuthority = false)]
    public void pickNewStar() {
        lastStar = currentStar;
        int temp = rnd.Next(0, starList.Count);
        currentStar = starList[temp];
        if (lastStar == currentStar) {
            currentStar = starList[rnd.Next(0, starList.Count)];
        }
        currentStar.SetActive(true);
    }






}
