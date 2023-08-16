using System;
using System.Collections;
using System.Collections.Generic;
using kcp2k;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour {

[Header("UI")]
[SerializeField] private InputField nameInputField = null;
[SerializeField] private Button continueButton = null;
[SerializeField] private InputField portInputField = null;

public static string DisplayName;
public static string Port;
public static ushort PortInt;
public const string PLAYER_PREFS_NAME_KEY = "PlayerName";
private const string DEFAULT_NAME = "Gamer";
public const string PLAYER_PREFS_PORT_KEY = "Port";
private const ushort DEFAULT_PORT = 30000;
bool nameBad;
bool portBad;




    // Start is called before the first frame update
    void Start() {
        SetUpNameInputField();
        SetUpPortInputField();
    }

    private void SetUpNameInputField() {
        if(!PlayerPrefs.HasKey(PLAYER_PREFS_NAME_KEY)) { 
            PlayerPrefs.SetString(PLAYER_PREFS_NAME_KEY, DEFAULT_NAME);
        }
        string defaultName = PlayerPrefs.GetString(PLAYER_PREFS_NAME_KEY);
        nameInputField.text = defaultName;
        SetPlayerName(defaultName);
    }

    private void SetUpPortInputField() {
        if(!PlayerPrefs.HasKey(PLAYER_PREFS_PORT_KEY)) { 
            PlayerPrefs.SetInt(PLAYER_PREFS_PORT_KEY, DEFAULT_PORT);
        }
        ushort defaultPort = (ushort)PlayerPrefs.GetInt(PLAYER_PREFS_PORT_KEY);
        portInputField.text = defaultPort.ToString();
        SetPort(defaultPort);
    }

    public void SetPlayerName(string name) {
        if(name.Length > 11 || name.Length == 0) {
            nameBad = true;
        } else {
            nameBad = false;
        }

        if(nameBad || portBad) {
            continueButton.interactable = false;
        } else {
            continueButton.interactable = true;
        }
    }
    public void SetPort(string port) {
        if(port.Length == 0) {
            return;
        }
        ushort tempPortInt;
        
        try {
            tempPortInt = (ushort)Int16.Parse(port);
            if(tempPortInt < 1024) {
                portBad = true;
                Debug.Log("Port is reserved!");
            } else {
                portBad = false;
            }
        } catch (FormatException) {
            portBad = true;
            Debug.Log("Port contains non numerical characters!");
        } catch (OverflowException) {
            portBad = true;
            Debug.Log("Port contains too large a number!");
        }
    

        if(nameBad || portBad) {
            continueButton.interactable = false;
        } else {
            continueButton.interactable = true;
        }
    }

    public void SetPort(ushort port) {
        if(port < 1024) {
            portBad = true;
            Debug.Log("Port is reserved!");
        } else {
            portBad = false;
        }

        if(nameBad || portBad) {
            continueButton.interactable = false;
        } else {
            continueButton.interactable = true;
        }
    }


    public void SavePlayerName() {
        DisplayName = nameInputField.text;
        PlayerPrefs.SetString(PLAYER_PREFS_NAME_KEY, DisplayName);
    }

    public void SavePort() {
        Port = portInputField.text;
        if(portInputField.text == "") {
            Port = "30000";
            PortInt = 30000;
        } else {
            PortInt = (ushort)Int16.Parse(Port);
        }
        PlayerPrefs.SetInt(PLAYER_PREFS_PORT_KEY, PortInt);
        GameObject.Find("NetworkManager").GetComponent<KcpTransport>().Port = PortInt;
    }


}
