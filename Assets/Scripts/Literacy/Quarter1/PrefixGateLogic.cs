using UnityEngine;
using TMPro;
using System.Collections;

public class PrefixGateLogic : MonoBehaviour
{
    public TMP_Text feedbackText;
    public HeroMover heroScript;
    public GameObject gateObject;

    public GameObject levelCompleteUI;

    void Start()
    {
        feedbackText.gameObject.SetActive(false);
    }

    public void SelectCorrectPrefix()
    {
        feedbackText.text = "UN + HAPPY = UNHAPPY! Gate Opening...";
        feedbackText.color = Color.green;
        feedbackText.gameObject.SetActive(true);
        StartCoroutine(GateOpenSequence());
    }

    public void SelectWrongPrefix()
    {
        feedbackText.text = "REHAPPY? That's not a word!";
        feedbackText.color = Color.red;
        feedbackText.gameObject.SetActive(true);
    }

    IEnumerator GateOpenSequence()
    {
        yield return new WaitForSeconds(2f);

        // Hide the gate and the UI
        gateObject.SetActive(false);
        this.gameObject.SetActive(false);

        levelCompleteUI.SetActive(true);

        Debug.Log("Week 2 Complete! Level 1 finished.");
    }
}