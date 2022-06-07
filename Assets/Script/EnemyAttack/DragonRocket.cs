using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonRocket : MonoBehaviour
{
    public int damage;
    UniversalManager um;
    Rigidbody rb;
    GameObject player;
    public float speed;
    public float rotSpeed;
    public float rotSpeedSpeed;
    bool launched;
    public GameObject exp;
    public Weapon playerWeapon;
    public float camShakeTime;
    public float camShakeIntensity;
    void Awake()
    {
        um = FindObjectOfType<UniversalManager>();

        player = um.player;
        rb = GetComponent<Rigidbody>();
    }
    public void Launch()
    {
        launched = true;
    }
    void Update()
    {
        if (!launched)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        rotSpeed += Time.deltaTime * rotSpeedSpeed;
        Quaternion curRot = transform.rotation;
        Quaternion target = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(curRot, target, rotSpeed * Time.deltaTime);
        rb.velocity = transform.forward.normalized * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject e = Instantiate(exp);
        e.transform.position = transform.position;
        e.GetComponent<EnemyAttack>().damage = damage;
        e.transform.GetChild(0).GetComponent<PlayerAttack>().damage = damage;
        e.transform.GetChild(0).GetComponent<PlayerAttack>().usedWeapon = playerWeapon;
        Destroy(e.GetComponent<EnemyAttack>(), 0.1f);
        Destroy(e.transform.GetChild(0).GetComponent<PlayerAttack>(), 0.1f);


        FindObjectOfType<UniversalManager>().cameraManager.ShakeCamera(camShakeTime, camShakeIntensity, true, 0);

        Destroy(gameObject);
    }

}
