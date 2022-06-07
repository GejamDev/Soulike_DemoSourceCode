using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public UniversalManager um;
    InventoryManager im;
    DeathManager dm;
    public bool mouseOn;

    void Awake()
    {
        mouseOn = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        im = um.inventoryManager;
        dm = um.deathManager;
    }
    public void Update()
    {
        mouseOn = dm.dead;


        Cursor.lockState = mouseOn ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = mouseOn;
    }
}
