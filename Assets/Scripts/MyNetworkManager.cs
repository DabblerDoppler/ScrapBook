using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class MyNetworkManager : NetworkManager {

    GameObject starManager;
    GameObject spawn;

    public override void OnStartServer() {
        base.OnStartServer();

        startPositions.Add(GameObject.Find("PlayerSpawn").transform);
        startPositions.Add(GameObject.Find("PlayerSpawn (1)").transform);

        starManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "StarManager"));
        NetworkServer.Spawn(starManager);
        starManager.name = "StarManager";

        GameObject[] enemySpawns = GameObject.FindGameObjectsWithTag("EnemySpawn");
        Debug.Log("Spawn point found: " + enemySpawns[0]);
        foreach (GameObject spawnPoint in enemySpawns) {
            spawn = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "EnemySpawner"));
            spawn.transform.position = spawnPoint.transform.position;
            NetworkServer.Spawn(spawn);
        }

        GameObject[] platformSpawns = GameObject.FindGameObjectsWithTag("PlatformSpawn");

        foreach(GameObject spawnPoint in platformSpawns) {
            spawn = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "MovingPlatform"));
            spawn.transform.position = spawnPoint.transform.position;
            spawn.GetComponent<MovingPlatformController>().startWaypoint = spawnPoint.transform.position;
            spawn.GetComponent<MovingPlatformController>().startWaypoint = new Vector3(spawn.GetComponent<MovingPlatformController>().startWaypoint.x, spawn.GetComponent<MovingPlatformController>().startWaypoint.y, 0);
            spawn.GetComponent<MovingPlatformController>().endWaypoint = spawnPoint.transform.GetChild(0).position;
            spawn.GetComponent<MovingPlatformController>().endWaypoint = new Vector3(spawn.GetComponent<MovingPlatformController>().endWaypoint.x, spawn.GetComponent<MovingPlatformController>().endWaypoint.y, 0);
            NetworkServer.Spawn(spawn);
        }

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
    }

    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);
        GameObject.Find("WinScreen").GetComponent<Text>().text = "";
    }


    
}



