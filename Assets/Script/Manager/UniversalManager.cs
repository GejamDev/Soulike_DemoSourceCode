using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalManager : MonoBehaviour
{
    [Header("Managers")]
    public CameraManager cameraManager;
    public CursorManager cursorManager;
    public InventoryManager inventoryManager;
    public DeathManager deathManager;
    public CinemaManager cinemaManager;
    public DialogueManager dialogueManager;
    public ExplanationManager explanationManager;
    public UIManager uiManager;
    public SoundManager soundManager;
    public BossBattleManager bossBattleManager;
    
    [Header("Objects")]
    public GameObject player;

}
