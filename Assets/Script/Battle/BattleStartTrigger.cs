using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStartTrigger : MonoBehaviour
{
    bool used;
    UniversalManager um;
    CinemaManager cm;
    public Cinema fightCinema;
    public MonoBehaviour[] ais;

    void Awake()
    {
        um = FindObjectOfType<UniversalManager>();
        cm = um.cinemaManager;
        
        used = false;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != um.player)
        {
            return;
        }
        if (used)
            return;
        used = true;
        if (fightCinema == null)
        {
            for(int i =0; i < ais.Length; i++)
            {
                ais[i].Invoke("StartBattle", 0);
            }
        }
        else
        {
            cm.StartCinema(fightCinema);
        }
    }
}
