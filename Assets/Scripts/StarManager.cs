using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class StarManager : NetworkBehaviour {
    public List<GameObject> starList;
    public System.Random rnd;

    //When spawned (which only happens on the server), make a list of all the star spawn points. 
    //Then, spawn the first star (using mirror's spawning syntax) and set its position to a random
    //spawn point from our list.
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
        GameObject currentStar = Instantiate(GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().spawnPrefabs.Find(prefab => prefab.name == "Star"));
        NetworkServer.Spawn(currentStar);
        currentStar.transform.position = starList[rnd.Next(0, starList.Count)].GetComponent<Transform>().position;
    }

    public IEnumerator SpawnAfterSeconds(Vector3 position) {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 4.0f));
        CmdPickNewStar(position);
    }


    [Command(requiresAuthority = false)]
    void CmdPickNewStar(Vector3 position) {
        Vector3 temp = starList[rnd.Next(0, starList.Count)].transform.position;
        while (position == temp) {
            temp = starList[rnd.Next(0, starList.Count)].transform.position;
        }
        GameObject currentStar = Instantiate(GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().spawnPrefabs.Find(prefab => prefab.name == "Star"));
        currentStar.transform.position = temp;
        NetworkServer.Spawn(currentStar);
    }


}
