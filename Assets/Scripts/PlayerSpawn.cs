using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake() {
        SpawnSystem.AddPlayerSpawn(transform);
    }
    void OnDestroy() {
        SpawnSystem.RemovePlayerSpawn(transform);
    }
}
