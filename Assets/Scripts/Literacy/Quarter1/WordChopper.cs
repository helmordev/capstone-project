using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // NEEDED for the timer

public class WordChopper : MonoBehaviour
{
    [Header("Word Buttons")]
    public Button prefixButton;
    public Button rootButton;
    public Button suffixButton;

    [Header("Feedback")]
    public TMP_Text feedbackText;

    [Header("Connections")]
    public HeroMover heroScript; // Drag your Hero here in the Inspector!

    private bool prefixChopped = false;
    private bool suffixChopped = false;

    void Start()
    {
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    public void ChopPrefix()
    {
        prefixButton.gameObject.SetActive(false);
        prefixChopped = true;
        CheckWinCondition();
    }

    public void ChopSuffix()
    {
        suffixButton.gameObject.SetActive(false);
        suffixChopped = true;
        CheckWinCondition();
    }

    public void ClickedRoot()
    {
        feedbackText.text = "Oops! Only chop the extra parts!";
        feedbackText.color = Color.red;
        feedbackText.gameObject.SetActive(true);
    }

    void CheckWinCondition()
    {
        if (prefixChopped && suffixChopped)
        {
            feedbackText.text = "Correct! HAPPY is the Root Word!";
            feedbackText.color = Color.green;
            feedbackText.gameObject.SetActive(true);
            rootButton.GetComponent<Image>().color = Color.yellow;

            // Start the 2-second timer to close the minigame
            StartCoroutine(WinSequence());
        }
    }

    IEnumerator WinSequence()
    {
        Debug.Log("Timer started... waiting 2 seconds.");
        yield return new WaitForSeconds(2f);
        Debug.Log("Timer finished!");

        if (heroScript != null)
        {
            Debug.Log("Hero script found! Telling Hero to move.");
            heroScript.ResumeWalking();
        }
        else
        {
            Debug.LogError("CRITICAL: The Hero Script is missing in the Inspector! Drag the player into the Word Chopper script.");
        }
    }
}