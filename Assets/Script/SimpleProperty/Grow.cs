using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grow : MonoBehaviour
{
    public Vector3 speed;

    void Update()
    {
        transform.localScale += speed * Time.deltaTime;
    }
}
