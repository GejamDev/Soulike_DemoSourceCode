using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AI_Zombie : MonoBehaviour
{

    [Header("Variables")]
    public Animator an;
    public EnemyAttack damageZone;
    public Image hpBar;
    public GameObject statUI;

    Rigidbody rb;
    NavMeshAgent nav;
    LineRenderer lr;



    UniversalManager um;

    CameraManager cm;

    GameObject player;
    PlayerScript ps;
    CombatMovement combatM;
    SoundManager sm;

    [Header("Setting")]
    public int maxHp;
    public float barSynchronizeSpeed;
    public float attackTime;
    public float attackRange;
    public int attackDamage;
    public int laserDamage;
    public float attackMaintainTime;
    public float moveSpeed;
    public float moveSpeedWhenAttacking;
    public float rotateSpeed;
    public float navMeshTargetResetTick;
    public float attackCoolTime;
    public float damageDelay;
    public float attackCamShakeTime;
    public float attackCamShakeIntensity;
    public GameObject damageParticle;
    public GameObject attackParticle;
    public float laserTick_Min;
    public float laserTick_Max;
    public float laserTime;
    public float laserDelay;
    public Transform laserStart;
    public float laserCamShakeTime;
    public float laserCamShakeIntensity;
    public LayerMask groundLayer;
    public ParticleSystem laserParticle;

    [Header("Status")]
    public int hp;
    public bool inBattle;
    public bool running;
    public bool attacking;
    public bool reloaded;
    public bool dead;
    public bool lasering;
    public bool emittingLaser;

    void Awake()
    {
        um = FindObjectOfType<UniversalManager>();

        cm = um.cameraManager;
        sm = um.soundManager;

        rb = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        lr = GetComponent<LineRenderer>();

        player = um.player;
        ps = player.GetComponent<PlayerScript>();
        combatM = player.GetComponent<CombatMovement>();


        reloaded = true;

        damageZone.damage = attackDamage;
        damageZone.gameObject.SetActive(false);

        hp = maxHp;
        nav.speed = moveSpeed;

        InvokeRepeating(nameof(SetTarget_Tick), 0, navMeshTargetResetTick);
        StartCoroutine(Laser_Tick());
    }

    IEnumerator Laser_Tick()
    {
        while (true)
        {
            yield return new WaitUntil(() => !lasering);
            yield return new WaitUntil(() => inBattle);
            yield return new WaitForSeconds(Random.Range(laserTick_Min, laserTick_Max));
            yield return new WaitUntil(() => !attacking);

            StartLaser();
        }
    }
    public void StartLaser()
    {
        sm.PlaySound("ZombieLaserCharge", 1);
        lasering = true;
        an.ResetTrigger("endLaser");
        an.SetTrigger("startLaser");
        Invoke(nameof(ShootLaser), laserDelay);
        Invoke(nameof(EndLaser), laserTime);
    }
    public void ShootLaser()
    {
        cm.ShakeCamera(laserCamShakeTime, laserCamShakeIntensity, true, 0);
        emittingLaser = true;
        sm.PlaySound("ZombieLaser", 1);
    }
    public void EndLaser()
    {
        emittingLaser = false;
        an.ResetTrigger("startLaser");
        an.SetTrigger("endLaser");
        lasering = false;
    }


    public void StartBattle()
    {
        inBattle = true;
    }


    void Update()
    {

        Animate();
        SetHP();
        if (dead)
            return;


        if (inBattle)
        {
            //인 배틀 중에는! 
            BattleBehaviour();
        }
        else
        {
            StayIdle();
        }

    }
    public void SetHP()
    {
        hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, (float)hp / maxHp, barSynchronizeSpeed * Time.deltaTime);
        if (hp <= 0 && !dead)
        {
            Die();
        }
    }
    public void Die()
    {
        dead = true;
        an.SetTrigger("death");
        nav.enabled = false;
        rb.isKinematic = true;
        GetComponent<CapsuleCollider>().enabled = false;
        StopAllCoroutines();
        statUI.SetActive(false);
    }
    public void StayIdle()
    {
        running = false;
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }
    public void BattleBehaviour()
    {
        Quaternion curRot = transform.rotation;
        Quaternion targetRot;
        lr.positionCount = 0;
        var p_emission = laserParticle.emission;
        p_emission.rateOverTime = 0;
        if (lasering)
        {

            if (emittingLaser)
            {

                Vector3 target = combatM.blocking ? combatM.shieldBundle.position : player.transform.position;
                Vector3 startPos = laserStart.position;
                bool intervented = false;
                RaycastHit hit;
                if(Physics.Raycast(startPos, (target-startPos).normalized, out hit, Vector3.Distance(target, startPos), groundLayer))
                {
                    intervented = true;
                    target = hit.point;
                }
                else if(Physics.Raycast(target, (startPos - target).normalized, out hit, Vector3.Distance(target, startPos), groundLayer))
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




            nav.updatePosition = false;

            targetRot = Quaternion.Euler(new Vector3(0, Mathf.Atan2(player.transform.position.x - transform.position.x, player.transform.position.z - transform.position.z) * Mathf.Rad2Deg, 0));
            transform.rotation = Quaternion.Lerp(curRot, targetRot, Time.deltaTime * rotateSpeed);
            return;
        }
        nav.updatePosition = true;

        if (attacking)
        {
            //targetRot = lastTargetAttackRot;

            nav.speed = moveSpeedWhenAttacking;
            targetRot = Quaternion.Euler(new Vector3(0, Mathf.Atan2(player.transform.position.x - transform.position.x, player.transform.position.z - transform.position.z) * Mathf.Rad2Deg, 0));
            transform.rotation = Quaternion.Lerp(curRot, targetRot, Time.deltaTime * rotateSpeed);
            return;
        }

        Vector3 direction = new Vector2(player.transform.position.x - transform.position.x, player.transform.position.z - transform.position.z);
        if (Vector3.Distance(transform.position, player.transform.position) <= attackRange && reloaded)//close enough to attack
        {
            nav.speed = moveSpeedWhenAttacking;
            StartAttack();
        }
        else//need to approach player
        {
            bool actuallyMoving = nav.path.corners.Length > 1;
            if (actuallyMoving)
            {
                nav.speed = moveSpeed;
                nav.updatePosition = true;
                running = true;
                targetRot = Quaternion.Euler(new Vector3(0, Mathf.Atan2(nav.path.corners[1].x - transform.position.x, nav.path.corners[1].z - transform.position.z) * Mathf.Rad2Deg, 0));
            }
            else
            {
                nav.updatePosition = false;
                running = false;
                targetRot = Quaternion.Euler(new Vector3(0, Mathf.Atan2(player.transform.position.x - transform.position.x, player.transform.position.z - transform.position.z) * Mathf.Rad2Deg, 0));
            }

            transform.rotation = Quaternion.Lerp(curRot, targetRot, Time.deltaTime * rotateSpeed);
        }

    }
    public void StartAttack()
    {
        reloaded = false;
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        attacking = true;
        an.ResetTrigger("endAttack");
        an.SetTrigger("attack");
        Invoke(nameof(EndAttack), attackTime);
        StartCoroutine(Attack());
    }
    IEnumerator Attack()
    {
        sm.PlaySound("ZombieSwing", 1);
        yield return new WaitForSeconds(damageDelay);
        cm.ShakeCamera(attackCamShakeTime, attackCamShakeIntensity, true, 0);
        damageZone.gameObject.SetActive(true);
        GameObject p = Instantiate(attackParticle);
        Destroy(p, 10);
        p.transform.position = damageZone.transform.position + Vector3.down * 0.3f;

        cm.ShakeCamera(attackCamShakeTime, attackCamShakeIntensity, true, 0);

        yield return new WaitForSeconds(attackMaintainTime);
        sm.PlaySound("ZombieSmash", 1);
        damageZone.gameObject.SetActive(false);



        yield return null;
    }
    public void EndAttack()
    {
        attacking = false;
        an.ResetTrigger("attack");
        an.SetTrigger("endAttack");
        Invoke(nameof(Reload), attackCoolTime);
    }
    public void Reload()
    {
        reloaded = true;
    }
    public void Animate()
    {
        an.SetBool("running", running);
    }
    public void SetTarget_Tick()
    {
        if (!inBattle || dead)
        {
            return;
        }
        nav.SetDestination(player.transform.position + Vector3.right * Random.Range(-0.01f, 0.01f));
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
}
