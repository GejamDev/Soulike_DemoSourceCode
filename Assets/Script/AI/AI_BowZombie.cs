using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AI_BowZombie : MonoBehaviour
{

    [Header("Variables")]
    public Animator an;
    public Image hpBar;
    public GameObject statUI;

    Rigidbody rb;



    UniversalManager um;

    CameraManager cm;
    SoundManager sm;

    GameObject player;
    PlayerScript ps;
    CombatMovement combatM;

    [Header("Setting")]
    public float rotateSpeed;
    public int maxHp;
    public float barSynchronizeSpeed;
    public GameObject damageParticle;
    public float reloadSpeed;
    public float shootDelay;
    public GameObject arrow;
    public float shootForce;
    public Transform arrowSpawnPos;
    public int damage;
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

        cm = um.cameraManager;
        sm = um.soundManager;

        rb = GetComponent<Rigidbody>();

        player = um.player;
        ps = player.GetComponent<PlayerScript>();
        combatM = player.GetComponent<CombatMovement>();


        reloaded = true;
        
        hp = maxHp;
    }
    


    public void StartBattle()
    {
        inBattle = true;
    }


    void Update()
    {
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
        rb.isKinematic = true;
        GetComponent<CapsuleCollider>().enabled = false;
        StopAllCoroutines();
        statUI.SetActive(false);
    }
    public void StayIdle()
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }
    public void BattleBehaviour()
    {
        Quaternion curRot = transform.rotation;
        Quaternion targetRot;
        targetRot = Quaternion.Euler(new Vector3(0, Mathf.Atan2(player.transform.position.x - transform.position.x, player.transform.position.z - transform.position.z) * Mathf.Rad2Deg, 0));
        transform.rotation = Quaternion.Lerp(curRot, targetRot, Time.deltaTime * rotateSpeed);
        if (reloaded)
        {
            reloaded = false;
            Invoke(nameof(Reload), reloadSpeed);

            ShootReady();
        }
    }
    public void ShootReady()
    {
        an.SetTrigger("shootArrow");
        Invoke(nameof(ShootArrow), shootDelay);
    }
    public void ShootArrow()
    {
        sm.PlaySound("BowShoot", 1);

        GameObject a = Instantiate(arrow);
        a.transform.position = arrowSpawnPos.position;
        Rigidbody rb = a.GetComponent<Rigidbody>();
        a.transform.LookAt(player.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)));
        rb.AddForce(a.transform.forward.normalized * shootForce * rb.mass, ForceMode.Impulse);
        EnemyAttack ea = a.GetComponent<EnemyAttack>();
        ea.damage = damage;
    }


    public void Reload()
    {
        reloaded = true;
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
