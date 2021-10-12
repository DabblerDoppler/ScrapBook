using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformController : RaycastController {

    public Vector3 move;
    public LayerMask passengerMask;

    // Start is called before the first frame update
    public override void Start() {
        base.Start();
        
    }

    // Update is called once per frame
    void Update() {
        Vector3 velocity = move * Time.deltaTime;
        MovePassengers(velocity);
        transform.Translate(velocity);
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
            float rayLength = SKIN_WIDTH * 2;
            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
                if (hit) {
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
