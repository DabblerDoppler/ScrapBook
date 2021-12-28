using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PauseManager : MonoBehaviour {

    public GameObject inGameObject;
    public GameObject pauseObject;

    public GameObject settingsObject;
    public GameObject teamsObject;


    void Start() {
    }
    void Update() {
        if(Input.GetButtonDown("Start")) {
            if(pauseObject.activeInHierarchy || settingsObject.activeInHierarchy || teamsObject.activeInHierarchy) {
                pauseObject.SetActive(false);
                settingsObject.SetActive(false);
                teamsObject.SetActive(false);
                NetworkClient.localPlayer.GetComponent<Player>().pauseInputs = false;
                inGameObject.SetActive(true);
            } else {
                pauseObject.SetActive(true);
                settingsObject.SetActive(false);
                teamsObject.SetActive(false);
                inGameObject.SetActive(false);
                NetworkClient.localPlayer.GetComponent<Player>().pauseInputs = true;
            }
        }
    }
}
