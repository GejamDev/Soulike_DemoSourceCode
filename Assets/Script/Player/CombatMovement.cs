using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CombatMovement : MonoBehaviour
{
    public UniversalManager um;
    CameraManager cm;
    InventoryManager im;
    SoundManager sm;
    PlayerMovement pm;
    PlayerScript ps;
    Animator an;
    AnimatorOverrideController aoc;
    public Weapon debugWeapon;


    [Header("Weapon")]
    public Weapon defaultWeapon;
    public Weapon currentWeapon;
    public Shield defaultShield;
    public Shield currentShield;


    [Header("Status")]
    public bool attacking;
    public bool reloaded;
    public bool blocking;
    public Vector2 attackingDirection;

    [Header("Objects")]
    public Transform handTransform;
    public Transform weaponBundle;
    public Transform shieldBundle;

    [Header("Pickable")]
    public GameObject pickableWeaponPrefab;
    public float pickableSpawnDistance;
    public float pickableThrowForce;
    

    void Awake()
    {
        //get variables
        pm = GetComponent<PlayerMovement>();
        ps = GetComponent<PlayerScript>();

        cm = um.cameraManager;
        im = um.inventoryManager;
        sm = um.soundManager;



        //still variables from movement script
        an = pm.an;



        aoc = new AnimatorOverrideController(an.runtimeAnimatorController);
        an.runtimeAnimatorController = aoc;


        //set to default weapon
        ChangeWeaponTo(defaultWeapon);
        ChangeShieldTo(defaultShield);
    }

    public void ChangeWeaponTo(Weapon weapon)
    {
        Weapon preWeapon = currentWeapon == null ? defaultWeapon : currentWeapon;


        //change weapon setting
        currentWeapon = weapon;


        //reload
        reloaded = false;
        CancelInvoke(nameof(Reload));
        Invoke(nameof(Reload), currentWeapon.reloadTime);



        //change attack animation
        aoc[defaultWeapon.attackAnimation.name] = currentWeapon.attackAnimation;
        

        //enable  weapon object
        for(int i =0;  i < weaponBundle.childCount; i++)
        {
            GameObject w = weaponBundle.GetChild(i).gameObject;
            w.SetActive(w.name == currentWeapon.name);
        }

        //throw away old weapon
        if(preWeapon != null)
        {
            if (preWeapon != defaultWeapon)
            {
                GameObject p = Instantiate(pickableWeaponPrefab);
                p.transform.position = transform.position + transform.forward * pickableSpawnDistance;
                Rigidbody rb = p.GetComponent<Rigidbody>();
                rb.AddForce(transform.forward.normalized * pickableThrowForce, ForceMode.Impulse);
                PickableWeapon pa = p.GetComponent<PickableWeapon>();
                pa.weapon = preWeapon;
                pa.SetSettings();
            }
        }
    }
    public void ChangeShieldTo(Shield shield)
    {
        //change weapon setting
        currentShield = shield;

        
        

        //enable  weapon object
        for (int i = 0; i < shieldBundle.childCount; i++)
        {
            GameObject s = shieldBundle.GetChild(i).gameObject;
            if(shield == null)
            {
                s.SetActive(false);
            }
            else
            {
                s.SetActive(s.name == currentShield.name);
            }
        }
    }

    void Update()
    {
        AttackBehaviour();
        ShieldBehaviour();
    }
    public void AttackBehaviour()
    {
        if (im.usingItem)
            return;
        if (!attacking && Input.GetMouseButtonDown(0) && !pm.rolling && pm.grounded && !pm.stickToLadder && reloaded && !blocking && !pm.paused)
        {
            StartAttack();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ChangeWeaponTo(defaultWeapon);
        }
    }
    public void ShieldBehaviour()
    {
        bool preBlockingState = blocking;

        if (currentShield == null)
        {
            blocking = false;
        }
        else if (im.usingItem)
        {
            blocking = false;
        }
        else
        {
            blocking = Input.GetMouseButton(1) && !attacking && !pm.rolling && pm.grounded && !pm.stickToLadder && ps.stamina >= currentShield.staminaCost * Time.deltaTime && !pm.paused;
        }
        if (blocking)
        {
            ps.stamina -= currentShield.staminaCost * Time.deltaTime;
            ps.OnStaminaUsed(currentShield.staminaCost * Time.deltaTime);
        }
        


        if(!preBlockingState && blocking)
        {
            an.ResetTrigger("endBlock");
            an.SetTrigger("startBlock");
        }
        else if(preBlockingState && !blocking)
        {
            an.ResetTrigger("startBlock");
            an.SetTrigger("endBlock");
        }
    }

    void StartAttack()
    {
        reloaded = false;
        attacking = true;
        Invoke(nameof(EndAttack), currentWeapon.attackTime);
        an.ResetTrigger("endAttack");
        an.SetTrigger("attack");
        StartCoroutine(Attack(currentWeapon));
        attackingDirection = new Vector2(pm.cam.forward.x, pm.cam.forward.z).normalized;
    }

    IEnumerator Attack(Weapon weapon)
    {
        if (weapon.hasCamShake)
        {
            cm.ShakeCamera(weapon.camShakeTime, weapon.camShakeIntensity, weapon.camShakeFade, weapon.damageDelay);
        }
        if (weapon.instantSound.Length > 0)
        {
            sm.PlaySound(weapon.instantSound, 1);
        }

        yield return new WaitForSeconds(weapon.damageDelay);
        if (weapon.attackSound.Length > 0)
        {
            sm.PlaySound(weapon.attackSound, 1);
        }
        GameObject a = Instantiate(weapon.attackPrefab);
        Vector3 orgPos = a.transform.position;
        a.transform.SetParent(transform);
        a.transform.localPosition = orgPos;
        a.transform.localEulerAngles = Vector3.zero;
        PlayerAttack pa = a.GetComponent<PlayerAttack>();
        pa.damage = weapon.damage;
        pa.usedWeapon = weapon;
        Destroy(a, weapon.attackLastTime);





        yield return null;
    }

    void EndAttack()
    {
        attacking = false;
        an.ResetTrigger("attack");
        an.SetTrigger("endAttack");
        Invoke(nameof(Reload), currentWeapon.reloadTime);
    }
    public void Reload()
    {
        reloaded = true;
    }
}
