using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    [Header("Intro Settings")]
    [Tooltip("Drag your Video Player component here")]
    public VideoPlayer myVideoPlayer;

    [Tooltip("The exact name of your Main Menu scene")]
    public string mainMenuSceneName = "LoadingScene";

    void Start()
    {
        // Check if the Video Player is attached
        if (myVideoPlayer != null)
        {
            // Tell Unity: "When the video reaches the end, run the OnVideoFinished function"
            myVideoPlayer.loopPointReached += OnVideoFinished;
        }
        else
        {
            Debug.LogError("No Video Player attached to IntroController! Loading menu immediately.");
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // This function automatically runs the moment the video stops
    void OnVideoFinished(VideoPlayer vp)
    {
        // Good practice: disconnect the listener when we are done
        myVideoPlayer.loopPointReached -= OnVideoFinished;

        Debug.Log("Video finished! Loading Main Menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}