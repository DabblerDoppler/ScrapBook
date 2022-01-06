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

    [SerializeField] private int minPlayers = 1;

    [Header("Scene Management")]
    [Scene] [SerializeField] public string menuScene = string.Empty;
    [Scene] [SerializeField] public string[] levels = new string[2];
    public Dropdown levelSelect;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab;

    [SerializeField] private GameObject mainMenuCanvas;

    [Header("Game")]
    [SerializeField] private Player gamePlayerPrefab;

    public static event Action OnClientConnected = delegate{ };

    public static event Action ClientDisconnected = delegate{ };

    public List<NetworkRoomPlayerLobby> RoomPlayers {get;} = new List<NetworkRoomPlayerLobby>();
    
    public List<Player> GamePlayers { get; } = new List<Player>();


    [SerializeField]
    

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        OnClientConnected.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        ClientDisconnected.Invoke();
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
    



    public override void OnStartServer() {

        base.OnStartServer();

    }


    public override void OnServerAddPlayer(NetworkConnection conn) {
        //base.OnServerAddPlayer(conn);

        if(SceneManager.GetActiveScene().path == menuScene) {
            bool isLeader = RoomPlayers.Count == 0;

            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);

            NetworkServer.Spawn(roomPlayerInstance.gameObject, conn);

            roomPlayerInstance.isLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }

    }
            //this stuff should happen when we move from lobby to game
    /*
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

    public override void OnStopServer() {
        RoomPlayers.Clear();
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        if(conn.identity != null) {
            var player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();

            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public void NotifyPlayersOfReadyState() {
        foreach(var player in RoomPlayers) {
            //this true can be changed to different logic to prevent the leader from starting
            //without everyone readying
            player.HandleReadyToStart(true);
        }
    }


    public void StartGame() {
        ServerChangeScene(levels[levelSelect.value]);
    }

    //something isn't working here
    public override void ServerChangeScene(string newSceneName) {
        base.ServerChangeScene(newSceneName);

        Debug.Log("Calling spawn after load");
        StartCoroutine(SpawnAfterLoad());
    }

    IEnumerator SpawnAfterLoad() {
        Debug.Log("waiting");
        yield return new WaitForSeconds(0.1f);
        Debug.Log("done waiting");


        GameObject[] playerSpawns = GameObject.FindGameObjectsWithTag("Respawn");
        foreach (GameObject spawn in playerSpawns) {
            startPositions.Add(spawn.transform);
        }

        for (int i = RoomPlayers.Count - 1; i >= 0; i--) {
                var conn = RoomPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);
                gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);

                Debug.Log("Player spawned." + gamePlayerInstance.gameObject);
                gamePlayerInstance.gameObject.transform.position = GetStartPosition().position;
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

    }


    public override void OnServerChangeScene(String newSceneName) {

        base.OnServerChangeScene(newSceneName);

        if(mode == NetworkManagerMode.ClientOnly) {

            GameObject player = GameObject.Find("LobbyPlayer(Clone)");
            while(player != null) {
                NetworkServer.Destroy(player);
                player = GameObject.Find("LobbyPlayer(Clone)");
            }
            mainMenuCanvas.SetActive(false);
        }


    }
}



