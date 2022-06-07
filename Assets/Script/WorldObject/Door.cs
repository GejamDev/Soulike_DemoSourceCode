using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    UniversalManager um;
    GameObject player;
    InventoryManager im;
    ExplanationManager em;
    PlayerMovement pm;
    SoundManager sm;
    CameraManager cm;
    public float reachDistance;
    public bool open;

    public Vector3 closePos;
    public Vector3 closeRot;
    public Vector3 openPos;
    public Vector3 openRot;
    public float moveSpeed;
    public Item key;
    public bool reachable;
    public string openSound;
    public string lockedSound;
    public float camShakeTime;
    public float camShakeIntensity;


    public void Awake()
    {
        um = FindObjectOfType<UniversalManager>();
        im = um.inventoryManager;
        em = um.explanationManager;
        sm = um.soundManager;
        cm = um.cameraManager;

        player = um.player;
        pm = player.GetComponent<PlayerMovement>();

        closePos = transform.position;
        closeRot = transform.eulerAngles;
    }

    public void Open()
    {
        if(openSound.Length>0)
            sm.PlaySound(openSound, 1);
        cm.ShakeCamera(camShakeTime, camShakeIntensity, true, 0);
        open = true;
    }
    void Update()
    {
        bool preReachableState = reachable;
        reachable = Vector3.Distance(player.transform.position, transform.position) < reachDistance;

        if (reachable && !em.usableDoorList.Contains(this))
        {
            em.usableDoorList.Add(this);
        }
        else if(!reachable && em.usableDoorList.Contains(this))
        {
            em.usableDoorList.Remove(this);
        }

        if (pm.paused)
            return;
        if (reachable && Input.GetKeyDown(KeyCode.F)&&!open)
        {
            if (key == null)
            {
                Open();
            }
            else if(im.HasItem(key))
            {
                im.UseItem(key);
                Open();
            }
            else if(lockedSound.Length>0)
            {
                sm.PlaySound(lockedSound, 1);
            }
        }




        transform.position = Vector3.Lerp(transform.position, open ? openPos : closePos, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, open ? Quaternion.Euler(openRot) : Quaternion.Euler(closeRot), moveSpeed * Time.deltaTime);
    }

}
