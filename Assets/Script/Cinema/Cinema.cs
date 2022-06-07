using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Cinema : ScriptableObject
{
    public List<CinemaLine> lines = new List<CinemaLine>();
}

[System.Serializable]
public class CinemaLine
{
    public CinemaLineType type;
    public CinemaDialogue dialogue;
    public string customAction;
    public InventorySlot item;
}
[System.Serializable]
public class CinemaDialogue
{
    public CinemaCharacter character;
    public string line;
    public float talkSpeed;
    public bool skippable = true;
}

public enum CinemaLineType
{
    Dialogue,
    Custom,
    ReceiveItem
}
