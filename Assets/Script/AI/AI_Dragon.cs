using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class AI_Dragon : MonoBehaviour
{
    [Header("Variables")]
    UniversalManager um;
    BossBattleManager bbm;
    GameObject player;
    PlayerScript ps;
    CombatMovement combatM;
    CameraManager cm;
    CinemaManager cinema;
    SoundManager sm;
    public Animator an;
    public LayerMask groundLayer;
    public GameObject damageParticle;
    public bool reusePattern;


    [Header("Setting")]
    public int maxHp;
    public List<DragonPattern> patterns = new List<DragonPattern>();
    public float defaultRestTime;
    public float patternStartTime;
    public float headRotSpeed;
    public float defaultZPos;
    public float zAxisMoveSpeed;
    public Cinema defeatCinema;

    [Header("ThrowBack")]
    public float throwBackStartDistance;
    public float throwBackTime;

    [Header("Laser")]
    public float laserDamage;
    public bool lasering;
    public bool emittingLaser;
    public float laserStartDelay;
    public float laserTime;
    public float headRotSpeedWhileLasering;
    public GameObject laserHead;
    public LineRenderer laserLR;
    public ParticleSystem laserRibbonParticle;
    public Transform laserStartPos;
    public float laserCamShakeTime;
    public float laserCamShakeIntensity;
    public float defaultCamShakeWhileLasering;

    [Header("Slam")]
    public AnimationCurve slamPosAccuracy;
    public bool slamming;
    public float slamPosZMin;
    public float slamPosZMax;
    public int slamIterationMin;
    public int slamIterationMax;
    public int slamDamage;
    public int slamRockDamage;
    public float slamDamageDelay;
    public float slamCamShakeTime;
    public float slamCamShakeIntensity;
    public EnemyAttack slamAttack;
    public GameObject slamParticle;
    public GameObject slamRock;
    public Transform slamRockSpawnPos;
    public int slamRockSpawnCountMin;
    public int slamRockSpawnCountMax;
    public AnimationCurve slamRockSize;
    public AnimationCurve slamRockTorque;
    public AnimationCurve slamRockForce;
    public float slamRockAdditionalForce_min;
    public float slamRockAdditionalForce_max;
    public float slamRockSpawnPosRandomness;
    public float zAxisMoveSpeedWhileSlamming;

    [Header("Launch Rocket")]
    public GameObject rocketPrefab;
    public Transform rocketLaunchPos;
    public int rocketDamage;
    public float rocketLaunchDelay;

    [Header("Spawn Entities")]
    public GameObject zombie;
    public GameObject bowZombie;
    public Transform randomEntitiesSpawnPos;
    public float spawnDurationMin;
    public float spawnDurationMax;

    [Header("Status")]
    public Quaternion preHeadRot;
    public bool inBattle;
    public int hp;
    public bool death;
    public bool throwingBack;
    public bool inPattern;
    public bool patternUsable;
    public string preUsedPattern;
    public float currentZPos;


    void Awake()
    {



        um = FindObjectOfType<UniversalManager>();

        player = um.player;
        ps = player.GetComponent<PlayerScript>();
        combatM = player.GetComponent<CombatMovement>();
        cinema = um.cinemaManager;
        

        cm = um.cameraManager;
        bbm = um.bossBattleManager;
        sm = um.soundManager;


        hp = maxHp;
        defaultZPos = transform.position.z;
        currentZPos = defaultZPos;

        slamAttack.damage = slamDamage;
        slamAttack.gameObject.SetActive(false);

        StartCoroutine(KeepSpawnEntities());
    }

    public void StartBattle()
    {
        sm.PlaySound("DragonRoar", 1);
        hp = maxHp; 
        inBattle = true;
        bbm.currentBoss = gameObject;
        bbm.maxHp = maxHp;
        bbm.bossName = name;
        an.SetTrigger("intro");
        inPattern = false;
        patternUsable = false;
        Invoke(nameof(ReloadPatternUSable), patternStartTime);
        preUsedPattern = "";
        slamAttack.gameObject.SetActive(false);
    }
    void ReloadPatternUSable()
    {
        patternUsable = true;
    }

    public void Update()
    {

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
        if(bbm.currentBoss == gameObject)
        {
            bbm.hp = hp;
        }
        if (hp <= 0 && !death)
        {
            Die();
        }
    }
    public void Die()
    {
        bbm.hp = 0;
        bbm.currentBoss = null;
        death = true;
        CancelAllPatterns();
        an.SetTrigger("death");
        cinema.StartCinema(defeatCinema);
        sm.PlaySound("DragonDeathRoar", 1);
    }
    public void TakeDamage(int dam, Weapon weapon, Vector3 pos)
    {
        hp -= dam;
        if (weapon != null)
        {
            ps.OnHit(weapon);
        }
        GameObject p = Instantiate(damageParticle);
        p.transform.position = pos;
        p.transform.LookAt(player.transform.position);
    }
    public void TakeDamage(int dam, Weapon weapon)
    {
        TakeDamage(dam, weapon, transform.position);
    }
    void OnTriggerEnter(Collider other)
    {
        if (death)
            return;
        if (other.CompareTag("PlayerAttack"))
        {
            PlayerAttack pa = other.GetComponent<PlayerAttack>();
            TakeDamage(pa.damage * (int)ps.additionalDamage, pa.usedWeapon == null ? null : pa.usedWeapon, other.transform.position);
        }
    }
    public void StayIdle()
    {

    }


    public void BattleBehaviour()
    {
        if(player.transform.position.x>transform.position.x+ throwBackStartDistance)
        {

            //throw
            OnPlayerBack();
        }
        else
        {
            if (!inPattern && patternUsable)
            {
                //start new pattern



                List<string> patternNames = new List<string>();
                foreach(DragonPattern dp in patterns)
                {
                    if(dp.patternName!= preUsedPattern || reusePattern)
                    {
                        for (int i = 0; i < dp.density; i++)
                        {
                            patternNames.Add(dp.patternName);
                        }
                    }
                }
                string executingPattern = patternNames[Random.Range(0, patternNames.Count)];
                StartCoroutine(ExecutePattern(executingPattern));
            }
        }



        var p_emission = laserRibbonParticle.emission;
        if (emittingLaser)
        {
            Vector3 start = laserStartPos.position;
            Vector3 target = combatM.blocking ? combatM.shieldBundle.transform.position : player.transform.position;

            if (!combatM.blocking)
            {
                ps.TakeDamage(laserDamage * Time.deltaTime, 0.1f);
            }
            laserLR.positionCount = 2;
            laserLR.SetPosition(0, start);
            laserLR.SetPosition(1, target);


            laserRibbonParticle.transform.position = (start + target) * 0.5f;

            var p_shape = laserRibbonParticle.shape;
            p_shape.radius = Vector3.Distance(start, target) / laserRibbonParticle.transform.localScale.x * 0.5f;
            laserRibbonParticle.transform.LookAt(target);

            p_emission.rateOverTime = 100;
        }
        else
        {
            laserLR.positionCount = 0;
            p_emission.rateOverTime = 0;
        }

        //move
        float currentZ = transform.position.z;
        float targetZ = currentZPos;
        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(currentZ, targetZ, Time.deltaTime * (slamming ? zAxisMoveSpeedWhileSlamming : zAxisMoveSpeed)));

        Animate();
    }
    void LateUpdate()
    {
        if (!inBattle)
            return;
        //update head
        if (lasering || !slamming)
        {
            LookAtPlayer();
        }
        else
        {
            LookAtNothing();
        }
    }
    public void LookAtPlayer()
    {
        Quaternion curRot = preHeadRot;
        Quaternion target = Quaternion.LookRotation(player.transform.position - laserHead.transform.position);
        laserHead.transform.rotation = Quaternion.Lerp(curRot, target, Time.deltaTime * headRotSpeedWhileLasering);
        preHeadRot = laserHead.transform.rotation;
    }
    public void LookAtNothing()
    {
        Quaternion curRot = preHeadRot;
        Quaternion target = laserHead.transform.rotation;
        laserHead.transform.rotation = Quaternion.Lerp(curRot, target, Time.deltaTime * headRotSpeed);
        preHeadRot = laserHead.transform.rotation;
    }
    public void Animate()
    {
        an.SetBool("fireBreathing", lasering);

    }
    public IEnumerator ExecutePattern(string patternName)
    {
        preUsedPattern = patternName;
        inPattern = true;
        yield return StartCoroutine("Pattern_" + patternName);
        yield return new WaitForSeconds(defaultRestTime);
        inPattern = false;
    }
    public void OnPlayerBack()
    {
        if (inBattle && !throwingBack)
        {
            //end pattern
            CancelAllPatterns();

            //throw
            throwingBack = true;
            an.ResetTrigger("throwBack");
            an.SetTrigger("throwBack");
            Invoke(nameof(ReloadThrowingBack), throwBackTime);
        }
    }
    void ReloadThrowingBack()
    {
        throwingBack = false;
        inPattern = false;
    }
    public void CancelAllPatterns()
    {
        StopAllCoroutines();
        lasering = false;
        emittingLaser = false;
        slamming = true;
    }

    //patterns
    public IEnumerator Pattern_Laser()
    {
        lasering = true;
        sm.PlaySound("DragonLaserCharge", 1);

        yield return new WaitForSeconds(laserStartDelay);

        sm.PlaySound("DragonLaser", 1);

        emittingLaser = true;
        cm.ShakeCamera(laserCamShakeTime, laserCamShakeIntensity, true, 0);
        cm.ShakeCamera(laserTime, defaultCamShakeWhileLasering, false, 0);

        yield return new WaitForSeconds(laserTime);


        lasering = false;
        emittingLaser = false;
        yield return new WaitForSeconds(1.5f);
    }

    public IEnumerator Pattern_Slam()
    {
        int iteration = Random.Range(slamIterationMin, slamIterationMax + 1);
        slamming = true;
        for(int i = 0; i < iteration; i++)
        {
            an.SetTrigger("slam");
            float playerZ = player.transform.position.z;
            float randomness = Random.Range(slamPosZMin, slamPosZMax);
            currentZPos = Mathf.Lerp(playerZ, playerZ, slamPosAccuracy.Evaluate(Random.Range(0, 1f)));
            sm.PlaySound("DragonMove", 1);
            yield return new WaitForSeconds(2.5f);
        }
        slamming = false;
        yield return new WaitForSeconds(1);
    }
    public void OnSlam()
    {
        sm.PlaySound("DragonSlam", 1);
        cm.ShakeCamera(slamCamShakeTime, slamCamShakeIntensity, true, 0);

        slamAttack.gameObject.SetActive(true);
        Invoke(nameof(DisableSlamDamageZone), 0.1f);

        GameObject sp = Instantiate(slamParticle);
        sp.transform.position = slamAttack.transform.position;
        Destroy(sp, 5);

        int rockSpawnCount = Random.Range(slamRockSpawnCountMin, slamRockSpawnCountMax + 1);
        for(int i =0; i < rockSpawnCount; i++)
        {
            GameObject r = Instantiate(slamRock);
            r.transform.position = slamRockSpawnPos.position + new Vector3(Random.Range(-slamRockSpawnPosRandomness, slamRockSpawnPosRandomness), Random.Range(-slamRockSpawnPosRandomness, slamRockSpawnPosRandomness), Random.Range(-slamRockSpawnPosRandomness, slamRockSpawnPosRandomness));
            r.transform.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360))), Quaternion.LookRotation(player.transform.position-r.transform.position), Random.Range(0, 1f));
            r.transform.localScale = Vector3.one*slamRockSize.Evaluate(Random.Range(0, 1f));
            r.GetComponent<SizeAnimator>().originScale = r.transform.localScale;

            Rigidbody rb = r.GetComponent<Rigidbody>();
            rb.AddTorque(new Vector3(slamRockTorque.Evaluate(Random.Range(0, 1f)), slamRockTorque.Evaluate(Random.Range(0, 1f)), slamRockTorque.Evaluate(Random.Range(0, 1f))), ForceMode.Impulse);
            rb.AddForce(Vector3.left * Random.Range(slamRockAdditionalForce_min, slamRockAdditionalForce_max) + r.transform.forward.normalized * slamRockForce.Evaluate(Random.Range(0, 1f)), ForceMode.Impulse);
            EnemyAttack ea = r.GetComponent<EnemyAttack>();
            ea.damage = slamRockDamage;
        }


    }
    public void DisableSlamDamageZone()
    {
        slamAttack.gameObject.SetActive(false);
    }


    public IEnumerator Pattern_Spawn_Zombie()
    {
        an.SetTrigger("slam");
        yield return new WaitForSeconds(1.3f);
        GameObject z = Instantiate(zombie);
        z.SetActive(false);
        z.transform.position = slamRockSpawnPos.position;
        z.SetActive(true);
        z.GetComponent<AI_Zombie>().Invoke("StartBattle", 3);
        yield return new WaitForSeconds(1);
    }

    public IEnumerator Pattern_Spawn_BowZombie()
    {
        an.SetTrigger("slam");
        yield return new WaitForSeconds(1.3f);
        GameObject z = Instantiate(bowZombie);
        z.transform.position = slamRockSpawnPos.position; 
        z.GetComponent<AI_BowZombie>().Invoke("StartBattle", 3);
        yield return new WaitForSeconds(1);
    }

    public IEnumerator Pattern_LaunchRocket()
    {
        sm.PlaySound("DragonRocketReload", 1);
        an.SetTrigger("launchRocket");
        GameObject r = Instantiate(rocketPrefab);
        r.transform.position = rocketLaunchPos.position;
        r.transform.rotation = Quaternion.LookRotation(player.transform.position - r.transform.position);
        DragonRocket dr = r.GetComponent<DragonRocket>();
        dr.Invoke(nameof(dr.Launch), rocketLaunchDelay);
        dr.damage = rocketDamage; 

        yield return new WaitForSeconds(rocketLaunchDelay);
        sm.PlaySound("DragonRocketLaunch", 1);
        yield return new WaitForSeconds(1);
    }
    public IEnumerator KeepSpawnEntities()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnDurationMin, spawnDurationMax));
            if (inBattle)
            {
                int ran = Random.Range(0, 1);
                switch (ran)
                {
                    case 0:
                        {
                            GameObject z = Instantiate(zombie);
                            z.SetActive(false);
                            z.transform.position = randomEntitiesSpawnPos.position;
                            z.SetActive(true);
                            z.GetComponent<AI_Zombie>().Invoke("StartBattle", 3);
                        }
                        break;
                    case 1:
                        {
                            GameObject z = Instantiate(bowZombie);
                            z.transform.position = randomEntitiesSpawnPos.position;
                            z.GetComponent<AI_BowZombie>().Invoke("StartBattle", 3);
                        }
                        break;
                }
            }
        }
    }
}
[System.Serializable]
public class DragonPattern
{
    public string patternName;
    public int density;
}