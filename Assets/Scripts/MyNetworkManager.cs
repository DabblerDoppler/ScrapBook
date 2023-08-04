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

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;

    public int teams = 1;

    [SerializeField] private int minPlayers = 1;

    [Header("Scene Management")]
    [Scene] [SerializeField] public string menuScene = string.Empty;
    [Scene] [SerializeField] public string[] levels = new string[2];
    public Dropdown levelSelect;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerLobby roomPlayerPrefab;
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject joinScreen;

    [Header("Game")]
    [SerializeField] private GameObject gamePlayerPrefab;
    [SerializeField] private GameObject spawnSystem;
    public GameObject playerMapObject;
    public List<GameObject> starList;
    public System.Random rnd;

    public static event Action ClientConnected = delegate{ };

    public static event Action ClientDisconnected = delegate{ };

    public List<NetworkRoomPlayerLobby> RoomPlayers {get;} = new List<NetworkRoomPlayerLobby>();
    
    public List<Player> GamePlayers { get; } = new List<Player>();


    [SerializeField]
    

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);

        if(SceneManager.GetActiveScene().name == "MainMenu") {
            NetworkClient.AddPlayer();

        }
        joinScreen.GetComponent<JoinLobbyMenu>().HandleClientConnected();
        ClientConnected.Invoke();
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

    public void StartGame() {
        ServerChangeScene(levels[levelSelect.value]);
    }

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
    




    public override void ServerChangeScene(string newSceneName) {
        for (int i = RoomPlayers.Count - 1; i >= 0; i--) {
                var conn = RoomPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);

                gamePlayerInstance.GetComponent<Player>().SetDisplayName(RoomPlayers[i].DisplayName);

                GamePlayers.Add(gamePlayerInstance.GetComponent<Player>());

                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
                Debug.Log("replaced player");
                Debug.Log("conn is " + conn.identity.name);
        }
        base.ServerChangeScene(newSceneName);
    }

    public override void OnClientSceneChanged(NetworkConnection conn) {
        mainMenuCanvas.SetActive(false);
            starList = FindObjectsOfType<GameObject>().ToList<GameObject>();
            for (int i = 0; i < starList.Count; i++) {
                if (starList[i].tag != "StarSpawn") {
                    Debug.Log("Removed" + starList[i]);
                    starList.RemoveAt(i);
                    i -= 1;
                }
            }
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player")) {
            player.GetComponent<Player>().myMapObject = Instantiate(playerMapObject, new Vector3(0, 0, 0), Quaternion.identity);
            player.GetComponent<Player>().myMapObject.GetComponent<MapObject>().associatedTransform = player.gameObject.transform;
            player.GetComponent<Player>().myMapObject.GetComponent<Image>().color = player.GetComponent<Player>().colors[player.GetComponent<Player>().team];
        }

        base.OnClientSceneChanged(conn);
    }

    public override void OnServerSceneChanged(string sceneName) {
        if(sceneName != "MainMenu") {

            //we spawn players in ServerChangeScene, and then move them to start positions here
            GameObject[] playerSpawns = GameObject.FindGameObjectsWithTag("Respawn");
            foreach (GameObject spawn in playerSpawns) {
                startPositions.Add(spawn.transform);
            }

            for (int i = GamePlayers.Count - 1; i >= 0; i--) {
                var player = GamePlayers[i];
                player.gameObject.transform.position = GetStartPosition().position;
                player.myMapObject = Instantiate(playerMapObject, new Vector3(0, 0, 0), Quaternion.identity);
                player.myMapObject.GetComponent<MapObject>().associatedTransform = player.gameObject.transform;
                player.GetComponent<Player>().myMapObject.GetComponent<Image>().color = player.GetComponent<Player>().colors[player.GetComponent<Player>().team];

            }






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

            starList = FindObjectsOfType<GameObject>().ToList<GameObject>();
            for (int i = 0; i < starList.Count; i++) {
                if (starList[i].tag != "StarSpawn") {
                    Debug.Log("Removed" + starList[i]);
                    starList.RemoveAt(i);
                    i -= 1;
                }
            }

            rnd = new System.Random();
            starManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "StarManager"));
            NetworkServer.Spawn(starManager);
            starManager.name = "StarManager";
        }

        StartCoroutine(BeginGame());


        base.OnServerSceneChanged(sceneName);
    }

    public IEnumerator BeginGame() {
        yield return new WaitForSeconds(2.0f);
        GameObject.Find("StarManager").GetComponent<StarManager>().Begin();
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

    public override void OnServerReady(NetworkConnection conn) {
        base.OnServerReady(conn);
        OnServerReadied?.Invoke(conn);
    }

}



