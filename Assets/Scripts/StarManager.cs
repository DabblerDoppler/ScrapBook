using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class StarManager : NetworkBehaviour {
    public List<GameObject> starList;
    public GameObject currentStar;
    public System.Random rnd;

    [Server]
    void Start() {
        rnd = new System.Random();
        starList = FindObjectsOfType<GameObject>().ToList<GameObject>();
        for (int i = 0; i < starList.Count; i++) {
            if (starList[i].tag != "StarSpawn") {
                Debug.Log("Removed" + starList[i]);
                starList.RemoveAt(i);
                i -= 1;
            }
        }
        currentStar = Instantiate(GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().spawnPrefabs.Find(prefab => prefab.name == "Star"));
        NetworkServer.Spawn(currentStar);
        currentStar.transform.position = starList[rnd.Next(0, starList.Count)].GetComponent<Transform>().position;
    }






}
