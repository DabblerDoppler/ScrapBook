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
        StartCoroutine(WaitAndSetUI(0.25f, conn));
    }

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);
        StartCoroutine(WaitAndSetUI(0.25f, conn));
    }
    //janky as fuck but you know me

    IEnumerator WaitAndSetUI(float seconds, NetworkConnection conn) {
        yield return new WaitForSeconds(seconds);

        List<GameObject> childList = GameObject.Find("Canvas").GetComponent<TextChildArray>().myChildren;
        GameObject[] playerArray = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerArray.Length; i++) {
            childList[i].GetComponent<StarUI>().attachedPlayer = playerArray[i].GetComponent<Player>();
        }


        /*
        GameObject player = conn.identity.gameObject;

        foreach (GameObject child in GameObject.Find("Canvas").GetComponent<TextChildArray>().myChildren) {
            if (child.GetComponent<StarUI>().attachedPlayer == null) {
                child.GetComponent<StarUI>().SetPlayer(player.GetComponent<Player>());
                break;
            }
        }
        */

    }
    
}



