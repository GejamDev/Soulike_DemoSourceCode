using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossBattleManager : MonoBehaviour
{
    public GameObject currentBoss;
    public Text bossNameTxt;
    public GameObject bossBattleUI;
    public Image bossHpBar;
    public int maxHp;
    public int hp;
    public float synchronizeSpeed;
    public string bossName;
    void Update()
    {
        bossBattleUI.SetActive(currentBoss != null);
        bossNameTxt.text = bossName;
        if (currentBoss == null)
        {
            hp = 0;
            return;
        }
        bossNameTxt.text = bossName;
        bossHpBar.fillAmount = Mathf.Lerp(bossHpBar.fillAmount, (float)hp/maxHp,Time.deltaTime*synchronizeSpeed);
    }

}
