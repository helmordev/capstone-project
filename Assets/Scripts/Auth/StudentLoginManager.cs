using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class StudentLoginManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField studentIdInput;
    public TMP_InputField pinInput;
    public Button loginButton;
    public TMP_Text feedbackText;

    public string hubSceneName = "HubSelectionScene";

    [Header("API Settings")]
    public string apiBaseUrl = "http://localhost:8000";

    private Coroutine feedbackHideCoroutine;

    void Start()
    {
        // 1. Hide the feedback text as soon as the game starts
        feedbackText.gameObject.SetActive(false);
    }

    public void OnLoginButtonClicked()
    {
        string lrn = studentIdInput.text.Trim();
        string pin = pinInput.text.Trim();

        // 2. Show the text when they click, and make it yellow for "processing"
        feedbackText.gameObject.SetActive(true);

        if (string.IsNullOrEmpty(lrn) || string.IsNullOrEmpty(pin))
        {
            ShowFeedback("Please enter both your LRN and PIN.", Color.red, true);
            return;
        }

        if (lrn.Length != 12)
        {
            ShowFeedback("LRN must be exactly 12 characters.", Color.red, true);
            return;
        }

        if (pin.Length != 6)
        {
            ShowFeedback("PIN must be exactly 6 characters.", Color.red, true);
            return;
        }

        loginButton.interactable = false;

        // Show yellow text while loading
        ShowFeedback("Connecting to server...", Color.yellow, false);

        StartCoroutine(LoginRoutine(lrn, pin));
    }

    IEnumerator LoginRoutine(string lrn, string pin)
    {
        StudentLoginRequest requestData = new StudentLoginRequest { lrn = lrn, pin = pin };
        string jsonData = JsonUtility.ToJson(requestData);

        string loginEndpoint = apiBaseUrl + "/api/v1/auth/login";
        UnityWebRequest request = new UnityWebRequest(loginEndpoint, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");

        yield return request.SendWebRequest();

        loginButton.interactable = true;

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
        {
            ShowFeedback("Error: Could not connect to the server.", Color.red, true);
            yield break;
        }

        if (request.responseCode >= 200 && request.responseCode < 300)
        {
            StudentLoginSuccessResponse response = JsonUtility.FromJson<StudentLoginSuccessResponse>(request.downloadHandler.text);

            if (response != null && !string.IsNullOrEmpty(response.token))
            {
                ShowFeedback("Login successful!", Color.green, false);

                PlayerPrefs.SetString("AccessToken", response.token);
                PlayerPrefs.DeleteKey("RefreshToken");

                if (response.student != null)
                {
                    PlayerPrefs.SetString("StudentId", response.student.id ?? string.Empty);
                    PlayerPrefs.SetInt("StudentGrade", response.student.grade);
                    PlayerPrefs.SetString("StudentSection", response.student.section ?? string.Empty);
                    PlayerPrefs.SetInt("MustChangePassword", response.student.must_change_password ? 1 : 0);
                }

                PlayerPrefs.Save();

                SceneManager.LoadScene(hubSceneName);
                yield break;
            }

            ShowFeedback("An unknown error occurred.", Color.red, true);
            yield break;
        }

        string errorMessage = ExtractMessage(request.downloadHandler.text, "Login failed.");
        ShowFeedback(errorMessage, Color.red, true);
    }

    // --- NEW HELPER FUNCTIONS FOR UX ---

    // This function sets the text and color, and decides if it should auto-hide
    private void ShowFeedback(string message, Color color, bool autoHide)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);

        if (feedbackHideCoroutine != null)
        {
            StopCoroutine(feedbackHideCoroutine);
            feedbackHideCoroutine = null;
        }

        if (autoHide)
        {
            feedbackHideCoroutine = StartCoroutine(HideFeedbackAfterDelay(3f));
        }
    }

    IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.gameObject.SetActive(false);
        feedbackHideCoroutine = null;
    }

    private static string ExtractMessage(string rawJson, string fallbackMessage)
    {
        if (string.IsNullOrEmpty(rawJson))
        {
            return fallbackMessage;
        }

        StudentApiMessageResponse response = JsonUtility.FromJson<StudentApiMessageResponse>(rawJson);
        if (response != null && !string.IsNullOrEmpty(response.message))
        {
            return response.message;
        }

        return fallbackMessage;
    }
}

// --- JSON Data Structures ---
[Serializable]
public class StudentLoginRequest
{
    public string lrn;
    public string pin;
}

[Serializable]
public class StudentLoginSuccessResponse
{
    public string message;
    public string token;
    public StudentProfile student;
}

[Serializable]
public class StudentProfile
{
    public string id;
    public string full_name;
    public int grade;
    public string section;
    public bool must_change_password;
}

[Serializable]
public class StudentApiMessageResponse
{
    public string message;
}
