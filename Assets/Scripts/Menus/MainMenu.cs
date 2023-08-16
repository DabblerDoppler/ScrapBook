using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainMenu : MonoBehaviour {

    public GameObject lobby;
    public GameObject mainMenu;
    public GameObject joinMenu;


    public NetworkManager networkManager;

    public void Host() {
        if(!NetworkServer.active) {
            networkManager.StartHost();
        }
        if(NetworkServer.active) {
            lobby.SetActive(true);
            joinMenu.SetActive(false);
            mainMenu.SetActive(false);
        }
    }

    public void Quit() {
        Debug.Log("QUITING");
        Application.Quit();
    }
}
