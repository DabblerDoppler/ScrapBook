using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class HostScript : NetworkBehaviour {

    public NetworkManager networkManager;
    public Canvas canvas;

    //this is not an ideal implementation, but since the custom host function needs to get
    //objects in the scene, we load the scene, wait a second, then host.
    public void Host() {
        SceneManager.LoadSceneAsync("Level1", LoadSceneMode.Single);
        StartCoroutine(HostAfterSecond());
    }

    public IEnumerator HostAfterSecond() {
        yield return new WaitForSeconds(0.25f);
        networkManager.StartHost();
        canvas.gameObject.SetActive(false);
    }

}
