using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnSystem : NetworkBehaviour {


    [SerializeField] private GameObject playerPrefab = null;

    private static List<Transform> playerSpawns = new List<Transform>();

    public GameObject playerMapObject;

    private int nextPlayerIndex = 0;

    public static void AddPlayerSpawn(Transform transform) {
        playerSpawns.Add(transform);
    }

    public static void RemovePlayerSpawn(Transform transform) {
        playerSpawns.Remove(transform);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        MyNetworkManager.OnServerReadied += SpawnPlayer;
    }

    [ServerCallback]
    private void OnDestroy() {
        MyNetworkManager.OnServerReadied -= SpawnPlayer;
    }

    [Server]
    public void SpawnPlayer(NetworkConnection conn) {
        foreach(Player p in GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().GamePlayers) {
            Transform spawn = playerSpawns[nextPlayerIndex];

            if(spawn == null) {
                Debug.LogError("Missing spawn point");
                return;
            }
            GameObject playerInstance = p.gameObject;
            //= conn.identity.gameObject;
            playerInstance.transform.position = spawn.position;
            NetworkServer.Spawn(playerInstance, conn);
            playerInstance.GetComponent<Player>().myMapObject = Instantiate(playerMapObject, new Vector3(0, 0, 0), Quaternion.identity);
            playerInstance.GetComponent<Player>().myMapObject.GetComponent<MapObject>().associatedTransform = playerInstance.transform;
            nextPlayerIndex++;
        }
    }


}
