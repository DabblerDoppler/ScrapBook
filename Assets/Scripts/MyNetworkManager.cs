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
    }


    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);
        if (numPlayers == 2) {
            starManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "StarManager"));
            NetworkServer.Spawn(starManager);
            starManager.name = "StarManager";
        }
    }



}
