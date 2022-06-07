using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityManager : MonoBehaviour
{
    void Awake()
    {
        QualitySettings.SetQualityLevel(5);
    }
}
