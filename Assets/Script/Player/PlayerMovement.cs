using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    CombatMovement combatManager;
    InventoryManager im;
    PlayerScript ps;
    CinemaManager cinemaManager;
    SoundManager sm;
    [HideInInspector]public Transform cam;

    [Header("Variables")]
    public UniversalManager um;
    public Animator an;
    CameraManager cameraManager;
    CapsuleCollider cc;

    [Header("Camera Settings")]

    public float cameraSensitivity;
    public float cameraDistance;
    public float minCamRot;
    public float maxCamRot;

    [Header("Particle Settings")]
    public ParticleSystem walkParticle;
    public float walkParticleDefaultEmissionRate;
    public GameObject jumpParticlePrefab;
    public Transform jumpParticlePos;
    public GameObject landParticlePrefab;
    public Transform landParticlePos;

    [Header("Movement Settings")]
    public bool alwaysRun;
    public float walkSpeed;
    public float runSpeed;

    [Header("Rotation Settings")]
    public float rotateSpeed;

    [Header("Jump Settings")]
    public Transform feetPos;
    public LayerMask groundLayer;
    public float groundCheckDistance;
    public float jumpForce;
    public AnimationCurve fallDamageCurve;

    [Header("Climb Settings")]
    public LayerMask ladderLayer;
    public float ladderCheckDistance;
    public Transform ladderCheckPosition;
    public float climbSpeed;
    public float climbDownSpeed;

    [Header("Roll Settings")]
    public int rollStaminaCost;
    public float rollTime;
    public float rollCoolTime;
    public float rollSpeed;
    public AnimationCurve rollSpeedCurve;
    public float colliderHeightWhenRolling;

    [Header("States")]
    public float maxYPos;
    public float curCameraX;

    public Vector2 moveInput;
    public Vector3 currentRightDirection;
    public Vector3 currentForwardDirection;

    public bool grounded;
    public bool climbing;
    public bool climbingDown;
    public bool stickToLadder;
    public Vector3 ladderCheckDirection;

    public bool rolling;
    public bool rollReloaded;
    public float rolledTime;
    public Vector2 rollDirection;

    public Vector3 lastCamPos;
    public bool paused;
    



    public void Awake()
    {

        //get variables

        rb = GetComponent<Rigidbody>();
        combatManager = GetComponent<CombatMovement>();
        ps = GetComponent<PlayerScript>();
        cc = GetComponent<CapsuleCollider>();



        cameraManager = um.cameraManager;
        im = um.inventoryManager;
        cinemaManager = um.cinemaManager;
        sm = um.soundManager;




        cam = Camera.main.transform;



        //set variables
        rollReloaded = true;

        maxYPos = transform.position.y;
    }

    public void Update()
    {
        paused = cinemaManager.playingCinema;
        if (ps.dead)
        {
            cam.transform.position = lastCamPos;
            ApplyCamShake();
            return;
        }
        CheckFallDamage();
        MoveAxis();
        MoveCamera();
        Rotate();
        Animate();
        CheckGrounded();
        JumpMovement();
        RollMovement();
        SetParticle();
        lastCamPos = cam.transform.position;
    }
    public void MoveAxis()
    {
        if (rolling)
            return;



        Vector2 preMoveInput = moveInput;
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));


        if (paused)
        {
            moveInput = Vector2.zero;
        }



        bool preClimbingState = climbing;
        bool preClimbingDownState = climbingDown;

        if (!stickToLadder)
        {
            ladderCheckDirection = currentForwardDirection;
        }
        bool preStickToLadderState = stickToLadder;

        stickToLadder = Physics.Raycast(ladderCheckPosition.position, ladderCheckDirection.normalized, ladderCheckDistance, ladderLayer) && !rolling;
        climbing = false;
        climbingDown = false;
        if (stickToLadder)
        {

            climbing = moveInput.y == 1 && !paused;
            climbingDown = moveInput.y == -1 && !paused;
        }
        else
        {
            if (preStickToLadderState)
            {
                //fall
                FallOffOfClimb();
            }
        }


        if(!combatManager.attacking)
            ResetForwardDirection();

        //move
        Vector3 move_horizontal = currentRightDirection * Input.GetAxisRaw("Horizontal");
        Vector3 move_vertical = (climbing || climbingDown) ? Vector3.zero : currentForwardDirection * Input.GetAxisRaw("Vertical");

        float speed = CalculatedRunSpeed();

        Vector3 move = (move_horizontal + move_vertical).normalized * speed;

        if (paused)
        {
            move = Vector3.zero;
        }


        if (stickToLadder)
        {
            if (climbing)
            {
                rb.velocity = new Vector3(move.x, climbSpeed, move.z);
            }
            else if (climbingDown)
            {
                rb.velocity = new Vector3(move.x, -climbDownSpeed, move.z);
            }
            else
            {
                rb.velocity = new Vector3(move.x, 0, move.z);
            }
        }
        else
        {
            rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
        }
        
    }
    public float CalculatedRunSpeed()
    {
        float result = 1;
        result *= (Input.GetKey(KeyCode.LeftShift) || alwaysRun) ? runSpeed : walkSpeed;
        if (combatManager.attacking)
        {
            result *= combatManager.currentWeapon.walkSpeedWhenAttak;
        }
        if (combatManager.blocking)
        {
            result *= combatManager.currentShield.walkSpeedWhenBlocking;
        }



        result *= CalculateRunAnimationSpeed();


        return result;
    }
    public float CalculateRunAnimationSpeed()
    {
        float result = 1;
        result*=combatManager.currentWeapon.runSpeedWhenHolding;
        if (im.usingItem)
        {
            result *= im.currentlyUsingItem.walkSpeedWhenUsing;
        }
        return result;
    }
    public void ResetForwardDirection()
    {
        currentRightDirection = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        currentForwardDirection = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
    }
    public void MoveCamera()
    {
        if (!paused)
        {
            float xAxis = Input.GetAxis("Mouse X");
            float yAxis = -Input.GetAxis("Mouse Y");
            cam.eulerAngles += new Vector3(0, xAxis, 0) * cameraSensitivity;
            curCameraX += -Input.GetAxis("Mouse Y") * cameraSensitivity;
            curCameraX = Mathf.Clamp(curCameraX, minCamRot, maxCamRot);
            cam.eulerAngles = new Vector3(curCameraX, cam.eulerAngles.y, cam.eulerAngles.z);
        }
        cam.position = transform.position - cam.forward.normalized * cameraDistance;

        ApplyCamShake();

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward.normalized, out hit, cameraDistance, groundLayer))
        {
            cam.position = transform.position - cam.forward.normalized * (Vector3.Distance(transform.position, hit.point) - 0.5f);
        }
        else if (Physics.Raycast(transform.position, -cam.forward.normalized, out hit, cameraDistance, groundLayer))
        {
            cam.position = transform.position - cam.forward.normalized * (Vector3.Distance(transform.position, hit.point) - 0.5f);
        }


    }
    public void ApplyCamShake()
    {
        cam.position += cameraManager.curCamShakeOffset;

    }
    public void Rotate()
    {
        if (paused)
            return;
        if (moveInput == Vector2.zero && !rolling && !combatManager.attacking && !combatManager.blocking)
            return;
        
        

        Vector2 input = stickToLadder ? new Vector2(ladderCheckDirection.x, ladderCheckDirection.z) : new Vector2(rb.velocity.x, rb.velocity.z);
        float angle = 




            (
            combatManager.blocking ? Mathf.Atan2(currentForwardDirection.x, currentForwardDirection.z)
            :
            (
            (combatManager.attacking ? (Mathf.Atan2(combatManager.attackingDirection.x, combatManager.attackingDirection.y)): 
            
            (rolling ? Mathf.Atan2(rollDirection.x, rollDirection.y) :
            
            Mathf.Atan2(input.x, input.y)))
            
            )
            )
            * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, angle, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, (im.usingItem ? im.currentlyUsingItem.rotSpeedWhenUsing : 1) * rotateSpeed * Time.deltaTime);
    }
    public void Animate()
    {
        an.SetBool("walking", moveInput != Vector2.zero && ((!Input.GetKey(KeyCode.LeftShift) && !alwaysRun) || im.usingItem) && !stickToLadder && !combatManager.blocking);
        an.SetBool("running", moveInput != Vector2.zero && (Input.GetKey(KeyCode.LeftShift) || alwaysRun) && !stickToLadder && !combatManager.blocking && !im.usingItem);
        an.SetBool("idle", moveInput == Vector2.zero && !stickToLadder && !combatManager.blocking);





        an.SetBool("climbing", stickToLadder && !combatManager.blocking);
        an.SetFloat("climbSpeed", climbing ? 1 : (climbingDown ? -1 : 0));
        an.SetFloat("runSpeed", CalculateRunAnimationSpeed());
    }

    public void CheckGrounded()
    {
        bool preGroundedState = grounded;
        grounded = Physics.OverlapBox(feetPos.position, new Vector3(0.25f, 0.5f, 0.25f), Quaternion.identity, groundLayer).Length != 0;
        if (grounded && rb.velocity.y <= 0)
        {
            EndJump();
        }
        if(!preGroundedState && grounded&&rb.velocity.y<=1)
        {
            OnLand();
        }
    }

    public void JumpMovement()
    {
        if (paused)
            return;
        if (im.usingItem)
            return;
        if (Input.GetKeyDown(KeyCode.Space) && grounded && !rolling && !combatManager.attacking)
        {
            StartJump();
        }
    }

    public void StartJump()
    {
        sm.PlaySound("Jump", 1);
        an.ResetTrigger("endJump");
        an.SetTrigger("jump");
        rb.velocity = Vector3.up;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);


        GameObject p = Instantiate(jumpParticlePrefab);
        p.transform.position = jumpParticlePos.position;
        Destroy(p, 5);
    }
    public void EndJump()
    {
        if (combatManager.attacking)
            return;
        an.ResetTrigger("jump");
        an.SetTrigger("endJump");
    }
    public void FallOffOfClimb()
    {
        if (combatManager.attacking)
            return;
        an.ResetTrigger("endJump");
        an.SetTrigger("jump");
    }

    public void RollMovement()
    {
        if (combatManager.attacking)
            return;
        if (rolling)
        {
            KeepRoll();
        }
        if (paused)
            return;
        if (im.usingItem)
            return;
        if (Input.GetKeyDown(KeyCode.LeftShift) && rollReloaded && moveInput != Vector2.zero && !stickToLadder && ps.stamina>= rollStaminaCost)
        {
            StartRoll();
        }
    }

    public void KeepRoll()
    {
        float speed = rollSpeed * rollSpeedCurve.Evaluate(rolledTime/rollTime);
        rolledTime += Time.deltaTime;
        rb.velocity = new Vector3(rollDirection.x * speed, rb.velocity.y, rollDirection.y * speed);
    }

    public void StartRoll()
    {
        ps.stamina -= rollStaminaCost;
        ps.OnStaminaUsed(rollStaminaCost);
        rolledTime = 0;
        rolling = true;
        rollReloaded = false;
        an.ResetTrigger("endRoll");
        an.SetTrigger("roll");
        Invoke(nameof(PlayRollSound), 0.1f);

        rollDirection = new Vector2(rb.velocity.x, rb.velocity.z).normalized;


        float originCCHeight = cc.height;
        cc.height *= colliderHeightWhenRolling;
        cc.center += Vector3.down * (originCCHeight * (1 - colliderHeightWhenRolling) * 0.5f);


        Invoke(nameof(EndRoll), rollTime);
        Invoke(nameof(ReloadRoll), rollCoolTime);
    }
    public void PlayRollSound()
    {
        sm.PlaySound("Roll", 0.8f);
    }
    public void EndRoll()
    {
        if (combatManager.attacking)
            return;

        cc.center = Vector3.zero;
        cc.height /= colliderHeightWhenRolling;

        rolling = false;
        an.ResetTrigger("roll");
        an.SetTrigger("endRoll");
    }
    public void ReloadRoll()
    {

        rollReloaded = true;
    }

    public void OnLand()
    {
        GameObject p = Instantiate(landParticlePrefab);
        p.transform.position = landParticlePos.position;
        Destroy(p, 5);
        sm.PlaySound("Land", 1);
        float fallDistance = maxYPos - transform.position.y;
        if (fallDistance > 0)
        {
            int dam = Mathf.RoundToInt(fallDamageCurve.Evaluate(Mathf.Clamp(fallDistance, 0, 100)));
            if (dam > 0)
            {
                ps.TakeDamage(dam, 1);
            }
        }
    }

    public void SetParticle()
    {
        var walkEmission = walkParticle.emission;
        walkEmission.rateOverTime = (moveInput == Vector2.zero || !grounded) ? 0 : walkParticleDefaultEmissionRate * CalculatedRunSpeed();
    }
    public void CheckFallDamage()
    {
        if (!grounded && !stickToLadder)
        {
            if (transform.position.y > maxYPos)
                maxYPos = transform.position.y;
        }
        else
        {
            maxYPos = float.MinValue;
        }
    }
}
