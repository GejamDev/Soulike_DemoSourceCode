using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Weapon : ScriptableObject
{


    [Header("Characteristics")]
    public float runSpeedWhenHolding;


    [Header("Attack")]
    public int damage;
    public float walkSpeedWhenAttak;
    public float attackTime;
    public float reloadTime;
    public float damageDelay;
    public AttackType type;
    public GameObject attackPrefab;
    public float attackLastTime;


    [Header("Animation")]
    public AnimationClip attackAnimation;

    [Header("Cam Shake")]
    public bool hasCamShake;
    public float camShakeTime;
    public float camShakeIntensity;
    public bool camShakeFade;

    [Header("Hit Cam Shake")]
    public bool hasCamShakeOnHit;
    public float camShakeTimeOnHit;
    public float camShakeIntensityOnHit;
    public bool camShakeFadeOnHit;

    [Header("Sound")]
    public string instantSound;
    public string attackSound;
    public string hitSound;
}

public enum AttackType
{
    Close
}
