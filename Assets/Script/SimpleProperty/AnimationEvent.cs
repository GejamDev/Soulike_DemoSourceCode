using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public MonoBehaviour behaviour;
    public void Trigger(string name)
    {
        behaviour.Invoke(name, 0);
    }
}
