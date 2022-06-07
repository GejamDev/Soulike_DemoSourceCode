using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [Header("Variables")]
    public UniversalManager um;
    public Animator an;
    PlayerMovement pm;
    CameraManager cameraManager;
    CombatMovement combatMovement;
    InventoryManager im;
    DeathManager dm;
    SoundManager sm;


    [Header("Setting")]
    public GameObject bloodParticle;
    public Image hpBar;
    public int maxHp;
    public float camShakeTime;
    public AnimationCurve damageCamShakePowerCurve;
    public Color vignetteColor_Full;
    public float vignetteIntensity_Full;
    public Color vignetteColor_Death;
    public float vignetteIntensity_Death;
    public float barSynchronizeSpeed;
    public float invincibilityTime;
    public Image staminaBar;
    public int maxStamina;
    public float staminaRegenCoolTime;
    public float staminaRegenSpeed;

    [Header("Status")]
    public float hp;
    public bool invincible;
    public float stamina;
    public bool regeneratingStamina;
    public bool dead;
    public float additionalDamage;
    
    void Awake()
    {
        QualitySettings.SetQualityLevel(5);
        hp = maxHp;
        stamina = maxStamina;

        cameraManager = um.cameraManager;
        im = um.inventoryManager;
        dm = um.deathManager;
        sm = um.soundManager;

        pm = GetComponent<PlayerMovement>();
        combatMovement = GetComponent<CombatMovement>();


        regeneratingStamina = true;
        
    }

    public void Update()
    {
        SetHP();
        if (dead)
        {
            return;

        }
        SetStamina();
    }
    public void SetHP()
    {
        if (hp > maxHp)
        {
            hp = maxHp;
        }
        hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, (float)hp / maxHp, barSynchronizeSpeed * Time.deltaTime);
        
        if (hp <= 0 && !dead)
        {
            sm.PlaySound("Death", 1);
            dm.PlayerDie();
        }
    }
    public void SetStamina()
    {
        if (regeneratingStamina)
        {
            stamina += staminaRegenSpeed * Time.deltaTime;
            if (stamina > maxStamina)
                stamina = maxStamina;
        }

        staminaBar.fillAmount = Mathf.Lerp(staminaBar.fillAmount, (float)stamina / maxStamina, barSynchronizeSpeed * Time.deltaTime);
    }
    public void TakeDamage(float dam, float camShakeMultiplier)
    {
        cameraManager.ShakeCamera(camShakeTime, damageCamShakePowerCurve.Evaluate(dam) * camShakeMultiplier, true, 0);
        hp -= dam;
        sm.PlaySound("Hit", 1);
        invincible = true;
        Invoke(nameof(DisableInvincibility), invincibilityTime);
    }
    public void DisableInvincibility()
    {
        invincible = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyAttack"))
        {
            if (pm.rolling || invincible)
            {
                return;
            }
            EnemyAttack em = other.GetComponent<EnemyAttack>();




            if (combatMovement.blocking)
            {
                Vector2 attackedPos = new Vector2(em.transform.position.x, em.transform.position.z) - new Vector2(transform.position.x, transform.position.z);

                float attackedAngle = (Mathf.Atan2(attackedPos.x, attackedPos.y) * Mathf.Rad2Deg + 1080) % 360;
                if (em.GetComponent<Rigidbody>() != null)
                {
                    return;
                }


                float currentPlayerAngle = (transform.eulerAngles.y + 1080) % 360;


                if (Mathf.Abs(currentPlayerAngle - attackedAngle) <= combatMovement.currentShield.blockRange * 0.5f)
                {
                    OnBlock();
                    return;
                }
                else if(Mathf.Abs(currentPlayerAngle - attackedAngle) <= 360 - combatMovement.currentShield.blockRange * 0.5f)
                {
                    OnBlock();
                    return;
                }
            }
            if (em == null)
                return;

            GameObject b = Instantiate(bloodParticle);
            b.transform.position = transform.position;
            b.transform.LookAt(em.transform.position);



            TakeDamage(em.damage, 1);
        }
    }
    public void OnBlock()
    {
        Debug.Log("blocked");
        sm.PlaySound("ShieldBlock", 1);
    }
    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("EnemyAttack"))
        {
            if (pm.rolling || invincible)
            {
                return;
            }
            EnemyAttack em = collision.gameObject.GetComponent<EnemyAttack>();




            if (combatMovement.blocking)
            {
                Vector2 attackedPos = new Vector2(em.transform.position.x, em.transform.position.z) - new Vector2(transform.position.x, transform.position.z);

                float attackedAngle = (Mathf.Atan2(attackedPos.x, attackedPos.y) * Mathf.Rad2Deg + 1080) % 360;
                if (em.GetComponent<Rigidbody>() != null)
                {
                    attackedAngle = (-em.transform.eulerAngles.y + 1080) % 360;
                }


                float currentPlayerAngle = (transform.eulerAngles.y + 1080) % 360;
                
                if (Mathf.Abs(currentPlayerAngle - attackedAngle) <= combatMovement.currentShield.blockRange * 0.5f)
                {
                    OnBlock();
                    return;
                }
                else if (Mathf.Abs(currentPlayerAngle - attackedAngle) <= 360 - combatMovement.currentShield.blockRange * 0.5f)
                {

                    OnBlock();
                    return;
                }
            }


            GameObject b = Instantiate(bloodParticle);
            b.transform.position = transform.position;
            b.transform.LookAt(em.transform.position);



            TakeDamage(em.damage, 1);
        }
    }

    public void OnHit(Weapon weapon)
    {
        if (weapon == null)
            return;
        if (weapon.hasCamShakeOnHit)
        {
            cameraManager.ShakeCamera(weapon.camShakeTimeOnHit, weapon.camShakeIntensityOnHit, weapon.camShakeFadeOnHit, 0);
            if (weapon.hitSound.Length > 0)
            {
                sm.PlaySound(weapon.hitSound, 1);
            }
        }
    }

    public void OnStaminaUsed(float usedAmount)
    {
        CancelInvoke(nameof(RestartRegeneratingStamina));
        regeneratingStamina = false;
        Invoke(nameof(RestartRegeneratingStamina), staminaRegenCoolTime);
    }
    public void RestartRegeneratingStamina()
    {
        regeneratingStamina = true;
    }
    public void PlayDeathAnimation()
    {
        an.SetTrigger("die");
    }
}
