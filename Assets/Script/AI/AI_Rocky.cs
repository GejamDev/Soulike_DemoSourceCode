using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AI_Rocky : MonoBehaviour
{

    [Header("Variables")]
    public Animator an;
    public EnemyAttack damageZone;
    public Image hpBar;
    public GameObject statUI;
    public Cinema deathCinema;

    Rigidbody rb;
    NavMeshAgent nav;



    UniversalManager um;
    SoundManager sm;
    CameraManager cm;
    CinemaManager cinemaM;

    GameObject player;
    PlayerScript ps;

    [Header("Setting")]
    public int maxHp;
    public float barSynchronizeSpeed;
    public float attackTime;
    public float attackRange;
    public int attackDamage;
    public float attackMaintainTime;
    public float moveSpeed;
    public float rotateSpeed;
    public float navMeshTargetResetTick;
    public float attackCoolTime;
    public float damageDelay;
    public float attackCamShakeTime;
    public float attackCamShakeIntensity;
    public GameObject damageParticle;

    [Header("Status")]
    public int hp;
    public bool inBattle;
    public bool running;
    public bool attacking;
    public bool reloaded;
    public bool dead;

    void Awake()
    {
        um = FindObjectOfType<UniversalManager>();

        sm = um.soundManager;
        cm = um.cameraManager;
        cinemaM = um.cinemaManager;

        rb = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();

        player = um.player;
        ps = player.GetComponent<PlayerScript>();


        reloaded = true;

        damageZone.damage = attackDamage;
        damageZone.gameObject.SetActive(false);

        hp = maxHp;

        InvokeRepeating(nameof(SetTarget_Tick), 0, navMeshTargetResetTick);

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
        cinemaM.StartCinema(deathCinema);
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




        if (attacking)
        {
            //targetRot = lastTargetAttackRot;

            targetRot = Quaternion.Euler(new Vector3(0, Mathf.Atan2(player.transform.position.x - transform.position.x, player.transform.position.z - transform.position.z) * Mathf.Rad2Deg, 0));
            transform.rotation = Quaternion.Lerp(curRot, targetRot, Time.deltaTime * rotateSpeed);
            return;
        }

        Vector3 direction = new Vector2(player.transform.position.x - transform.position.x, player.transform.position.z - transform.position.z);
        if (Vector3.Distance(transform.position, player.transform.position) <= attackRange && reloaded)//close enough to attack
        {
            nav.updatePosition = false;
            StartAttack();
        }
        else//need to approach player
        {
            bool actuallyMoving = nav.path.corners.Length > 1;
            if (actuallyMoving)
            {
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
        cm.ShakeCamera(attackCamShakeTime, attackCamShakeIntensity, true, damageDelay);
        um.soundManager.PlaySound("RockySwing", 1);
        yield return new WaitForSeconds(damageDelay);
        damageZone.gameObject.SetActive(true);
        yield return new WaitForSeconds(attackMaintainTime);
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
        if (!inBattle||dead)
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
