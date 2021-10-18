using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MovingPlatformController : RaycastController {

    [SyncVar]
    public Vector3 startWaypoint;

    [SyncVar]
    public Vector3 endWaypoint;

    public bool movingToEnd;
    float distanceBetweenWaypoints;
    float percentMoved;
    float nextMoveTime;

    public float waitTime;

    public float speed;

    [Range(0,2)]
    public float easeAmount;

    public LayerMask passengerMask;

    // Start is called before the first frame update
    public override void Start() {
        base.Start();
        movingToEnd = true;
        percentMoved = 0.0f;
        distanceBetweenWaypoints = Vector3.Distance(startWaypoint, endWaypoint);
        passengerMask = LayerMask.GetMask("Players", "Enemies");
    }

    // Update is called once per frame
    void Update() {
            UpdateRaycastOrigins();

            Vector3 velocity = CalculatePlatformMovement();
            MovePassengers(velocity);

        if (isServer) {
            transform.Translate(velocity);
        }
    }

    float Ease(float x) {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x,a) + Mathf.Pow(1-x, a));
    }

    Vector3 CalculatePlatformMovement() {

        if(Time.time < nextMoveTime) {
            return Vector3.zero;
        }

        percentMoved += Time.deltaTime * (speed / distanceBetweenWaypoints);

        percentMoved = Mathf.Clamp01(percentMoved);
        float easedPercentMoved = Ease(percentMoved);

        Vector3 newPos;
        //this line may need to check the bool to work right, reversing the start/end order if its false
        if (movingToEnd) {
            newPos = Vector3.Lerp(startWaypoint, endWaypoint, easedPercentMoved);
        } else {
            newPos = Vector3.Lerp(endWaypoint, startWaypoint, easedPercentMoved);
        }

        if (percentMoved >= 1.0f) {
            percentMoved = 0;
            movingToEnd = !movingToEnd;
            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }


    void MovePassengers(Vector3 velocity) {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        //Vertically moving platform
        if (velocity.y != 0) {
            float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - SKIN_WIDTH) * directionY;
                        hit.transform.Translate(new Vector3(pushX, pushY));
                        movedPassengers.Add(hit.transform);
                    }
                }
            }
        }

        if (velocity.x != 0) {
            float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
            for (int i = 0; i < horizontalRayCount; i++) {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.green);
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        float pushX = velocity.x - (hit.distance - SKIN_WIDTH) * directionX;
                        float pushY = 0;
                        hit.transform.Translate(new Vector3(pushX, pushY));
                        movedPassengers.Add(hit.transform);
                    }
                }
            }
        }

        if(directionY == -1 || velocity.y == 0 && velocity.x != 0) {
            float rayLength = 2.0f * SKIN_WIDTH;
            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
                Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.red);
                if (hit) {
                    Debug.Log(hit + " has been hit for moving passengers!");
                    if (!movedPassengers.Contains(hit.transform)) {
                        float pushX = velocity.x;
                        float pushY = velocity.y;
                        hit.transform.Translate(new Vector3(pushX, pushY));
                        movedPassengers.Add(hit.transform);
                    }
                }
            }
        }


    }

}
