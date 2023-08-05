using System.Collections;
using Mirror;
using System;
using UnityEngine;

public class Controller2D : RaycastController {

    public bool incrementStars;
    bool killPlayer;
    public bool spikeDamage;
    GameObject destroyEnemy;
    Player knockdownPlayer;
    Collider2D lastStarHit;
    public LayerMask collisionMask;
    public LayerMask playerCollisionMask;
    public LayerMask starCollisionMask;
    public LayerMask spikeCollisionMask;
    public LayerMask lavaCollisionMask;
    public LayerMask teleporterCollisionMask;
    public LayerMask enemyCollisionMask;
    public LayerMask bumpCollisionMask;
    public LayerMask bumpHitboxCollisionMask;
    public CollisionInfo collisions;
    public CollisionInfo playerCollisions;
    public CollisionInfo enemyCollisions;
    public Boolean wallAbove;


    [SerializeField] private GameObject bumpHitboxPrefab;

    public float velocityX;
    public float velocityY;

    const float WALL_ABOVE_RAY_LENGTH = 0.5f;


    public override void Start() {
        base.Start();
        playerCollisionMask = LayerMask.GetMask("Players");
        collisionMask = LayerMask.GetMask("Floor");
        bumpCollisionMask = LayerMask.GetMask("BumpBlock");
        bumpHitboxCollisionMask = LayerMask.GetMask("BumpHitbox");
        starCollisionMask = LayerMask.GetMask("Star", "FallenStar");
        enemyCollisionMask = LayerMask.GetMask("Enemies");
        spikeCollisionMask = LayerMask.GetMask("Spike");
        lavaCollisionMask = LayerMask.GetMask("Lava");
        teleporterCollisionMask = LayerMask.GetMask("Teleporter");
    }

    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();
        killPlayer = false;
        spikeDamage = false;
        knockdownPlayer = null;
        collisions.Reset();
        playerCollisions.Reset();
        enemyCollisions.Reset();


        velocityX = velocity.x;
        velocityY = velocity.y;

        
        //update velocityX so other players can see it

        if(isServer) {
            RpcSetVelocity(velocity.x);
        } else {
            CmdSetVelocity(velocity.x);
        }

        HorizontalCollisions(ref velocity);
        HorizontalCollisions_Bump(ref velocity);
        //HorizontalCollisions_Players(ref velocity);
        //HorizontalCollisions_Spikes(ref velocity);
        HorizontalCollisions_Teleporters(ref velocity);
        HorizontalCollisions_Spikes(ref velocity);
        //Collisions_BumpHitbox(ref velocity);
        
        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
            VerticalCollisions_Bump(ref velocity);
            VerticalCollisions_Spikes(ref velocity);
            VerticalCollisions_Lava(ref velocity);
            VerticalCollisions_Enemies(ref velocity);
        }

        //if the player is crouching, test to see if they're allowed to stand up or jump.
        wallAbove = false;
        if(GetComponent<Player>().crouching || GetComponent<Player>().sliding) {
            CrouchCollisions(ref velocity);
        }

        VerticalCollisions_Players(ref velocity);
        HorizontalCollisions_Players(ref velocity);
        //HorizontalCollisions_Enemies(ref velocity);

        if (knockdownPlayer != null && knockdownPlayer.team != GetComponent<Player>().team) {
            if (isServer) {
                if (GetComponent<Player>().groundPound) {
                    knockdownPlayer.RpcKnockdown3();
                } else { 
                    knockdownPlayer.RpcKnockdown();
                }
            } else {
                if (GetComponent<Player>().groundPound) {
                    CmdKnockdownPlayer3(knockdownPlayer);
                } else {
                    CmdKnockdownPlayer(knockdownPlayer);
                }
            }
        }

        if(destroyEnemy != null) {
            Debug.Log("destroyEnemy is :" + destroyEnemy);
            GameObject enemy = destroyEnemy;
            if(isServer) {
                Debug.Log("enemy is: " + enemy);
                RpcKillEnemy(enemy);
            } else {
                CmdKillEnemy(enemy);
            }
        }

        destroyEnemy = null;
        
        if (incrementStars && lastStarHit != null && GetComponentInParent<Player>().intangibility < 0) {
            GetComponentInParent<Player>().CmdAddStars(1);
            if (lastStarHit.GetComponent<Star>() != null) {
                lastStarHit.GetComponentInParent<Star>().playerTouch();
            } else {
                lastStarHit.GetComponentInParent<DroppedStar>().playerTouch();
            }
            incrementStars = false;
        }

        //this is to kill the associated player if you step in lava.
        if (killPlayer) {
            GetComponent<Player>().Death();
        }

        if(spikeDamage) {
            if(isServer) {
                GetComponent<Player>().RpcKnockdown_Spike();
            } else {
                CmdKnockdownPlayer_Spike(GetComponent<Player>());
            }
        }

        transform.position += velocity;
    }


    void VerticalCollisions(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }
    }

    void VerticalCollisions_Bump(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, bumpCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                if(velocity.y > 0.01) {
                    //bump block up
                    if(isServer) {
                        GameObject hitbox = Instantiate(bumpHitboxPrefab);
                        hitbox.transform.position = hit.transform.position + new Vector3(0.0f, 0.5f, 0.0f);
                        NetworkServer.Spawn(hitbox);
                    } else {
                        CmdSpawnBumpHitbox(hit.transform.position + new Vector3(0.0f, 0.5f, 0.0f));
                    }
                } else {
                    if(transform.GetComponent<Player>().groundPound) {
                        //bump block down
                        if(isServer) {
                            GameObject hitbox = Instantiate(bumpHitboxPrefab);
                            hitbox.transform.position = hit.transform.position - new Vector3(0.0f, 0.5f, 0.0f);
                            //should change rotation here too
                            NetworkServer.Spawn(hitbox);
                        } else {
                            CmdSpawnBumpHitbox(hit.transform.position - new Vector3(0.0f, 0.5f, 0.0f));
                        }
                    }
                }
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }
    }

    void VerticalCollisions_Players(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength * 2.75f, playerCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                if (hit.transform.GetComponent<Player>().intangibility <= 0 && GetComponent<Player>().intangibility <= 0 && !GetComponent<Player>().onGround) {
                    playerCollisions.below = directionY == -1;
                    playerCollisions.above = directionY == 1;
                    if (playerCollisions.below && transform.position.y > hit.transform.position.y) {
                        knockdownPlayer = hit.collider.GetComponent<Player>();
                    } else {
                        playerCollisions.below = false;
                        playerCollisions.above = false;
                    }
                }
            }
        }
    }

    void VerticalCollisions_Enemies(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength * 2, enemyCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit && GetComponent<Player>().intangibility <= 0 && !GetComponent<Player>().onGround) {
                enemyCollisions.below = directionY == -1;
                enemyCollisions.above = directionY == 1;
                if (enemyCollisions.below && transform.position.y > hit.transform.position.y) {
                    //knockdown
                    Debug.Log("enemy hit: " + hit.collider.gameObject);
                    destroyEnemy = hit.collider.gameObject;
                }
            }
        }
    }

    void Collisions_BumpHitbox(ref Vector3 velocity) {
        float distance = Mathf.Pow(Mathf.Sqrt(velocity.x) + Mathf.Sqrt(velocity.y), 2.0f);
        RaycastHit2D hit;
        if(velocity.x == 0 && velocity.y == 0) {
            hit = Physics2D.BoxCast(collider.bounds.center, collider.size, 0.0f, new Vector2(1, 0), 0.05f, bumpHitboxCollisionMask);
            if(!hit) {
                hit = Physics2D.BoxCast(collider.bounds.center, collider.size, 0.0f, new Vector2(-1, 0), 0.05f, bumpHitboxCollisionMask);
            }
        } else {
        hit = Physics2D.BoxCast(collider.bounds.center, collider.size, 0.0f, new Vector2(velocity.x, velocity.y), 0.05f, bumpHitboxCollisionMask);
        }
        if (hit && GetComponent<Player>().intangibility <= 0) {
            Debug.Log(hit);
            spikeDamage = true;
        }
    }





    void CrouchCollisions(ref Vector3 velocity) {
        float directionY = 1;
        float rayLength = WALL_ABOVE_RAY_LENGTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                wallAbove = true;
            }
        }
    }

    void TeleportToOtherTeleporter(RaycastHit2D hit) {
        GameObject teleportTo;
        StartCoroutine(FreezeCam());
        if (hit.transform.name == "Teleporter_Left") {
            teleportTo = GameObject.Find("Teleporter_Right");
            //GetComponent<Rigidbody2D>().simulated = false;
            //StartCoroutine(Resimulate());
            transform.position = new Vector3(teleportTo.GetComponent<BoxCollider2D>().bounds.min.x - collider.size.x / 2, transform.position.y, transform.position.z);
        } else {
            teleportTo = GameObject.Find("Teleporter_Left");
            //GetComponent<Rigidbody2D>().simulated = false;
            //StartCoroutine(Resimulate());
            transform.position = new Vector3(teleportTo.GetComponent<BoxCollider2D>().bounds.max.x + collider.size.x / 2, transform.position.y, transform.position.z);
        }
    }


    [Command(requiresAuthority = false)]
    void CmdResimulate() { RpcResimulate(); }

    [Command(requiresAuthority = false)]
    void CmdUnsimulate() { RpcUnsimulate(); }

    [ClientRpc]
    void RpcResimulate() { StartCoroutine(Resimulate()); }

    [ClientRpc]
    void RpcUnsimulate() {
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<SpriteRenderer>().forceRenderingOff = true;
    }

    //might need to be space out for client
    IEnumerator Resimulate() {
        yield return new WaitForSeconds(0.1f);
        GetComponent<Rigidbody2D>().simulated = true;
        //GetComponent<SpriteRenderer>().forceRenderingOff = false;
    }

    //might need to be space out for client
    IEnumerator FreezeCam() {
        Camera.main.clearFlags = CameraClearFlags.Nothing;
        Camera.main.cullingMask = 0;
        yield return null;
        Camera.main.clearFlags = CameraClearFlags.Skybox;
        Camera.main.cullingMask = -1;
    }

    void VerticalCollisions_Lava(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, lavaCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
                killPlayer = true;
                
            }
        }
    }

        void VerticalCollisions_Spikes(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, spikeCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            if (hit) {
                velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                rayLength = hit.distance;
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
                spikeDamage = true;
                
            }
        }
    }

    public bool VerticalCollisions_uncrouch() {
        float rayLength = 0.5f;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i );
            RaycastHit2D foundHit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.red);
            if (foundHit) {
                return true;
            }
        }
        return false;
    }


    void HorizontalCollisions(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                rayLength = hit.distance;
                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
            }
        }
    }

        void HorizontalCollisions_Bump(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, bumpCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                rayLength = hit.distance;
                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
            }
        }
    }

    //testing
    void HorizontalCollisions_Players(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, playerCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                //float otherHsp = hit.collider.GetComponent<Controller2D>().velocityX;
                float otherHsp = 0.0f;
                velocity.x += ((hit.distance - (1.5f * SKIN_WIDTH) )  * directionX) - (otherHsp);
                if(Math.Sign(hit.distance) == Math.Sign(otherHsp)){
                    rayLength = hit.distance + otherHsp;
                } else {
                    rayLength = hit.distance - otherHsp;
                }
                playerCollisions.left = directionX == -1;
                playerCollisions.right = directionX == 1;

                //knockback other
                if(GetComponent<Player>().diving || GetComponent<Player>().long_jump) {
                    if(isServer) {
                        RpcKnockbackPlayer(hit.collider.GetComponent<Player>(), Math.Sign(directionX));
                    } else {
                        CmdKnockbackPlayer(hit.collider.GetComponent<Player>(), Math.Sign(directionX));
                    }
                }
            }
        }
    }

    /*
    void HorizontalCollisions_Enemies(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength * 2, enemyCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit && GetComponent<Player>().intangibility <= 0 && hit.collider.GetComponent<EnemyVariables>().spawnIntangibility < 0) {
                enemyCollisions.left = directionX == -1;
                enemyCollisions.right = directionX == 1;
                if (destroyEnemy == null) {
                    CmdKnockdownPlayer(GetComponent<Player>());
                }
            }
        }
    }
    */


    void HorizontalCollisions_Lava(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, lavaCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                rayLength = hit.distance;
                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
                killPlayer = true;
            }
        }
    }

        void HorizontalCollisions_Spikes(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, spikeCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                rayLength = hit.distance;
                collisions.left = directionX == -1;
                collisions.right = directionX == 1;
                spikeDamage = true;
            }
        }
    }


    void HorizontalCollisions_Teleporters(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, teleporterCollisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            if (hit) {
                TeleportToOtherTeleporter(hit);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.tag == "Star") {
            lastStarHit = col;
            incrementStars = true;
        } 
    }

    [Command(requiresAuthority = false)]
    public void CmdSpawnBumpHitbox(Vector3 spawnPosition) {
        GameObject hitbox = Instantiate(bumpHitboxPrefab);
        hitbox.transform.position = spawnPosition;
        //should change rotation here too
        NetworkServer.Spawn(hitbox);
    }

    [Command(requiresAuthority = false)]
    public void CmdKnockdownPlayer(Player player) {
        Debug.Log("CmdKnockdownPlayer run. Trying to call RpcKnockdown.");
        player.RpcKnockdown();
    }

    [Command(requiresAuthority = false)]
    public void CmdKnockdownPlayer_Spike(Player player) {
        Debug.Log("CmdKnockdownPlayer run. Trying to call RpcKnockdown.");
        player.RpcKnockdown_Spike();
    }

    [Command(requiresAuthority = false)]
    public void CmdKnockdownPlayer3(Player player) {
        Debug.Log("CmdKnockdownPlayer run. Trying to call RpcKnockdown.");
        player.RpcKnockdown3();
    }

    //WIP
    [Command(requiresAuthority = false)]
    public void CmdKnockbackPlayer(Player player, int direction) {

    }

    [ClientRpc]
    public void RpcKnockbackPlayer(Player player, int direction) {

    }

    [Command(requiresAuthority = false)]
    public void CmdKillEnemy(GameObject enemy) {
        RpcKillEnemy(enemy);
    }

    [ClientRpc]
    public void RpcKillEnemy(GameObject enemy) {
        Debug.Log("Server Destroying: " + enemy);
        Destroy(enemy);
    }

    [Command(requiresAuthority =false)]
    public void CmdSetVelocity(float velocity) {
        RpcSetVelocity(velocity);
    }

    [ClientRpc]
    public void RpcSetVelocity(float velocity) {
        velocityX = velocity;
    }


}
