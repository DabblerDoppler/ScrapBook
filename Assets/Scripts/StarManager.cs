using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class StarManager : NetworkBehaviour {
    

    //When spawned (which only happens on the server), make a list of all the star spawn points. 
    //Then, spawn the first star (using mirror's spawning syntax) and set its position to a random
    //spawn point from our list.
    [Server]
    public void Begin() {
        MyNetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>();
        GameObject currentStar = Instantiate(networkManager.spawnPrefabs.Find(prefab => prefab.name == "Star"));
        int chosen = networkManager.rnd.Next(0, networkManager.starList.Count);
        GameObject associatedSpawn = networkManager.starList[chosen];
        currentStar.transform.position = associatedSpawn.GetComponent<Transform>().position;
        currentStar.transform.SetParent(associatedSpawn.transform);
        currentStar.name = "FirstStar";
        NetworkServer.Spawn(currentStar);
        Debug.Log("Calling RpcSetParent");
        RpcSetParent(currentStar, chosen);
    }


    [ClientRpc]
    private void RpcSetParent(GameObject currentStar, int chosen) {
        Debug.Log("Chosen is " + chosen);
        GameObject associatedSpawn = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().starList[chosen];
        currentStar.transform.position = associatedSpawn.GetComponent<Transform>().position;
        currentStar.transform.SetParent(associatedSpawn.transform);
        Debug.Log("Moved star to " + associatedSpawn);
    }

    public IEnumerator SpawnAfterSeconds(Vector3 position) {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 2.0f));
        CmdPickNewStar(position);
    }


    [Command(requiresAuthority = false)]
    void CmdPickNewStar(Vector3 position) {
        MyNetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>();
        int temp = networkManager.rnd.Next(0, networkManager.starList.Count);
        while (position == networkManager.starList[networkManager.rnd.Next(0, networkManager.starList.Count)].transform.position) {
            temp = networkManager.rnd.Next(0, networkManager.starList.Count);
        }
        GameObject currentStar = Instantiate(networkManager.spawnPrefabs.Find(prefab => prefab.name == "Star"));
        currentStar.transform.position = networkManager.starList[networkManager.rnd.Next(0, networkManager.starList.Count)].transform.position;
        currentStar.transform.SetParent(networkManager.starList[networkManager.rnd.Next(0, networkManager.starList.Count)].transform);
        NetworkServer.Spawn(currentStar);
        RpcSetParent(currentStar, temp);
    }


}
