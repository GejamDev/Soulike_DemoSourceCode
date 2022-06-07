using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Item : ScriptableObject
{
    public Sprite sprite;
    public string customBehaviourName;
    public bool instantUse;
    public float useTime;
    public float walkSpeedWhenUsing;
    public float rotSpeedWhenUsing;
    public bool reusable;
    
    public string info;
}
