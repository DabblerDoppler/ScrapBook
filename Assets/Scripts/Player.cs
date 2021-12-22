using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Controller2D))]
public class Player : NetworkBehaviour {

    //input
    public float horizontalInput;
    float verticalInput;
    bool jumpHeld;
    bool jumpPressed;
    bool divePressed;
    bool diveHeld;
    bool spinPressed;

    private System.Random rand;

    public int stars;

    //actions
    public bool sliding;
    int walljump_lock;
    int facing;
    public bool spun;
    public bool long_jump;
    public bool crouching;
    public bool backflip;
    public bool diving;
    public bool groundPound;
    public bool onGround;
    public bool isDead;
    public float coyoteTime;
    public float goombaJumpCoyoteTime;
    public float coyoteTime_Wall;
    public float jumpBuffer;
    public float groundPoundWindup;
    public float groundPoundLag;
    public int lastWall;
    public float spinning;
    public float knockdown;
    public float intangibility;
    bool intangibilityEnded;

    //unity variables
    BoxCollider2D collider;
    Controller2D controller;

    //map object variables
    public GameObject playerMapObject;
    private GameObject myMapObject;

    //physics
    public float hsp;
    public float vsp;

    //yo, what do you think is behind that door marked pirate? Do you think a pirate lives in there?
    private Vector2 standColliderSize;
    private Vector2 standColliderOffset;
    private Vector2 crouchColliderSize;
    private Vector2 crouchColliderOffset;

    private Vector3 spawnPoint;

    public int team;

    Color[] colors;




    const float HSP_FRIC_GROUND = 0.00015f * 500  ;
    const float HSP_FRIC_SLIDE = 0.00009f * 500 ;
    const float HSP_FRIC_AIR = 0.000035f * 500  ;
    const float HSP_FRIC_WALLJUMP = 0.000005f * 500  ;
    const float HSP_MAX = 0.009f;
    const float HSP_MAX_SLIDE = 0.02f;
    const float HSP_MAX_LONGJUMP = 0.015f;
    const float HSP_MAX_CROUCH = 0.005f;
    const float HSP_KNOCKDOWN = 0.005f;
    const float VSP_MAX = 0.018f;
    const float VSP_MAX_WALL = 0.006f;
    const float VSP_GROUNDPOUND = 0.024f;
    const float VSP_KNOCKDOWN = 0.004f;
    const float GRAVITY = 0.000075f * 500 ;
    const float GRAVITY_HELD = 0.00005f * 500 ;
    const float JUMP_HEIGHT = 0.01325f;
    const float WALL_JUMP_HEIGHT = 0.01025f  ;
    const float WALL_JUMP_LENGTH = 0.09f;
    const float LONG_JUMP_HEIGHT = 0.011f;
    const float LONG_JUMP_LENGTH = 0.015f;
    const float BACKFLIP_HEIGHT = 0.01675f;
    const float BACKFLIP_LENGTH = 0.004f;
    const float PLAYER_JUMP_HEIGHT = 0.015f;
    const float PLAYER_HOP_HEIGHT = 0.008f;
    const float DIVE_LENGTH = 0.009f;
    const float DIVE_HEIGHT = 0.00605f;
    const float COYOTE_TIME = 0.1f;
    const float GROUNDPOUND_WINDUP = 0.20f;
    const float GROUNDPOUND_LAG = 0.20f;
    const float SPIN_TIME = 0.20f;
    const float KNOCKDOWN_TIME = 1.0f;
    const float INTANGIBILITY_TIME = 1.35f;
    public const float CROUCH_PERCENT = 0.5f;

    const float DEADZONE = 0.2f;
    const float WALK_SPEED = 0.0005f * 500;
    const float AIR_SPEED = 0.00015f * 500;
    const float SLIDE_SPEED = 0.1f * 500;
    const float CROUCH_SPEED = 0.0003f * 500;

    const int LAYER_FLOOR = 6;
    const int LAYER_WALL = 7;

    public Animator animator;

    private void Awake() {
        myMapObject = Instantiate(playerMapObject, new Vector3(0, 0, 0), Quaternion.identity);
        myMapObject.GetComponent<MapObject>().associatedTransform = transform;

        /*
        setTeam(GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().teams % 4);
        if (isServer) {
            GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().teams += 1;
        } else {
            CmdIncrementTeams();
        }
        */

    }

    private void Start() {
        Color[] colors = new Color[4];

        colors[0] = Color.green;
        colors[1] = Color.red;
        colors[2] = Color.yellow;
        colors[3] = Color.cyan;

        stars = 0;

        rand = new System.Random();

        collider = GetComponent<BoxCollider2D>();
        controller = GetComponent<Controller2D>();

        standColliderOffset = collider.offset;
        standColliderSize = collider.size;

        crouchColliderSize = new Vector2(standColliderSize.x, standColliderSize.y * CROUCH_PERCENT);
        crouchColliderOffset = new Vector2(standColliderOffset.x, - standColliderSize.y * 0.5f * CROUCH_PERCENT);

        spawnPoint = transform.position;

        walljump_lock = 0;
        sliding = false;
        long_jump = false;
        diving = false;
        groundPound = false;
        isDead = false;
        facing = 1;
        coyoteTime = 0.0f;
        coyoteTime_Wall = 0.0f;
        jumpBuffer = 0.0f;
        groundPoundWindup = 0.0f;
        groundPoundLag = 0.0f;
        intangibility = 0f;
        knockdown = 0f;

    }

    private void Update() {
        HandleMovement();
    }

    private void OnDestroy() {
        if (Camera.main != null) {
            Camera.main.GetComponent<CameraFollow>().hasTarget = false;
        }
    }

    public void setTeam(int toSet) {
        GetComponent<SpriteRenderer>().color = colors[toSet];
        team = toSet;
    }


    void HandleMovement() {
        if (isLocalPlayer) {
            if(isDead) {
                return;
            }
            HandleInput();
            //this needs to run on all client's copy of this object.
            //a command into a ClientRpc method is probably the way to go.
            if (intangibility < 0 && !intangibilityEnded) {
                if (isServer) {
                    RpcEndIntangibility();
                }
                else {
                    CmdEndIntangibility();
                }
            }

            onGround = controller.collisions.below;
            int onWall;

            if(controller.collisions.left) {
                onWall = -1;
            } else if (controller.collisions.right) {
                onWall = 1;
            } else {
                onWall = 0;
            }


            if (Math.Sign(onWall) != Math.Sign(horizontalInput) || onGround) {
                onWall = 0;
            }

            if (facing > 0) {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else {
                GetComponent<SpriteRenderer>().flipX = true;
            }

            if (onWall != 0 || onGround) {
                walljump_lock = 0;
                long_jump = false;
                spun = false;
                spinning = 0.0f;
            }

            if(diving && onGround) {
                diving = false;
                sliding = true;
            }
            if(diving && onWall != 0) {
                diving = false;
            }

            jumpBuffer -= Time.deltaTime;
            coyoteTime -= Time.deltaTime;
            coyoteTime_Wall -= Time.deltaTime;
            groundPoundWindup -= Time.deltaTime;
            groundPoundLag -= Time.deltaTime;
            spinning -= Time.deltaTime;
            intangibility -= Time.deltaTime;
            knockdown -= Time.deltaTime;
            goombaJumpCoyoteTime -= Time.deltaTime;


            if(onGround) {
                coyoteTime = COYOTE_TIME;
                backflip = false;
            }

            if(onWall != 0 && Math.Sign(horizontalInput) == Math.Sign(onWall)) {
                coyoteTime_Wall = COYOTE_TIME;
                lastWall = onWall;
                backflip = false;
            }

            if(!onGround && onWall == 0 && jumpPressed) {
                jumpBuffer = COYOTE_TIME;
            }

            if (jumpBuffer > 0) {
                jumpPressed = true;
            }


            if (walljump_lock != 0) {
                if (vsp > 0) {
                    if (Math.Sign(horizontalInput) == Math.Sign(walljump_lock)) {
                        horizontalInput *= 0.2f;
                    }
                    else {
                        horizontalInput *= 0.05f;
                    }
                }
                else {
                    if (Math.Sign(horizontalInput) == Math.Sign(walljump_lock)) {
                        horizontalInput *= 0.4f;
                    }
                    else {
                        horizontalInput *= 0.2f;
                    }
                }
            }

            if(knockdown > 0) {
                horizontalInput = 0;
            }

            if (controller.collisions.above || controller.collisions.below) { vsp = 0; }
            if (controller.collisions.left || controller.collisions.right) { hsp = 0; }


            //if (controller.playerCollisions.above || controller.playerCollisions.below) { vsp = 0; }
            //if ((controller.playerCollisions.left || controller.playerCollisions.right) && !controller.playerCollisions.below) { hsp = 0; }

            //goomba stomp
            if (controller.playerCollisions.below || controller.enemyCollisions.below) {
                spun = false;
                walljump_lock = 0;
                diving = false;
                long_jump = false;
                goombaJumpCoyoteTime = COYOTE_TIME;
                if (jumpHeld) {
                    vsp = PLAYER_JUMP_HEIGHT;
                    goombaJumpCoyoteTime = 0;
                }
                else {
                    vsp = PLAYER_HOP_HEIGHT;
                }
            }

            if (!crouching && !sliding && !diving) {
                if (controller.VerticalCollisions_uncrouch() && onGround) {
                    collider.size = crouchColliderSize;
                    collider.offset = crouchColliderOffset;
                    controller.CalculateRaySpacing();
                    crouching = true;
                }
                else if (collider.size != standColliderSize) {
                    collider.size = standColliderSize;
                    collider.offset = standColliderOffset;
                    controller.CalculateRaySpacing();
                }
            }





            if(knockdown > 0) {
            } else if (sliding) {
                SlideState(onGround);
            } else if (groundPoundWindup > 0) {
                GroundPoundWindupState();
            } else if (groundPound) {
                GroundPoundState();
            } else if (groundPoundLag > 0) {
                GroundPoundLagState();
            } else if (walljump_lock != 0) {
                WallJumpState();
            } else if (crouching) {
                CrouchState();
            } else {
            NormalState(onGround, onWall);
            }

            float moveSpeed = AIR_SPEED;
            if (onGround) { moveSpeed = WALK_SPEED; }
            if (crouching) { moveSpeed = CROUCH_SPEED; }
            if (!sliding && !groundPound && groundPoundWindup < 0 && groundPoundLag < 0) {
                hsp += moveSpeed * horizontalInput * Time.deltaTime;
            }


            if (onGround && diveHeld && !sliding && groundPoundLag < 0) {
                crouching = true;
                if (collider.size != crouchColliderSize) {
                    collider.size = crouchColliderSize;
                    collider.offset = crouchColliderOffset;
                    controller.CalculateRaySpacing();
                }
            } else {
                crouching = false;
            }




            //apply gravity
            float gravity = GRAVITY;
            if(jumpHeld && vsp > 0) {gravity = GRAVITY_HELD;}
            if (groundPoundWindup < 0) {
                vsp -= gravity * Time.deltaTime;
            }

            float vspMaxFinal = VSP_MAX;
            if (onWall != 0 && Math.Sign(horizontalInput) == Math.Sign(onWall) && vsp <= 0) { vspMaxFinal = VSP_MAX_WALL; }
            if(groundPound) {
                vspMaxFinal = VSP_GROUNDPOUND;
            }
            float hspMaxFinal = HSP_MAX;
            if(sliding) {hspMaxFinal = HSP_MAX_SLIDE; }
            if (crouching) { hspMaxFinal = HSP_MAX_CROUCH; }
            if (long_jump || diving) { hspMaxFinal = HSP_MAX_LONGJUMP; }

            //apply friction
            float friction = HSP_FRIC_AIR;
            if(onGround) {friction = HSP_FRIC_GROUND;}
            if(sliding) { friction = HSP_FRIC_SLIDE; }
            if(walljump_lock != 0) { friction = HSP_FRIC_WALLJUMP; }

            hsp = Approach(hsp, 0, friction * Time.deltaTime);

            //clamp to
            hsp = Clamp(hsp, -hspMaxFinal , hspMaxFinal);
            vsp = Clamp(vsp, -vspMaxFinal , vspMaxFinal);

            if(diving || long_jump) {
                if (Math.Sign(hsp) != Math.Sign(walljump_lock)) {
                    hsp *= -1;
                }
            }

            if(spinning > 0) {
                vsp = 0;
            }


            controller.Move(new Vector3(hsp, vsp) * Time.deltaTime * 600);

            animator.SetFloat("Player_hsp", hsp);
            animator.SetFloat("Player_vsp", vsp);


        }
    }

    void HandleInput() {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        jumpHeld = Input.GetButton("Jump");
        jumpPressed = Input.GetButtonDown("Jump");
        spinPressed = Input.GetButtonDown("Spin");
        diveHeld = Input.GetButton("Dive");
        divePressed = Input.GetButtonDown("Dive");
    }


    void NormalState(bool onGround, int onWall) {
        if (onWall != 0) {
            facing = -onWall;
        }

        if (horizontalInput != 0 && onGround) {
            facing = Math.Sign(horizontalInput);
        }




        //ground jump
        if (jumpPressed && coyoteTime > 0) {
            vsp = JUMP_HEIGHT;
        }
        //wall jump
        if (jumpPressed && coyoteTime_Wall > 0) {
            vsp = WALL_JUMP_HEIGHT;
            hsp = WALL_JUMP_LENGTH * -lastWall;
            walljump_lock = -lastWall;
        }

        if(jumpPressed && goombaJumpCoyoteTime > 0) {
            vsp = PLAYER_JUMP_HEIGHT - (PLAYER_HOP_HEIGHT * 0.1f);
            goombaJumpCoyoteTime = 0;
        }


        //slide
        if(divePressed && onGround && Math.Abs(hsp) > 0.006f && Math.Sign(hsp) == Math.Sign(horizontalInput)) {
            sliding = true;
            hsp = Math.Sign(hsp) * SLIDE_SPEED;
            if (collider.size != crouchColliderSize) {
                collider.size = crouchColliderSize;
                collider.offset = crouchColliderOffset;
                controller.CalculateRaySpacing();
            }
        }
        if (divePressed && !onGround && verticalInput < -0.25) {
            vsp = 0;
            hsp = 0;
            groundPoundWindup = GROUNDPOUND_WINDUP;
            groundPound = true;
        } else if (divePressed && !onGround && Math.Abs(hsp) > 0.0025) {
            vsp = DIVE_HEIGHT;
            hsp += DIVE_LENGTH * Math.Sign(hsp);
            walljump_lock = Math.Sign(hsp);
            diving = true;
            facing = Math.Sign(hsp);
            if (collider.size != crouchColliderSize) {
                collider.size = crouchColliderSize;
                collider.offset = crouchColliderOffset;
                controller.CalculateRaySpacing();
            }
        }

        if (spinPressed && !onGround && !spun && !groundPound && onWall == 0) {
            spinning = SPIN_TIME;
            spun = true;
        }

    }

    void WallJumpState() {
        if (divePressed && !onGround && verticalInput < -0.25) {
            vsp = 0;
            hsp = 0;
            groundPoundWindup = GROUNDPOUND_WINDUP;
            groundPound = true;
        }
        if (spinPressed && !onGround && !spun && !diving) {
            spinning = SPIN_TIME;
            spun = true;
        }
    }

    void GroundPoundWindupState() {
        //no actions
    }

    void GroundPoundState() {
        if (onGround) {
            groundPoundLag = GROUNDPOUND_LAG;
            groundPound = false;
            return;
        }
        hsp = 0;
        vsp = -VSP_GROUNDPOUND;
    }


    void GroundPoundLagState() {
        //no actions
    }

    void SlideState(bool onGround) {
        if(coyoteTime <= 0.0f || Math.Abs(hsp) < 0.0015f ) {
            sliding = false;
            return;
        }
        //long jump
        if(jumpPressed) {
            long_jump = true;
            hsp = Math.Sign(hsp) * LONG_JUMP_LENGTH;
            vsp = LONG_JUMP_HEIGHT;
            walljump_lock = Math.Sign(hsp);
        }
    }

    void CrouchState() {
        if (hsp != 0) {
            facing = Math.Sign(hsp);
        }
        if (jumpPressed) {
            backflip = true;
            hsp = -facing * BACKFLIP_LENGTH;
            vsp = BACKFLIP_HEIGHT;
            walljump_lock = Math.Sign(hsp);
        }
    }


    public void Death() {
        GetComponent<SpriteRenderer>().enabled = false;
        //GetComponent<Rigidbody2D>().simulated = false;
        DropStars(3);
        isDead = true;
        StartCoroutine(RespawnAfterTime(3));
    }

    IEnumerator RespawnAfterTime(float time) {
        yield return new WaitForSeconds(time);
        // Code to execute after the delay
        transform.position = spawnPoint;
        isDead = false;
        GetComponent<SpriteRenderer>().enabled = true;
        //GetComponent<Rigidbody2D>().simulated = true;
    }


    [ClientRpc]
    public void RpcKnockdown() {
        if (intangibility < 0) {
            DropStars(1);
            knockdown = KNOCKDOWN_TIME;
            intangibility = INTANGIBILITY_TIME;
            vsp = 0;
            //vsp = VSP_KNOCKDOWN;
            hsp = HSP_KNOCKDOWN;
            //GetComponent<Rigidbody2D>().simulated = false;
            intangibilityEnded = false;
        }
    }

    [ClientRpc]
    public void RpcKnockdown3() {
        if (intangibility < 0) {
            DropStars(3);
            knockdown = KNOCKDOWN_TIME;
            intangibility = INTANGIBILITY_TIME;
            vsp = 0;
            //vsp = VSP_KNOCKDOWN;
            hsp = HSP_KNOCKDOWN;
            //GetComponent<Rigidbody2D>().simulated = false;
            intangibilityEnded = false;
        }
    }

    void DropStars(int numToDrop) {
        Debug.Log("Client " + NetworkConnectionToClient.LocalConnectionId + "calling dropstars.");
        if (stars >= numToDrop) {
            //CmdSetStars spawns stars.
            CmdSetStars(stars - numToDrop, transform);
        }
        else {
            DropStars(numToDrop - 1);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSetStars(int newStars, Transform t) {
        int numToDrop = 0;
        if (newStars < stars) {
            numToDrop = stars - newStars;
        }
        RpcSetStars(newStars);
        for (int i = 0; i < numToDrop; i++) {
            GameObject currentStar = Instantiate(GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>().spawnPrefabs.Find(prefab => prefab.name == "DroppedStar"));
            NetworkServer.Spawn(currentStar);
            currentStar.transform.position = t.position;
            currentStar.GetComponent<DroppedStar>().ignoredPlayer = transform.gameObject;
            currentStar.GetComponent<Rigidbody2D>().velocity = new Vector2((float)((rand.NextDouble() - 0.5) * 10.0), (0.75f + (0.25f * (float)rand.NextDouble())) * 5.0f);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdAddStars(int toAdd) {
        RpcSetStars(stars + toAdd);
    }



    [ClientRpc]
    void RpcSetStars(int newStars) {
        stars = newStars;
    }



    [Command(requiresAuthority = false)]
    public void CmdEndIntangibility() {
        RpcEndIntangibility();
    }

    [ClientRpc]
    public void RpcEndIntangibility() {
        //GetComponent<Rigidbody2D>().simulated = true;
        intangibilityEnded = true;
        intangibility = -1;
        knockdown = -1;
    }





    public override void OnStartClient() {
        if (isServer) {
            RpcSetUI(0.2f);
        } else {
            CmdSetUI(0.2f);
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSetUI(float time) {
        RpcSetUI(time);
    }

    [ClientRpc]
    void RpcSetUI(float time) {
        StartCoroutine(WaitAndSetUI(time));
    }

    IEnumerator WaitAndSetUI(float seconds) {
        yield return new WaitForSeconds(seconds);
        List<GameObject> childList = GameObject.Find("Canvas").GetComponent<TextChildArray>().myChildren;
        GameObject[] playerArray = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playerArray.Length; i++) {
            childList[i].GetComponent<StarUI>().attachedPlayer = playerArray[i].GetComponent<Player>();
        }
    }


    public static float Approach(float argument0, float argument1, float argument2) {
        if (argument0 < argument1) {
            argument0 += argument2;
            if (argument0 > argument1) { return argument1; }
        }
        else {
            argument0 -= argument2;
            if (argument0 < argument1) { return argument1; }
        }
        return argument0;
    }

    public static float Clamp(float value, float min, float max) {
        if(value > max) {
            value = max;
        }
        if(value < min) {
            value = min;
        }
        return value;
    }




}
