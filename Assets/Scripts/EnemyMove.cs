using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class EnemyMove : RaycastController {

    public bool onGround;
    public bool facingRight;
    public LayerMask collisionMask_Walls;

    public CollisionInfo collisions;


    public float hsp;
    public float vsp;
    public const float WALK_SPEED = 0.015f;
    public const float GRAVITY = 0.0375f;
    public const float VSP_MAX = 0.018f;
    public const float HSP_MAX = 0.002f;
    public const float FRICTION_GROUND = 0.004f;
    public const float FRICTION_AIR = 0.002f;



    public override void Awake() {
        base.Awake();
    }

    public override void Start() {
        base.Start();
        //the ternary operator would be sick af if i didn't keep forgetting how to use it
        facingRight = (Math.Sign(UnityEngine.Random.Range(-1, 1)) == 1) ? false : true;
        collisionMask_Walls = LayerMask.GetMask("Floor", "EnemyWalls");
    }

    // Update is called once per frame
    void Update() {

        if(collisions.above || collisions.below) {
            vsp = 0;
        }

        if(collisions.left || collisions.right) {
            hsp = 0;
            facingRight = !facingRight;
        }

        onGround = collisions.below;

        if (onGround && GetComponent<EnemyVariables>().spawnIntangibility < 0) {
            hsp += WALK_SPEED * (facingRight ? 1 : -1) * Time.deltaTime;
        }

        vsp -= GRAVITY * Time.deltaTime;

        float friction;

        if(onGround) {
            friction = FRICTION_GROUND;
        } else {
            friction = FRICTION_AIR;
        }

        hsp = Player.Approach(hsp, 0, friction * Time.deltaTime);
        vsp = Player.Approach(vsp, 0, friction * Time.deltaTime);

        hsp = Player.Clamp(hsp, -HSP_MAX, HSP_MAX);
        vsp = Player.Clamp(vsp, -VSP_MAX, VSP_MAX);

        Vector3 velocity = new Vector3(hsp, vsp, 0);

        Move(velocity * Time.deltaTime * 600);
        GetComponent<Teleportable>().CheckTeleporters(velocity * Time.deltaTime * 600);

    }

    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();
        collisions.Reset();
        if (velocity.x != 0) {
            HorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
        }
        transform.position += velocity;
    }

    void VerticalCollisions(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask_Walls);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }
    }

    void HorizontalCollisions(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask_Walls);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                rayLength = hit.distance;
                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
            }
        }
    }




}
