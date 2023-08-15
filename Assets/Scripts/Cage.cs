using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;



public class Cage : NetworkBehaviour {

    GameObject localPlayer;
    RectTransform area;

    public Vector2 minPosition;
    public Vector2 maxPosition;

    public Vector3 starPosition;

    public List<Transform> walls;

    public bool containsPlayer;

    public bool hasDisappeared;

    public bool hasAppeared;

    public float timeRemaining;


    public float timeMaximum = 6.0f;

    private Material transparent;
    private Material solid;

    // Start is called before the first frame update

    
    void Awake() {
        transparent = Resources.Load("TransparentWall", typeof(Material)) as Material;
        solid = Resources.Load("Wall", typeof(Material)) as Material;
        containsPlayer = false;
        hasDisappeared = true;
        hasAppeared = false;
        area = gameObject.transform.Find("Area").GetComponent<RectTransform>();
        
        Vector3[] areaCorners = new Vector3[4];
        area.GetWorldCorners(areaCorners);

        minPosition = new Vector2();
        minPosition.x = areaCorners[0].x;
        minPosition.y = areaCorners[0].y;

        maxPosition = new Vector2();
        maxPosition.x = areaCorners[2].x;
        maxPosition.y = areaCorners[2].y;

        foreach (Transform child in transform) {
            if(child.name == "Wall") {
                walls.Add(child);     
                child.gameObject.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        if(localPlayer == null) {
            localPlayer = NetworkClient.localPlayer.gameObject;
        } else {

            timeRemaining -= Time.deltaTime;


            if(localPlayer.transform.position.x > minPosition.x && localPlayer.transform.position.x < maxPosition.x &&
            localPlayer.transform.position.y > minPosition.y && localPlayer.transform.position.y < maxPosition.y) {
                containsPlayer = true;
            } else {
                containsPlayer = false;
            }

            if(timeRemaining > 0 && !hasAppeared) {
                Debug.Log("Appearing");
                hasAppeared = true;
                //appear
                foreach(Transform wall in walls) {
                    wall.gameObject.SetActive(true);
                    //set layer to transparentFX
                    wall.gameObject.layer = 1;
                    wall.gameObject.GetComponent<SpriteRenderer>().material = transparent;
                }
            } else if (containsPlayer && timeRemaining > 0 ) {
                Debug.Log("Appearing");
                hasAppeared = true;
                //appear
                foreach(Transform wall in walls) {
                    wall.gameObject.layer = 6;
                    wall.gameObject.GetComponent<SpriteRenderer>().material = solid;
                }
            } else if(!hasDisappeared && timeRemaining <= 0) {
                Debug.Log("Disappearing");
                //make walls disappear
                foreach(Transform wall in walls) {
                    wall.gameObject.SetActive(false);
                }
                hasDisappeared = true;
                hasAppeared = false;
                //spawn next star
                if(isServer) {
                    StartCoroutine(GameObject.Find("StarManager").GetComponent<StarManager>().SpawnAfterSeconds(starPosition));
                }
            }
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSpawnStar(Vector3 sPosition) {
        StartCoroutine(GameObject.Find("StarManager").GetComponent<StarManager>().SpawnAfterSeconds(sPosition));
    }


}
