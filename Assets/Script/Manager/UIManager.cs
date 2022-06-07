using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Transform effectUI;
    public Text bossNamePrefab;


    public void ShowBossName(string bossName)
    {
        Text t = Instantiate(bossNamePrefab.gameObject).GetComponent<Text>();

        t.text = bossName;

        t.transform.SetParent(effectUI);
        Destroy(t, 10);
    }
}
