using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using kcp2k;

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
        MyNetworkManager.ClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable() {
        MyNetworkManager.OnClientConnected -= HandleClientDisconnected;
        MyNetworkManager.ClientDisconnected -= HandleClientDisconnected;
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

    public void BackButton() {
        GameObject.Find("NetworkManager").GetComponent<KcpConnection>().Disconnect();
    }

    public void HandleClientConnected() {

        Debug.Log("client connected");
        joinButton.interactable = true;

        lobbyMenu.SetActive(true);

        if(NetworkServer.active) {
            lobbyMenu.GetComponent<HostScript>().SetServer();
        } else {
        lobbyMenu.GetComponent<HostScript>().SetClient();
        }

        mainMenu.SetActive(false);
        joinScreen.SetActive(false);
    }

    private void HandleClientDisconnected() {
        joinButton.interactable = true;
    }

}
