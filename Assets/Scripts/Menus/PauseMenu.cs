using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class PauseMenu : NetworkBehaviour {

    public GameObject inGameCanvas;
    public GameObject mainMenuCanvas;
    
    void Start() {
        gameObject.SetActive(false);
    }

    public void Resume() {
        NetworkClient.localPlayer.GetComponent<Player>().pauseInputs = false;
    }


    public void Quit() {
        if(NetworkClient.isConnected) {
            NetworkClient.Disconnect();
            NetworkClient.Shutdown();
        } 
        
        if (NetworkServer.active) {
            NetworkServer.Shutdown();
        }
        Destroy(GameObject.Find("NetworkManager"));
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);

        mainMenuCanvas.SetActive(true);
        //inGameCanvas.SetActive(false);

    }


}
