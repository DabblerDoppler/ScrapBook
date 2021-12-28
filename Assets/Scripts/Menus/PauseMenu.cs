using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class PauseMenu : NetworkBehaviour {

    public GameObject inGameCanvas;
    public GameObject mainMenuCanvas;
    public Scene mainMenu;
    
    void Start() {
        gameObject.SetActive(false);
    }

    public void Quit() {
        if(NetworkClient.isConnected) {
            NetworkClient.Disconnect();
            NetworkClient.Shutdown();
        } else if (NetworkServer.active) {
            NetworkServer.Shutdown();
        }

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        mainMenuCanvas.SetActive(true);
        inGameCanvas.SetActive(false);

    }


}
