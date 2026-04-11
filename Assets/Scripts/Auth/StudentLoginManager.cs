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
    public string apiBaseUrl = "http://localhost:3000";

    void Start()
    {
        // 1. Hide the feedback text as soon as the game starts
        feedbackText.gameObject.SetActive(false);
    }

    public void OnLoginButtonClicked()
    {
        string studentId = studentIdInput.text;
        string pin = pinInput.text;

        // 2. Show the text when they click, and make it yellow for "processing"
        feedbackText.gameObject.SetActive(true);

        if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(pin))
        {
            ShowFeedback("Please enter both your Student ID and PIN.", Color.red, true);
            return;
        }

        loginButton.interactable = false;

        // Show yellow text while loading
        ShowFeedback("Connecting to server...", Color.yellow, false);

        StartCoroutine(LoginRoutine(studentId, pin));
    }

    IEnumerator LoginRoutine(string studentId, string pin)
    {
        LoginRequest requestData = new LoginRequest { studentId = studentId, pin = pin };
        string jsonData = JsonUtility.ToJson(requestData);

        string loginEndpoint = apiBaseUrl + "/api/v1/students/auth/login";
        UnityWebRequest request = new UnityWebRequest(loginEndpoint, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        loginButton.interactable = true;

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            // 3. Show RED text for network error and hide after 3 seconds
            ShowFeedback("Error: Could not connect to the server.", Color.red, true);
        }
        else
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);

            if (response != null && response.success)
            {
                // 4. Show GREEN text for success! (We don't hide this one so they see it worked)
                ShowFeedback("Login successful!", Color.green, false);

                PlayerPrefs.SetString("AccessToken", response.data.accessToken);
                PlayerPrefs.SetString("RefreshToken", response.data.refreshToken);
                PlayerPrefs.Save();

                // TODO: Load the next UI screen here
                SceneManager.LoadScene(hubSceneName);
            }
            else if (response != null && response.error != null)
            {
                // 5. Show RED text for wrong password/locked account and hide after 3 secs
                if (response.error.code == "AUTH_ACCOUNT_LOCKED")
                {
                    ShowFeedback("Account locked. Please wait 15 minutes.", Color.red, true);
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

    // --- NEW HELPER FUNCTIONS FOR UX ---

    // This function sets the text and color, and decides if it should auto-hide
    private void ShowFeedback(string message, Color color, bool autoHide)
    {
        feedbackText.text = message;
        feedbackText.color = color;

        if (autoHide)
        {
            // Stop any previous hiding timers so they don't overlap, then start a new one
            StopAllCoroutines();
            StartCoroutine(HideFeedbackAfterDelay(3f)); // Wait 3 seconds
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
public class LoginRequest
{
    public string studentId;
    public string pin;
}

[Serializable]
public class AuthResponse
{
    public bool success;
    public AuthData data;
    public AuthError error;
}

[Serializable]
public class AuthData
{
    public string accessToken;
    public string refreshToken;
    public int expiresIn;
}

[Serializable]
public class AuthError
{
    public string code;
    public string message;
}