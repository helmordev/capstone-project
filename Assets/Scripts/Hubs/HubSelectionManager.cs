using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HubSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField codeInput; // The student types the teacher's code here
    public Button joinButton;        // The button they click to submit
    public TMP_Text feedbackText;

    [Header("Network Settings")]
    public string apiBaseUrl = "http://YOUR_SERVER_URL";
    public string nextSceneName = "GameplayScene"; 

    void Start()
    {
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    public void OnJoinButtonClicked()
    {
        string enteredCode = codeInput.text.Trim(); // .Trim() removes accidental spaces

        if (string.IsNullOrEmpty(enteredCode))
        {
            ShowFeedback("Please enter the server code.", Color.red, true);
            return;
        }

        joinButton.interactable = false;
        ShowFeedback("Verifying code...", Color.yellow, false);

        StartCoroutine(JoinHubRoutine(enteredCode));
    }

    IEnumerator JoinHubRoutine(string code)
    {
        string accessToken = PlayerPrefs.GetString("AccessToken", "");
        
        if (string.IsNullOrEmpty(accessToken))
        {
            ShowFeedback("Error: You are not logged in. Please restart.", Color.red, false);
            yield break;
        }

        // Prepare the JSON request with the typed code
        HubJoinRequest requestData = new HubJoinRequest { code = code };
        string jsonData = JsonUtility.ToJson(requestData);

        string joinEndpoint = apiBaseUrl + "/api/v1/hubs/join";
        UnityWebRequest request = new UnityWebRequest(joinEndpoint, "POST");
        
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken); //

        yield return request.SendWebRequest();

        joinButton.interactable = true;

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            ShowFeedback("Network error. Could not reach server.", Color.red, true);
        }
        else
        {
            HubJoinResponse response = JsonUtility.FromJson<HubJoinResponse>(request.downloadHandler.text);

            if (response != null && response.success)
            {
                ShowFeedback("Successfully joined Hub!", Color.green, false);
                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene(nextSceneName);
            }
            else if (response != null && response.error != null)
            {
                // Handle specific API errors like invalid codes
                if (response.error.code == "CODE_INVALID_OR_EXPIRED" || response.error.code == "HUB_NOT_FOUND")
                {
                    ShowFeedback("Invalid or expired code. Please try again.", Color.red, true);
                }
                else if (response.error.code == "HUB_ALREADY_MEMBER" || response.error.code == "HUB_STUDENT_ALREADY_IN_HUB")
                {
                    ShowFeedback("You are already in a hub! Loading game...", Color.green, false);
                    yield return new WaitForSeconds(1f);
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    ShowFeedback(response.error.message, Color.red, true);
                }
            }
            else
            {
                ShowFeedback("An unknown error occurred.", Color.red, true);
            }
        }
    }

    private void ShowFeedback(string message, Color color, bool autoHide)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);

        if (autoHide)
        {
            StopAllCoroutines(); 
            StartCoroutine(HideFeedbackAfterDelay(3f));
        }
    }

    IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.gameObject.SetActive(false);
    }
}

// --- JSON Data Structures ---
[Serializable]
public class HubJoinRequest { public string code; }

[Serializable]
public class HubJoinResponse
{
    public bool success;
    public HubJoinData data;
    public AuthError error; 
}

[Serializable]
public class HubJoinData
{
    public string id;
    public string hubId;
    public string studentId;
    public string joinedAt;
}