using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public UniversalManager um;
    GameObject player;
    PlayerScript ps;
    PlayerMovement pm;
    CombatMovement cm;
    Animator playerAn;

    public const int maxInventoryCellSpace = 4;

    public int currentSlot;

    public GameObject selectedIcon;
    public GameObject itemUsageUI;
    public Image itemUsageBar;
    public GameObject itemInfoPrefab;

    public InventorySlot[] savedSlots;

    public InventorySlot[] inventorySlots;
    public InventoryCell[] inventoryCells;
    public Dictionary<InventoryCell, int> cellIndexDictionary = new Dictionary<InventoryCell, int>();
    public Dictionary<InventorySlot, InventoryCell> cellDictionary = new Dictionary<InventorySlot, InventoryCell>();
    public bool usingItem;
    public Item currentlyUsingItem;
    float itemUsedTime;
    
    

    public void Awake()
    {
        player = um.player;

        ps = player.GetComponent<PlayerScript>();
        pm = player.GetComponent<PlayerMovement>();
        cm = player.GetComponent<CombatMovement>();
        playerAn = ps.an;
        
        inventorySlots = new InventorySlot[maxInventoryCellSpace];
        for (int i =0; i < maxInventoryCellSpace; i++)
        {
            inventorySlots[i] = savedSlots[i];
            cellIndexDictionary.Add(inventoryCells[i], i);
            cellDictionary.Add(inventorySlots[i], inventoryCells[i]);
        }
        foreach (InventoryCell ic in inventoryCells)
        {
            UpdateCell(ic);
        }
        ChangeSelectingSlot();
    }

    public void GetItem(InventorySlot slot)
    {
        for(int i =0; i < maxInventoryCellSpace; i++)
        {
            if (inventorySlots[i].item == slot.item && inventorySlots[i].count > 0)
            {
                inventorySlots[i].count += slot.count;
                UpdateCell(inventoryCells[i]);
                return;
            }
        }
        for (int i = 0; i < maxInventoryCellSpace; i++)
        {
            if (inventorySlots[i].count <= 0)
            {
                inventorySlots[i].item = slot.item;
                inventorySlots[i].count = slot.count;
                UpdateCell(inventoryCells[i]);
                return;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentSlot = 0;
            ChangeSelectingSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentSlot = 1;
            ChangeSelectingSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentSlot = 2;
            ChangeSelectingSlot();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentSlot = 3;
            ChangeSelectingSlot();
        }


        if (Input.GetKeyDown(KeyCode.E) && !usingItem && pm.grounded && !cm.attacking && !pm.rolling && !pm.stickToLadder && !cm.blocking)
        {
            UseItem();
        }
        UpdateUsageBar();
        

    }
    public void ChangeSelectingSlot()
    {
        if(inventorySlots[currentSlot].count > 0)
        {
            GameObject i = Instantiate(itemInfoPrefab);
            i.transform.SetParent(inventoryCells[currentSlot].transform);
            i.transform.localPosition = new Vector2(0, 0);
            i.transform.localScale = Vector2.one;
            i.GetComponent<Text>().text = inventorySlots[currentSlot].item.info;

        }
        selectedIcon.transform.localPosition = new Vector2(0, 75 - 50 * currentSlot);
    }
    public void UpdateUsageBar()
    {
        if (usingItem)
        {
            itemUsageUI.SetActive(true);
            itemUsedTime += Time.deltaTime;
            itemUsageBar.fillAmount = (float)itemUsedTime / currentlyUsingItem.useTime;
        }
        else
        {
            itemUsageUI.SetActive(false);
            itemUsedTime = 0;
        }
    }
    public void UseItem()
    {

        if (inventorySlots[currentSlot].count >= 1)
        {
            Item item = inventorySlots[currentSlot].item;
            if (item.customBehaviourName.Length == 0)
                return;
            
            if (!item.reusable)
            {
                inventorySlots[currentSlot].count--;
                UpdateCell(cellDictionary[inventorySlots[currentSlot]]);
            }
            usingItem = true;
            itemUsedTime = 0;
            currentlyUsingItem = item;
            Invoke(nameof(EndUsingItem), item.useTime);
            if (item.instantUse)
            {
                Invoke(item.customBehaviourName, 0);
            }
        }
    }
    public void EndUsingItem()
    {
        string action = currentlyUsingItem.customBehaviourName;
        if (action.Length != 0 && !currentlyUsingItem.instantUse)
        {
            Invoke(action, 0);
        }
        usingItem = false;
    }
    public void UpdateCell(InventoryCell cell)
    {
        InventorySlot slot = inventorySlots[cellIndexDictionary[cell]];
        cell.count.text = slot.count > 1 ? slot.count.ToString() : "";
        if(slot.count > 0)
        {
            cell.icon.sprite = slot.item.sprite;
            cell.icon.color = Color.white;
        }
        else
        {
            cell.icon.color = new Color(1, 1, 1, 0);
        }
    }
    

    //custom item actions


    void UseHpPotion()
    {
        ps.hp += 20;
    }
    void UseStaminaPotion()
    {
        ps.stamina = ps.maxStamina;
    }
    void UseNormalApple()
    {
        ps.stamina = ps.maxStamina;
        ps.hp = ps.maxHp;


        pm.runSpeed *= 4;
        pm.jumpForce *= 2;
        pm.rollSpeed *= 4;
        pm.climbSpeed *= 5;
        ps.additionalDamage *= 5;

        Invoke(nameof(EndNormalAppleEffect), 5);
    }
    void EndNormalAppleEffect()
    {
        float posion = 0.8f;
        pm.runSpeed *= 0.25f * posion;
        pm.jumpForce *= 0.5f * posion;
        pm.rollSpeed *= 0.25f * posion;
        pm.climbSpeed *= 0.2f * posion;
        ps.additionalDamage *= 0.2f * posion;
    }
    public bool HasItem(Item item)
    {
        foreach(InventorySlot slot in inventorySlots)
        {
            if (slot.item == item && slot.count >= 1)
            {
                return true;
            }
        }
        return false;
    }
    public void UseItem(Item item)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.item == item && slot.count >= 1)
            {
                slot.count--;
                UpdateCell(cellDictionary[inventorySlots[currentSlot]]);
                return;
            }
        }
        Debug.LogError("doesn't have item : " + item.name);
    }
}

[System.Serializable]
public class InventorySlot
{
    public Item item;
    public int count;
}
