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
        SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        networkManager.StartClient();
        StartCoroutine(JoinAfterSecond());
    }

    public IEnumerator JoinAfterSecond() {
        yield return new WaitForSeconds(1.0f);
        if(!NetworkClient.isConnected) {
            networkManager.StopClient();
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        } else {
        canvas.gameObject.SetActive(false);
        }
    } 

}
