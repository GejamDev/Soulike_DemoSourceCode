using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnDestroy : MonoBehaviour
{
    public string sound;

    void OnDestroy()
    {
        FindObjectOfType<UniversalManager>().soundManager.PlaySound(sound, 1);
    }
}
