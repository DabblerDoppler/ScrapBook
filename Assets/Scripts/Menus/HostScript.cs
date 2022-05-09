using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HostScript : MonoBehaviour {

    public NetworkManager networkManager;

    public GameObject mainMenuCanvas;
    public GameObject levelSelect;
    public GameObject levelSelectText;
    public GameObject startButton;
    public Text backButtonText;

    public GameObject lobbyMenu;
    public GameObject mainMenu;
    public Canvas canvas;

    const string CLIENT_BACK_TEXT = "Leave Lobby";
    const string SERVER_BACK_TEXT = "Disband Lobby";

    public void SetClient() {
            levelSelectText.SetActive(false);
            levelSelect.SetActive(false);
            startButton.SetActive(false);
            backButtonText.text = CLIENT_BACK_TEXT;
    }

    public void SetServer() {
        levelSelectText.SetActive(true);
        levelSelect.SetActive(true);
        startButton.SetActive(true);
        backButtonText.text = SERVER_BACK_TEXT;
    }

    //this is not an ideal implementation, but since the custom host function needs to get
    //objects in the scene, we load the scene, wait a second, then host.

    public void StopHost() {
        networkManager.StopHost();
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);

    }


    /*
    public IEnumerator HostAfterSecond() {
        yield return new WaitForSeconds(0.25f);
        networkManager.StartHost();
        canvas.gameObject.SetActive(false);
    }
    */

    public void StartGame() {
        Debug.Log("Game Starting...");
        GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().StartGame();
        mainMenuCanvas.SetActive(false);
        NetworkClient.localPlayer.gameObject.GetComponent<NetworkRoomPlayerLobby>().RpcDisableCanvas(mainMenuCanvas);
    }

}
