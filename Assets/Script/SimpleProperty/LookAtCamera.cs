using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform cam;

    void Awake()
    {
        cam = Camera.main.transform;
    }
    void Update()
    {
        transform.LookAt(cam.position);
    }
}
