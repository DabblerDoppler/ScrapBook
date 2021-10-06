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
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);

        //change to 2 when done testing
        if (numPlayers == 1) {
            starManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "StarManager"));
            NetworkServer.Spawn(starManager);
            starManager.name = "StarManager";
        }

    }

    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);
    }


    
}



