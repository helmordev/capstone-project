using UnityEngine;
using UnityEngine.SceneManagement;

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
        quarterNavPanel.SetActive(false);

        // Hide all the right-side quarter panels
        for (int i = 0; i < quarterPanels.Length; i++)
        {
            if (quarterPanels[i] != null)
            {
                quarterPanels[i].SetActive(false);
            }
        }

        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        quarterNavPanel.SetActive(true);

        // Restore the quarter panel they were looking at before opening settings
        ShowQuarter(currentQuarterIndex);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit(); 
    }

    public void LoadLevel(string levelSceneName)
    {
        Debug.Log("Loading level: " + levelSceneName);
        SceneManager.LoadScene(levelSceneName);
    }
}