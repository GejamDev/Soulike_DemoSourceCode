using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FlyingZombieState
{
    Rest,
    Moving,
    Lasering
}
public class AI_FlyingZombie : MonoBehaviour
{
    [Header("Variables")]
    UniversalManager um;
    GameObject player;
    PlayerScript ps;
    CombatMovement combatM;
    Rigidbody rb;
    CameraManager cm;
    LineRenderer lr;
    public Animator an;
    public GameObject statUI;
    public Image hpUI;
    public LayerMask groundLayer;
    public GameObject damageParticle;
    public Vector3 moveTargetPosFixed;


    [Header("Setting")]
    public int maxHp;
    public float flySpeedSynchronizeSpeed;
    public float flySpeed;
    public float rotSpeed;
    public float minDistance;
    public FlyingZombieState[] attackStateArray;

    [Header("Laser")]
    public int laserDamage;
    public float laserRestTime;
    public float laserAttackReadyTime;
    public Transform laserStart;
    public ParticleSystem laserParticle;
    public float laserMinDistance;
    public float laserStartDelay;
    public float laserTime;
    public bool emittingLaser;
    public float laserCamShakeTime;
    public float laserCamShakeIntensity;
    public float laserXRotLimit;

    [Header("Status")]
    public FlyingZombieState currentState;
    public bool nextAttackStateDetermined;
    public FlyingZombieState determinedNextAttackState;
    public Vector3 velocity;
    public bool inBattle;
    public int hp;
    public bool death;
    public bool inAttack;
    public bool readyForNewAttack;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();



        um = FindObjectOfType<UniversalManager>();

        player = um.player;
        ps = player.GetComponent<PlayerScript>();
        combatM = player.GetComponent<CombatMovement>();

        rb = GetComponent<Rigidbody>();

        cm = um.cameraManager;


        hp = maxHp;
        nextAttackStateDetermined = false;
        readyForNewAttack = true;
    }

    public void StartBattle()
    {
        inBattle = true;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartBattle();
        }

        SetHP();

        if (death)
            return;

        if (inBattle)
        {
            BattleBehaviour();
        }
        else
        {
            StayIdle();
        }
    }
    public void SetHP()
    {
        hpUI.fillAmount = (float)hp / maxHp;
    }
    public void TakeDamage(int dam, Weapon weapon)
    {
        hp -= dam;
        if (weapon != null)
        {
            ps.OnHit(weapon);
        }
        GameObject p = Instantiate(damageParticle);
        p.transform.position = transform.position;
        p.transform.LookAt(player.transform.position);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            PlayerAttack pa = other.GetComponent<PlayerAttack>();
            TakeDamage(pa.damage * (int)ps.additionalDamage, pa.usedWeapon == null ? null : pa.usedWeapon);
        }
    }
    public void StayIdle()
    {
        rb.velocity = Vector3.zero;
        an.SetBool("idle", true);
    }
    public void BattleBehaviour()
    {
        switch (currentState)
        {
            case FlyingZombieState.Rest:
                Behaviour_Rest();
                break;
            case FlyingZombieState.Moving:
                Behaviour_Moving();
                break;
            case FlyingZombieState.Lasering:
                Behaviour_Lasering();
                break;


            default:
                Debug.LogError("Unknown flying zombie state:" + currentState.ToString());
                break;
        }
    }
    #region battle behaviour
    public void Behaviour_Rest()
    {
        rb.velocity = Vector3.zero;
        an.SetBool("idle", true);

        //laser
        var p_emission = laserParticle.emission;
        p_emission.rateOverTime = 0;
        lr.positionCount = 0;
    }
    public void Behaviour_Moving()
    {
        if (!nextAttackStateDetermined)
        {
            determinedNextAttackState = attackStateArray[Random.Range(0, attackStateArray.Length)];
            nextAttackStateDetermined = true;
        }

        float attackingDistance = 0;
        switch (determinedNextAttackState)
        {
            case FlyingZombieState.Lasering:
                attackingDistance = laserMinDistance;
                break;
            default:
                Debug.LogError("Unknown flying zombie attacking state:" + currentState.ToString());
                break;
        }
        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist <= attackingDistance && readyForNewAttack)
        {
            currentState = determinedNextAttackState;
        }
        else
        {
            if (dist > minDistance)
            {

                //animate
                an.SetBool("idle", false);

                //move
                Vector3 targetVel = (player.transform.position + moveTargetPosFixed - transform.position).normalized * flySpeed;
                rb.velocity = Vector3.Lerp(rb.velocity, targetVel, Time.deltaTime * flySpeedSynchronizeSpeed);
                
                RotateTowardPlayer();
            }
            else
            {
                an.SetBool("idle", true);
                rb.velocity = Vector3.zero;

                RotateTowardPlayer();
            }
        }

        //laser
        var p_emission = laserParticle.emission;
        p_emission.rateOverTime = 0;
        lr.positionCount = 0;
    }


    public void Behaviour_Lasering()
    {
        RotateTowardPlayer();
        if (!inAttack)
        {
            //start attack
            StartCoroutine(LaserAttack());
        }
        else
        {
            //in attack
            rb.velocity = Vector3.zero;
            //transform.LookAt(player.transform.position);
            //transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);


            lr.positionCount = 0;
            var p_emission = laserParticle.emission;
            p_emission.rateOverTime = 0;

            if (emittingLaser)
            {
                Vector3 target = combatM.blocking ? combatM.shieldBundle.position : player.transform.position;
                Vector3 startPos = laserStart.position;
                bool intervented = false;
                RaycastHit hit;
                if (Physics.Raycast(startPos, (target - startPos).normalized, out hit, Vector3.Distance(target, startPos), groundLayer))
                {
                    intervented = true;
                    target = hit.point;
                }
                else if (Physics.Raycast(target, (startPos - target).normalized, out hit, Vector3.Distance(target, startPos), groundLayer))
                {
                    intervented = true;
                    target = hit.point;
                }

                p_emission.rateOverTime = 50;

                var p_shape = laserParticle.shape;
                p_shape.radius = Vector3.Distance(startPos, target) * 5;
                laserParticle.transform.position = (startPos + target) * 0.5f;
                laserParticle.transform.LookAt(target);


                lr.positionCount = 2;
                lr.SetPositions(new Vector3[2] { startPos, target });
                if (!combatM.blocking && !intervented)
                {
                    ps.TakeDamage(laserDamage * Time.deltaTime, 0.1f);
                }

            }

        }
    }


#endregion

    public void RotateTowardPlayer()
    {
        Vector2 dir = new Vector2(player.transform.position.x - transform.position.x, player.transform.position.z - transform.position.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg, 0), rotSpeed * Time.deltaTime);
    }
    
    IEnumerator LaserAttack()
    {
        inAttack = true;

        an.SetTrigger("laser");


        yield return new WaitForSeconds(laserStartDelay);
        //start attack
        cm.ShakeCamera(laserCamShakeTime, laserCamShakeIntensity, true, 0);


        emittingLaser = true;


        yield return new WaitForSeconds(laserTime);

        //end attack
        emittingLaser = false;
        an.SetTrigger("endLaser");

        inAttack = false;

        readyForNewAttack = false;
        currentState = FlyingZombieState.Rest;
        yield return new WaitForSeconds(laserRestTime);
        currentState = FlyingZombieState.Moving;
        yield return new WaitForSeconds(laserAttackReadyTime);
        readyForNewAttack = true;
    }



}