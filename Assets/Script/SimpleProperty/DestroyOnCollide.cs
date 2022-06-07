using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnCollide : MonoBehaviour
{
    public float delay;
    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject, delay);
    }
}
