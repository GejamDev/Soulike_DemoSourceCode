using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemaTrigger : MonoBehaviour
{
    UniversalManager um;
    CinemaManager cm;
    ExplanationManager em;
    GameObject player;
    PlayerMovement pm;
    public int interactedCount;
    public float reachDistance;
    public Cinema[] cinemaArray;
    public bool reachable;

    void Awake()
    {
        um = FindObjectOfType<UniversalManager>();
        player = um.player;
        pm = player.GetComponent<PlayerMovement>();
        cm = um.cinemaManager;
        em = um.explanationManager;


        interactedCount = 0;
    }

    void Update()
    {

        bool preReachableState = reachable;
        reachable = Vector3.Distance(player.transform.position, transform.position) < reachDistance;

        if (reachable && !em.interactableList.Contains(gameObject))
        {
            em.interactableList.Add(gameObject);
        }
        else if (!reachable && em.interactableList.Contains(gameObject))
        {
            em.interactableList.Remove(gameObject);
        }
        if (pm.paused)
            return;

        if (reachable && Input.GetKeyDown(KeyCode.F))
        {
            cm.StartCinema(interactedCount >= cinemaArray.Length ? cinemaArray[cinemaArray.Length - 1] : cinemaArray[interactedCount]);
            interactedCount++;
        }


    }
}
