using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExplanationManager : MonoBehaviour
{
    public Text expText;
    public List<Door> usableDoorList = new List<Door>();
    public List<GameObject> interactableList = new List<GameObject>();

    void Update()
    {
        if (usableDoorList.Count != 0)
        {
            expText.text = "Press F to open door";
        }
        else if (interactableList.Count != 0)
        {
            expText.text = "Press F to interact";
        }
        else
        {
            expText.text = "";
        }
    }
}
