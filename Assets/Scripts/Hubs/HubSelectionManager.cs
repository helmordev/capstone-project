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
    public string apiBaseUrl = "http://localhost:8000";
    public string nextSceneName = "GameplayScene";

    private Coroutine feedbackHideCoroutine;
    private bool isJoining;

    void Start()
    {
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    public void OnJoinButtonClicked()
    {
        if (isJoining)
        {
            return;
        }

        string enteredCode = codeInput.text.Trim(); // .Trim() removes accidental spaces

        if (string.IsNullOrEmpty(enteredCode))
        {
            ShowFeedback("Please enter the server code.", Color.red, true);
            return;
        }

        SetJoinInProgress(true);
        ShowFeedback("Verifying code...", Color.yellow, false);

        StartCoroutine(JoinHubRoutine(enteredCode));
    }

    IEnumerator JoinHubRoutine(string code)
    {
        string accessToken = PlayerPrefs.GetString("AccessToken", "");

        if (string.IsNullOrEmpty(accessToken))
        {
            ShowFeedback("Error: You are not logged in. Please restart.", Color.red, false);
            SetJoinInProgress(false);
            yield break;
        }

        StudentJoinRoomRequest requestData = new StudentJoinRoomRequest { room_code = code };
        string jsonData = JsonUtility.ToJson(requestData);

        string joinEndpoint = apiBaseUrl + "/api/v1/student/join-room";
        UnityWebRequest request = new UnityWebRequest(joinEndpoint, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        SetJoinInProgress(false);

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
        {
            ShowFeedback("Network error. Could not reach server.", Color.red, true);
            yield break;
        }

        if (request.responseCode >= 200 && request.responseCode < 300)
        {
            StudentJoinRoomSuccessResponse response = JsonUtility.FromJson<StudentJoinRoomSuccessResponse>(request.downloadHandler.text);

            if (response != null && HasValidClassroom(response.classroom))
            {
                ShowFeedback(BuildSuccessMessage(response.message), Color.green, false);
                PlayerPrefs.SetString("ClassroomId", response.classroom.id ?? string.Empty);
                PlayerPrefs.SetString("ClassroomName", response.classroom.name ?? string.Empty);
                PlayerPrefs.SetInt("ClassroomGrade", response.classroom.grade);
                PlayerPrefs.SetString("ClassroomSection", response.classroom.section ?? string.Empty);
                PlayerPrefs.SetString("ClassroomRoomCode", response.classroom.room_code ?? string.Empty);
                PlayerPrefs.Save();

                yield return new WaitForSeconds(1f);
                SceneManager.LoadScene(nextSceneName);
                yield break;
            }

            ShowFeedback("An unknown error occurred.", Color.red, true);
            yield break;
        }

        string errorMessage = ExtractMessage(request.downloadHandler.text, "Failed to join room.");
        ShowFeedback(errorMessage, Color.red, true);
    }

    private void SetJoinInProgress(bool inProgress)
    {
        isJoining = inProgress;

        if (joinButton != null)
        {
            joinButton.interactable = !inProgress;
        }
    }

    private static bool HasValidClassroom(StudentClassroom classroom)
    {
        return classroom != null && !string.IsNullOrEmpty(classroom.id);
    }

    private static string BuildSuccessMessage(string serverMessage)
    {
        if (!string.IsNullOrEmpty(serverMessage))
        {
            string trimmedMessage = serverMessage.Trim();

            if (trimmedMessage.IndexOf("already", StringComparison.OrdinalIgnoreCase) >= 0 ||
                trimmedMessage.IndexOf("success", StringComparison.OrdinalIgnoreCase) >= 0 ||
                trimmedMessage.IndexOf("joined", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return trimmedMessage;
            }
        }

        return "Joined room successfully.";
    }

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
public class StudentJoinRoomRequest
{
    public string room_code;
}

[Serializable]
public class StudentJoinRoomSuccessResponse
{
    public string message;
    public StudentClassroom classroom;
}

[Serializable]
public class StudentClassroom
{
    public string id;
    public string name;
    public int grade;
    public string section;
    public string room_code;
}
