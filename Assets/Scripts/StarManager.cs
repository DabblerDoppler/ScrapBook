using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class StarManager : NetworkBehaviour {
    public GameObject[] starArray;
    public List<GameObject> starList;
    public GameObject currentStar;
    public int lastStar;
    System.Random rnd;

    // Start is called before the first frame update
    void Start() {
       rnd = new System.Random();

        starArray = new GameObject[200];
        starArray = FindObjectsOfType<GameObject>();

        starList = Enumerable.ToList<GameObject>(starArray);

        for(int i = 0; i < starList.Count; i++) {
            if(starList[i].tag != "Star") {
                Debug.Log("Removed" + starList[i]);
                starList.RemoveAt(i);
                i -= 1;
            } else {
                starList[i].SetActive(false);
            }
        }

        lastStar = rnd.Next(0, starList.Count);
        currentStar = starList[lastStar];

        currentStar.SetActive(true);


    }

    // Update is called once per frame
    void Update() {
        if(currentStar == null) {
            int star = rnd.Next(0, starList.Count);
            currentStar = starList[star];
            while(lastStar == star) {
                star = rnd.Next(0, starList.Count);
                currentStar = starList[star];
            }

            currentStar.SetActive(true);
        }
    

        
    }
}
