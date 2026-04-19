using UnityEngine;
using UnityEngine.InputSystem;

public class HeroMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float depthSpeed = 3f;
    public float minZ = -2f;
    public float maxZ = 5f;

    [Header("Level 1 Connections")]
    public Transform stopPoint;
    public GameObject wordMinigameUI;

    [Header("Week 2 Connections")]
    public Transform gateStopPoint;
    public GameObject prefixMinigameUI;
    private bool week2Triggered = false;

    private bool minigameActive = false;
    private bool week1Complete = false;

    void Start()
    {
        // Hide the Week 1 UI at the start
        if (wordMinigameUI != null)
            wordMinigameUI.SetActive(false);

        // NEW: Hide the Week 2 UI at the start
        if (prefixMinigameUI != null)
            prefixMinigameUI.SetActive(false);
    }

    void Update()
    {
        if (minigameActive || Keyboard.current == null) return;

        // Movement Logic
        float horizontalInput = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;

        float depthInput = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) depthInput = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) depthInput = -1f;

        Vector3 movement = new Vector3(horizontalInput * speed, 0, depthInput * depthSpeed) * Time.deltaTime;
        transform.Translate(movement);

        float clampedZ = Mathf.Clamp(transform.position.z, minZ, maxZ);
        transform.position = new Vector3(transform.position.x, transform.position.y, clampedZ);

        if (horizontalInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // TRIGGER LOGIC - Only runs if Week 1 isn't done yet!
        if (!week1Complete && stopPoint != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - stopPoint.position.x);
            if (distanceX < 1.5f) // Tightened the distance slightly
            {
                StartMinigame();
            }
        }

        if (week1Complete && !week2Triggered && gateStopPoint != null)
        {
            float distanceX = Mathf.Abs(transform.position.x - gateStopPoint.position.x);
            if (distanceX < 2f)
            {
                week2Triggered = true;
                minigameActive = true;
                prefixMinigameUI.SetActive(true);
            }
        }
    }

    void StartMinigame()
    {
        minigameActive = true;
        wordMinigameUI.SetActive(true);
    }

    public void ResumeWalking()
    {
        Debug.Log("RESUMING WALKING...");

        week1Complete = true;  
        minigameActive = false; 

        if (wordMinigameUI != null) wordMinigameUI.SetActive(false);

        // Hide the tree so it's gone
        if (stopPoint != null) stopPoint.gameObject.SetActive(false);

        // Push player slightly forward just to be 100% sure
        transform.position = new Vector3(transform.position.x + 2f, transform.position.y, transform.position.z);
    }

    public void ResumeWalkingFromGate()
    {
        minigameActive = false;
        // Teleport slightly past gate
        transform.position = new Vector3(transform.position.x + 3f, transform.position.y, transform.position.z);
    }
}