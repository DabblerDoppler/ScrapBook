using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class JoinLobbyMenu : MonoBehaviour {

    [SerializeField] private MyNetworkManager networkManager = null;

    [Header("UI")]

    [SerializeField] private GameObject joinScreen;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject lobbyMenu;
    [SerializeField] private InputField ipInputField = null;
    [SerializeField] private Button joinButton = null;

    private void OnEnable() {
        MyNetworkManager.OnClientConnected += HandleClientConnected;
        MyNetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable() {
        MyNetworkManager.OnClientConnected -= HandleClientDisconnected;
        MyNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby() {
        string ipAddress = ipInputField.text;

        if(ipAddress.Equals("")) {
            ipAddress = "localhost";
        }

        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected() {
        joinButton.interactable = true;

        lobbyMenu.SetActive(true);
        mainMenu.SetActive(false);
        joinScreen.SetActive(false);
    }

    private void HandleClientDisconnected() {
        joinButton.interactable = true;
    }

}
