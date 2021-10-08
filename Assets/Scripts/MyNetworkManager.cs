using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager {

    GameObject starManager;
    GameObject spawn;

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
        if (numPlayers == 2) {
            starManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "StarManager"));
            NetworkServer.Spawn(starManager);
            starManager.name = "StarManager";
        }

        GameObject[] enemySpawns = GameObject.FindGameObjectsWithTag("EnemySpawn");

        Debug.Log("Spawn point found: " + enemySpawns[0]);

        foreach(GameObject spawnPoint in enemySpawns) {
            spawn = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "EnemySpawner"));
            spawn.transform.position = spawnPoint.transform.position;
            NetworkServer.Spawn(spawn);
        }




    }

    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);
    }


    
}



