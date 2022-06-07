using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public UniversalManager um;
    GameObject player;
    PlayerScript ps;

    void Awake()
    {
        player = um.player;
        ps = player.GetComponent<PlayerScript>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && ps.dead)
        {
            Restart();
        }
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
