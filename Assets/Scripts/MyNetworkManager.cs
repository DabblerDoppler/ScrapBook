using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager {

    GameObject starManager;

    public override void OnStartServer() {
        base.OnStartServer();

        startPositions.Add(GameObject.Find("PlayerSpawn").transform);
        startPositions.Add(GameObject.Find("PlayerSpawn (1)").transform);
    }


    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);
        //change to 2 when done testing
        if (numPlayers == 1) {
            starManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "StarManager"));
            NetworkServer.Spawn(starManager);
            starManager.name = "StarManager";
        }
    }



}
