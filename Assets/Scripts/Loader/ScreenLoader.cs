using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking; 

public class ScreenLoader : MonoBehaviour
{
    [Header("Menu Panels (Right Side)")]
    public GameObject defaultPanel; // The NutriMind Logo
    public GameObject optionsPanel; // The Settings/Audio
    public GameObject quitPanel;    // The Yes/No prompt

    [Header("UI References")]
    public GameObject mainMenuUI;   
    public GameObject loadingUI;    
    public Slider loadingBar;
    public float fakeLoadSpeed = 0.5f;

    [Header("Network Routing Settings")]
    public string onlineSceneName = "OnlineScene";
    public string offlineSceneName = "OfflineScene";
    
    public string pingURL = "https://clients3.google.com/generate_204";

    void Start()
    {
        // Automatically show the NutriMind logo when the scene starts
        ShowDefaultPanel();
    }

    public void ShowDefaultPanel()
    {
        defaultPanel.SetActive(true);
        optionsPanel.SetActive(false);
        quitPanel.SetActive(false);
    }

    public void ShowOptionsPanel()
    {
        defaultPanel.SetActive(false);
        optionsPanel.SetActive(true);
        quitPanel.SetActive(false);
    }

    public void ShowQuitPanel()
    {
        defaultPanel.SetActive(false);
        optionsPanel.SetActive(false);
        quitPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Exiting Game...");
        Debug.Log("Note: 'Application.Quit()' will only work in a built version of the game. In the Unity Editor, this will not close the play mode.");
        Application.Quit(); // This closes the game when built
    }

    // --- YOUR EXISTING LOADING LOGIC ---

    public void StartGameLoad()
    {
        StartCoroutine(CheckInternetAndLoadAsync());
    }

    IEnumerator CheckInternetAndLoadAsync()
    {
        mainMenuUI.SetActive(false);
        loadingUI.SetActive(true);
        loadingBar.value = 0f;

        string targetScene = offlineSceneName; 

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            UnityWebRequest request = UnityWebRequest.Get(pingURL);
            request.timeout = 3; 
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                targetScene = onlineSceneName;
                Debug.Log("Internet check SUCCESS. Loading: " + targetScene);
            }
            else
            {
                Debug.Log("Internet check FAILED (Error: " + request.error + "). Loading: " + targetScene);
            }
        }
        else
        {
            Debug.Log("Device Wi-Fi/Data is fully disabled. Loading: " + targetScene);
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        float displayedProgress = 0f;

        while (!operation.isDone)
        {
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            displayedProgress = Mathf.MoveTowards(displayedProgress, realProgress, fakeLoadSpeed * Time.deltaTime);
            loadingBar.value = displayedProgress;

            if (displayedProgress >= 1f)
            {
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}