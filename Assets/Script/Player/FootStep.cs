using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootStep : MonoBehaviour
{
    public UniversalManager um;
    PlayerMovement pm;
     SoundManager sm;

    void Awake()
    {
        pm = um.player.GetComponent<PlayerMovement>();
        sm = um.soundManager;
    }
    public void PlayFootStepSound()
    {
        if (pm.stickToLadder || pm.grounded)
        {
            sm.PlaySound("Walk", 1);
        }
    }
}
