using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    UniversalManager um;
    public MonoBehaviour behaviour;
    public string behaviourName;

    void Awake()
    {
        um = FindObjectOfType<UniversalManager>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == um.player)
        {
            behaviour.Invoke(behaviourName, 0);
        }
    }
    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject == um.player)
        {
            behaviour.Invoke(behaviourName, 0);
        }
    }
}
