using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MyNetworkManager : NetworkManager {

    GameObject starManager;
    GameObject spawn;

    public int teams = 1;

    [SerializeField] private Scene menuScene;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;



    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        OnClientConnected.Invoke();
    }

    
    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        OnClientDisconnected.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn) {
        base.OnServerConnect(conn);

        if(numPlayers >= maxConnections) {
            conn.Disconnect();
            return;
        } 

/*
        //this prevents people from joining mid-match 
        if(SceneManager.GetActiveScene() != menuScene) {
            conn.Disconnect();
            return;
        }
        
    */
    }
    

    public override void OnServerAddPlayer(NetworkConnection conn) {
        base.OnServerAddPlayer(conn);

        if(SceneManager.GetActiveScene() == menuScene) {
            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }

    }


    public override void OnStartServer() {

        base.OnStartServer();

        //this stuff should happen when we move from lobby to game
        /*
        GameObject[] playerSpawns = GameObject.FindGameObjectsWithTag("Respawn");

        foreach (GameObject spawn in playerSpawns) {
            startPositions.Add(spawn.transform);
        }

        starManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "StarManager"));
        NetworkServer.Spawn(starManager);
        starManager.name = "StarManager";

        GameObject[] enemySpawns = GameObject.FindGameObjectsWithTag("EnemySpawn");
        Debug.Log("Spawn point found: " + enemySpawns[0]);
        foreach (GameObject spawnPoint in enemySpawns) {
            spawn = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "EnemySpawner"));
            spawn.GetComponent<EnemySpawner>().enemyName = "Enemy1";
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


        GameObject[] flySpawns = GameObject.FindGameObjectsWithTag("FlySpawn");

        foreach (GameObject spawnPoint in flySpawns) {
            spawn = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "EnemySpawner"));
            spawn.GetComponent<EnemySpawner>().enemyName = "EnemyFlying";
            spawn.transform.position = spawnPoint.transform.position;

            spawn.GetComponent<EnemySpawner>().endPoint = spawnPoint.transform.GetChild(0).position;
            spawn.GetComponent<EnemySpawner>().endPoint = new Vector3(spawn.GetComponent<EnemySpawner>().endPoint.x, spawn.GetComponent<EnemySpawner>().endPoint.y, 0);
            NetworkServer.Spawn(spawn);
        }
        */


    }

    //this stuff should happen when we move from lobby to game
    /*
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
    */

    
}



