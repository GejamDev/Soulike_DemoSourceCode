using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public UniversalManager um;
    SoundManager sm;
    public GameObject dialogueUI;
    public Image charIcon;
    public Text dialogueText;
    public bool skipping;

    void Awake()
    {
        sm = um.soundManager;




        dialogueUI.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            skipping = true;
        }
    }
    public IEnumerator PrintDialogue(CinemaDialogue dialogue)
    {
        skipping = false;
        dialogueUI.SetActive(true);
        if(dialogue.character.icon == null)
        {
            charIcon.color = new Color(1, 1, 1, 0);
        }
        else
        {
            charIcon.color = Color.white;
            charIcon.sprite = dialogue.character.icon;
        }
        string currentText = "";
        for (int i =0; i < dialogue.line.Length; i++)
        {
            string adding = dialogue.line[i].ToString();
            if (adding == "%")
            {
                adding = "\r\n";
            }
            currentText = currentText + adding;
            dialogueText.text = currentText;
            if(!skipping || !dialogue.skippable)
            {
                sm.PlaySound(dialogue.character.audioName, 1);
                yield return new WaitForSeconds(dialogue.talkSpeed);
            }
        }
        if (skipping && dialogue.skippable)
        {
            yield return new WaitUntil(() => !Input.GetKey(KeyCode.Space));
        }
        skipping = false;


        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        dialogueUI.SetActive(false);
    }
}
