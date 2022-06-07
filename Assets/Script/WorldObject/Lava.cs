using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    UniversalManager um;
    GameObject player;
    PlayerScript ps;
    public ParticleSystem[] particles;
    void Awake()
    {
        um = FindObjectOfType<UniversalManager>();
        player = um.player;

        ps = player.GetComponent<PlayerScript>();


        foreach(ParticleSystem p in particles)
        {
            var shape = p.shape;
            shape.scale = new Vector3(transform.localScale.x, transform.localScale.z, transform.localScale.y);

        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            ps.TakeDamage(9999, 1);
        }
    }
}
