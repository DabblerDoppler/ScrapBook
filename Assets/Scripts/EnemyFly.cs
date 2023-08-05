using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyFly : RaycastController {

    [SyncVar]
    public Vector3 startWaypoint;

    [SyncVar]
    public Vector3 endWaypoint;
    [SyncVar]
    public bool movingToEnd;
    [SyncVar]
    float distanceBetweenWaypoints;
    [SyncVar]
    float percentMoved;
    [SyncVar]
    float nextMoveTime;

    [SyncVar]
    public float waitTime;

    [SyncVar]
    public float speed;

    [Range(0, 2)]
    [SyncVar]
    public float easeAmount;



    public override void Awake() {
        base.Awake();
    }

    // Start is called before the first frame update
    public override void Start() {
        base.Start();
        movingToEnd = true;
        percentMoved = 0.0f;
        distanceBetweenWaypoints = Vector3.Distance(startWaypoint, endWaypoint);
    }

    // Update is called once per frame
    void Update() {

        if (GetComponent<EnemyVariables>().spawnIntangibility < 0) {
            UpdateRaycastOrigins();
            Vector3 velocity = CalculateMovement();
            transform.Translate(velocity);
        }
    }

    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculateMovement() {

        if (Time.time < nextMoveTime) {
            return Vector3.zero;
        }

        percentMoved += Time.deltaTime * (speed / distanceBetweenWaypoints);

        percentMoved = Mathf.Clamp01(percentMoved);
        float easedPercentMoved = Ease(percentMoved);

        Vector3 newPos;
        //this line may need to check the bool to work right, reversing the start/end order if its false
        if (movingToEnd) {
            newPos = Vector3.Lerp(startWaypoint, endWaypoint, easedPercentMoved);
        }
        else {
            newPos = Vector3.Lerp(endWaypoint, startWaypoint, easedPercentMoved);
        }

        if (percentMoved >= 1.0f) {
            percentMoved = 0;
            movingToEnd = !movingToEnd;
            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(col.GetComponent<Player>()){
            if(col.GetComponent<Player>().intangibility <= 0.0f) {
                Debug.Log("Trigger");
                if(isServer) {
                    col.GetComponent<Player>().RpcKnockdown_Spike();
                }
            }
        }
    }
    void OnTriggerStay2D(Collider2D col) {
        if(col.GetComponent<Player>()){
            if(col.GetComponent<Player>().intangibility <= 0.0f) {
                Debug.Log("Trigger");
                if(isServer) {
                    col.GetComponent<Player>().RpcKnockdown_Spike();
                }
            }
        }
    }


}
