using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CinemaManager : MonoBehaviour
{
    public bool debug;
    [Header("Variables")]
    public UniversalManager univM;
    DialogueManager dm;
    InventoryManager im;
    CameraManager cm;
    UIManager um;
    SoundManager sm;

    public AutoSpawnPoint[] autoSpawnPoints;

    [Header("Cinema")]
    public Cinema awakeCinema;

    [Header("Status")]
    public bool playingCinema;


    [Header("Custom")]
    GameObject player;
    PlayerScript ps;
    PlayerMovement pm;
    CombatMovement combatM;
    Animator playerAn;
    public Image blackFilter;
    public GameObject doorKeyA;
    public AI_Rocky rocky;
    public Door rockyStartDoor;
    public GameObject shieldAObj;
    public Shield shieldA;
    public Door rockyWallDoor;
    public AI_Dragon dragon;
    public GameObject dragonLava;
    public GameObject demoEndUI;


    void Awake()
    {

        dm = univM.dialogueManager;
        im = univM.inventoryManager;
        cm = univM.cameraManager;
        um = univM.uiManager;
        sm = univM.soundManager;

        player = univM.player;
        ps = player.GetComponent<PlayerScript>();
        pm = player.GetComponent<PlayerMovement>();
        combatM = player.GetComponent<CombatMovement>();
        playerAn = pm.an;


        demoEndUI.SetActive(false);

        if (awakeCinema != null && !debug)
        {
            StartCinema(awakeCinema);
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R) && Input.GetKeyDown(KeyCode.LeftControl))
        {
            PlayerPrefs.SetInt("PlayedCount", 0);
        }
    }



    public void StartCinema(Cinema cinema)
    {
        if (cinema == null)
            return;
        StartCoroutine(CinemaCor(cinema));
    }

    public IEnumerator CinemaCor(Cinema cinema)
    {
        if (playingCinema)
        {
            Debug.LogError("can't play two cinema at once");
        }

        playingCinema = true;
        for(int i =0; i < cinema.lines.Count; i++)
        {
            CinemaLine line = cinema.lines[i];
            switch (line.type)
            {
                case CinemaLineType.Dialogue:
                    yield return StartCoroutine(dm.PrintDialogue(line.dialogue));
                    break;
                case CinemaLineType.Custom:
                    yield return StartCoroutine(line.customAction);
                    break;
                case CinemaLineType.ReceiveItem:
                    im.GetItem(line.item);
                    break;
                default:
                    Debug.LogError("Unknown cinema line type : " + line.type.ToString());
                    break;
            }
        }
        playingCinema = false;


        yield return null;
    }



    
    //custom


    public IEnumerator DungeonStartTransition()
    {
        blackFilter.color = new Color(0, 0, 0, 1);
        player.transform.position = new Vector3(0, -2, 0);
        player.transform.rotation = Quaternion.identity;
        pm.curCameraX = 15;
        pm.cam.transform.eulerAngles = new Vector3(15, 0, 0);
        yield return new WaitForSeconds(0.1f);
        Invoke(nameof(PlayDungeonStartSound), 0.5f);
        if (!PlayerPrefs.HasKey("PlayedCount"))
        {
            playerAn.SetTrigger("standUp");
            for (int i = 1; i <= 100; i++)
            {
                blackFilter.color = new Color(0, 0, 0, 1 - ((float)i / 100));
                pm.cameraDistance = 2 + i * 0.03f;
                yield return new WaitForSeconds(0.05f);
            }
            PlayerPrefs.SetInt("PlayedCount", 1);
        }
        else if(PlayerPrefs.GetInt("PlayedCount") <= 1)
        {
            playerAn.SetTrigger("standUp");
            PlayerPrefs.SetInt("PlayedCount", PlayerPrefs.GetInt("PlayedCount") + 1);
            for (int i = 1; i <= 100; i++)
            {
                blackFilter.color = new Color(0, 0, 0, 1 - ((float)i / 100));
                pm.cameraDistance = 2 + i * 0.03f;
                yield return new WaitForSeconds(0.05f);
            }
        }
        else
        {
            playerAn.SetTrigger("standUp");
            PlayerPrefs.SetInt("PlayedCount", PlayerPrefs.GetInt("PlayedCount") + 1);
        }
        for(int i =0;i<autoSpawnPoints.Length; i++)
        {
            if (PlayerPrefs.GetInt("PlayedCount") >= autoSpawnPoints[i].deathCount)
            {
                player.transform.position = autoSpawnPoints[i].pos;
                break;
            }
        }
        blackFilter.color = new Color(0, 0, 0, 0);
        pm.cameraDistance = 5;
    }
    public void PlayDungeonStartSound()
    {
        sm.PlaySound("DungeonStart", 1);
    }

    public IEnumerator DungeonStartPlay()
    {
        playerAn.SetTrigger("startPlay");
        yield return null;
    }

    public IEnumerator RemoveDoorKeyA()
    {
        doorKeyA.SetActive(false);
        yield return null;
    }
    public IEnumerator DungeonRockyStartBattle()
    {
        um.ShowBossName("Rocky");
        rocky.StartBattle();
        rockyStartDoor.open = false;
        rockyStartDoor.moveSpeed *= 10000;
        cm.ShakeCamera(0.1f, 15, true, 0);
        cm.ShakeCamera(2, 1, true, 0);
        sm.PlaySound("RockyDoorShut", 1);

        yield return null;
    }
    public IEnumerator RockyDefeat()
    {
        rockyStartDoor.open = transform;
        rockyStartDoor.moveSpeed /= 10000;
        cm.ShakeCamera(5, 1, true, 0);
        rockyWallDoor.Open();

        yield return null;
    }
    public IEnumerator GetShieldA()
    {
        combatM.ChangeShieldTo(shieldA);
        shieldAObj.SetActive(false);
        yield return null;
    }
    public IEnumerator DungeonDragonStartBattle()
    {
        sm.ChangeMusic("DragonCastle");
        um.ShowBossName("Dragon");
        dragon.StartBattle();
        cm.ShakeCamera(0.1f, 15, true, 0);
        cm.ShakeCamera(2, 1, true, 0);
        sm.PlaySound("RockyDoorShut", 1);

        dragonLava.transform.position += Vector3.up * (-11 - dragonLava.transform.localPosition.y);
        yield return null;
    }
    public IEnumerator DragonDefeat()
    {
        sm.StopMusic();
        cm.ShakeCamera(3, 1, true, 0);
        dragonLava.transform.position += Vector3.up * (-11 - dragonLava.transform.localPosition.y);
        StartCoroutine(RemoveDragonLava());
        yield return null;
    }
    public IEnumerator RemoveDragonLava()
    {
        for (int i = 0; i < 550; i++)
        {
            dragonLava.transform.eulerAngles += new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            dragonLava.transform.position += Vector3.down * 0.02f;
            yield return null;
        }
    }
    public IEnumerator DungeonEnd()
    {
        sm.PlaySound("RockyDoorShut", 1);
        Time.timeScale = 0;
        demoEndUI.SetActive(true);
        yield return null;
    }
}

[System.Serializable]
public class AutoSpawnPoint
{
    public Vector3 pos;
    public int deathCount;
}


