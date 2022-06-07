using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathManager : MonoBehaviour
{
    public UniversalManager um;
    CameraManager cm;
    GameObject player;
    public GameObject ragdollPrefab;
    public float deathCamShakeTime;
    public float deathCamShakeIntensity;
    public GameObject deathUI;
    public bool dead;

    void Awake()
    {
        player = um.player;

        cm = um.cameraManager;

        deathUI.SetActive(false);
    }

    public void PlayerDie()
    {
        dead = true;
        Destroy(player.GetComponent<CombatMovement>());
        player.GetComponent<PlayerScript>().dead = true;
        player.GetComponent<PlayerScript>().PlayDeathAnimation();
        deathUI.SetActive(true);
    }
}
