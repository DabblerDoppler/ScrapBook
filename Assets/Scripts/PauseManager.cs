using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PauseManager : MonoBehaviour {

    public GameObject inGameObject;
    public GameObject pauseObject;


    void Start() {
    }
    void Update() {
        if(Input.GetButtonDown("Start")) {
            inGameObject.SetActive(!inGameObject.activeInHierarchy);
            pauseObject.SetActive(!pauseObject.activeInHierarchy);
            NetworkClient.localPlayer.gameObject.GetComponent<Player>().pauseInputs = 
                !NetworkClient.localPlayer.gameObject.GetComponent<Player>().pauseInputs;
        }
    }
}
