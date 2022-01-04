using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour {

[Header("UI")]
[SerializeField] private InputField nameInputField = null;
[SerializeField] private Button continueButton = null;

public static string DisplayName;
private const string PLAYER_PREFS_NAME_KEY = "PlayerName";


    // Start is called before the first frame update
    void Start() {
        SetUpInputField();
    }

    private void SetUpInputField() {
        if(!PlayerPrefs.HasKey(PLAYER_PREFS_NAME_KEY)) { return; }

        string defaultName = PlayerPrefs.GetString(PLAYER_PREFS_NAME_KEY);

        nameInputField.text = defaultName;

        SetPlayerName(defaultName);

    }

    public void SetPlayerName(string name) {
    }

    public void SavePlayerName() {
        DisplayName = nameInputField.text;

        PlayerPrefs.SetString(PLAYER_PREFS_NAME_KEY, DisplayName);
    }

}
