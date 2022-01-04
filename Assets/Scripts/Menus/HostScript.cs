using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class HostScript : NetworkBehaviour {

    public NetworkManager networkManager;

    public GameObject lobbyMenu;
    public GameObject mainMenu;

    public Canvas canvas;

    //this is not an ideal implementation, but since the custom host function needs to get
    //objects in the scene, we load the scene, wait a second, then host.
    public void HostLevel1() {
        SceneManager.LoadSceneAsync("Level1", LoadSceneMode.Single);
        StartCoroutine(HostAfterSecond());
    }

    public void HostLevel2() {
        SceneManager.LoadSceneAsync("Level2", LoadSceneMode.Single);
        StartCoroutine(HostAfterSecond());
    }

    public void StopHost() {
        networkManager.StopHost();
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);

    }

    public IEnumerator HostAfterSecond() {
        yield return new WaitForSeconds(0.25f);
        networkManager.StartHost();
        canvas.gameObject.SetActive(false);
    }

}
