using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableWeapon : MonoBehaviour
{
    UniversalManager um;
    GameObject player;
    CombatMovement cm;
    public Transform weaponBundle;

    public Weapon weapon;
    public float pickableDistance;

    void Awake()
    {
        um = FindObjectOfType<UniversalManager>();

        player = um.player;
        cm = player.GetComponent<CombatMovement>();
        Invoke(nameof(SetSettings), 0.1f);
    }
    public void SetSettings()
    {

        for (int i = 0; i < weaponBundle.childCount; i++)
        {
            GameObject g = weaponBundle.GetChild(i).gameObject;
            g.SetActive(g.name == weapon.name);
        }
    }

    public void Update()
    {
        if(Vector3.Distance(player.transform.position, transform.position) <= pickableDistance)
        {
            if(Input.GetKeyDown(KeyCode.E) && !cm.attacking)
            {
                cm.ChangeWeaponTo(weapon);
                Destroy(gameObject);
            }
        }
    }
}
