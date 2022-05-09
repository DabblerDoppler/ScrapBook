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
        GameObject associatedSpawn = starList[rnd.Next(0, starList.Count)];
        currentStar.transform.position = associatedSpawn.GetComponent<Transform>().position;
        currentStar.transform.SetParent(associatedSpawn.transform);
        NetworkServer.Spawn(currentStar);
    }

    public IEnumerator SpawnAfterSeconds(Vector3 position) {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 2.0f));
        CmdPickNewStar(position);
    }


    [Command(requiresAuthority = false)]
    void CmdPickNewStar(Vector3 position) {
        Transform temp = starList[rnd.Next(0, starList.Count)].transform;
        while (position == temp.position) {
            temp = starList[rnd.Next(0, starList.Count)].transform;
        }
        GameObject currentStar = Instantiate(GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().spawnPrefabs.Find(prefab => prefab.name == "Star"));
        currentStar.transform.position = temp.position;
        currentStar.transform.SetParent(temp);
        NetworkServer.Spawn(currentStar);
    }


}
