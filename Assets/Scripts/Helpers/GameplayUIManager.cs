using UnityEngine;

public class GameplayUIManager : MonoBehaviour
{
    [Header("Quarter Panels (Right Side)")]
    [Tooltip("Drag Quarter1_Panel, Quarter2_Panel, etc. here")]
    public GameObject[] quarterPanels;

    [Header("UI Containers")]
    [Tooltip("Drag the 'quarter_panel' (holds the left buttons) here")]
    public GameObject quarterNavPanel;

    [Tooltip("Drag the 'Settings_Panel' here")]
    public GameObject settingsPanel;

    // This remembers which quarter the player was looking at
    private int currentQuarterIndex = 0;

    void Start()
    {
        CloseSettings();
        ShowQuarter(0);
    }

    public void ShowQuarter(int index)
    {
        currentQuarterIndex = index;

        // Loop through all panels and turn ON only the one that matches the index
        for (int i = 0; i < quarterPanels.Length; i++)
        {
            if (quarterPanels[i] != null)
            {
                quarterPanels[i].SetActive(i == index);
            }
        }
    }

    public void OpenSettings()
    {
        quarterNavPanel.SetActive(false); // Hide the left buttons

        // Hide all the right-side quarter panels
        for (int i = 0; i < quarterPanels.Length; i++)
        {
            if (quarterPanels[i] != null)
            {
                quarterPanels[i].SetActive(false);
            }
        }

        settingsPanel.SetActive(true); // Show the settings panel
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);   // Hide settings
        quarterNavPanel.SetActive(true);  // Show left buttons again

        // Restore the quarter panel they were looking at before opening settings
        ShowQuarter(currentQuarterIndex);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit(); 
    }
}