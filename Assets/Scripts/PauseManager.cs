using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PauseManager : MonoBehaviour {
    
    [SerializeField]
    public GameObject inGameObject;
    [SerializeField]
    public GameObject pauseObject;
    [SerializeField]
    public GameObject settingsObject;
    [SerializeField]
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
