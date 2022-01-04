using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class JoinScript : NetworkBehaviour {

    public NetworkManager networkManager;
    public Canvas canvas;
    public GameObject pauseManager;

    public void SetIP(string ip) {
        networkManager.networkAddress = ip;
    }


    public void Join() {
        networkManager.StartClient();
    }

}
