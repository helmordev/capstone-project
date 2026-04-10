using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class ScreenLoader : MonoBehaviour
{
    public GameObject LoadingScreen;
    public Slider loadingBar;

    public float fakeLoadSpeed = 0.5f;

    public void LoadScene(int levelIndex)
    {
        StartCoroutine(LoadSceneAsynchronously(levelIndex));
    }

    IEnumerator LoadSceneAsynchronously(int levelIndex)
    {
        LoadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(levelIndex);
        operation.allowSceneActivation = false;

        float displayedProgress = 0f;

        while (!operation.isDone)
        {
            // actual progress (0–1)
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // smooth / slow progress
            displayedProgress = Mathf.MoveTowards(displayedProgress, realProgress, fakeLoadSpeed * Time.deltaTime);

            loadingBar.value = displayedProgress;

            // kapag full na yung bar saka lang mag load
            if (displayedProgress >= 1f)
            {
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}