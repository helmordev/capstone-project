using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking; 

public class ScreenLoader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuUI;   
    public GameObject loadingUI;    
    public Slider loadingBar;
    public float fakeLoadSpeed = 0.5f;

    [Header("Network Routing Settings")]
    public string onlineSceneName = "OnlineScene";
    public string offlineSceneName = "OfflineScene";
    
    public string pingURL = "https://clients3.google.com/generate_204";

    public void StartGameLoad()
    {
        StartCoroutine(CheckInternetAndLoadAsync());
    }

    IEnumerator CheckInternetAndLoadAsync()
    {
        // 1. Hide the Main Menu, Show the Loading Screen, reset the bar
        mainMenuUI.SetActive(false);
        loadingUI.SetActive(true);
        loadingBar.value = 0f;

        // 2. Determine which scene to load by checking the internet
        string targetScene = offlineSceneName; 

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            UnityWebRequest request = UnityWebRequest.Get(pingURL);
            
            // ADDED: Force Unity to stop trying after 3 seconds
            request.timeout = 3; 
            
            yield return request.SendWebRequest();

            // CHANGED: We now only check if the result was a 100% clean success
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

        // 3. Load the target scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;

        float displayedProgress = 0f;

        // 4. Smooth loading bar logic
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