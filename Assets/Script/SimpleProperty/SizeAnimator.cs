using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeAnimator : MonoBehaviour
{
    public Vector3 originScale;
    public AnimationCurve size;
    public bool loop;
    public float duration;
    float time;
    

    void Update()
    {
        time += Time.deltaTime;
        if (time >= duration)
        {
            if (loop)
            {
                time = time % duration;
            }
            else
            {
                time = duration;
            }
        }
        transform.localScale = originScale * size.Evaluate(time);
    }
}
