using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;

public class CodeGen : MonoBehaviour
{
    public TMP_Text codeText;
    public TMP_Text playerText;
    public int codeLength = 6;
    public static string code;
 

    void Start()
    {
        code = GenerateCode(codeLength);
        codeText.text = code;
        StartCoroutine(RunSequence(code)); // poller bagefter
    }

    string GenerateCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Text.StringBuilder code = new System.Text.StringBuilder();
        for (int i = 0; i < length; i++)
        {
            int index = Random.Range(0, chars.Length);
            code.Append(chars[index]);
        }
        return code.ToString();
    }

    private IEnumerator RunSequence(string code)
    {
        yield return StartCoroutine(SendCode(code));

        while (true)
        {
            yield return StartCoroutine(GetPlayers(code));
            yield return new WaitForSeconds(1f); // 1s ventetid
        }
    }

    private IEnumerator SendCode(string message)
    {
        string url = "http://localhost:8080/game";
        string jsonData = "{\"code\":\"" + message + "\"}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {   
            Debug.Log("SendCode Response: " + request.downloadHandler.text);
        }
    }

    [System.Serializable]
    public class ResponseMessage
    {
        public string message;
    }

    private IEnumerator GetPlayers(string code)
    {
        string url = "http://localhost:8080/playerCount";
        string jsonData = "{\"code\":\"" + code + "\"}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            string jsonResponse = request.downloadHandler.text;
            ResponseMessage parsed = JsonUtility.FromJson<ResponseMessage>(jsonResponse);

            if (parsed != null && !string.IsNullOrEmpty(parsed.message))
            {
                Debug.Log("Message: " + parsed.message);
                playerText.text = parsed.message + "/8 Spillere";

                if (int.Parse(parsed.message) == 8)
                {
                    Debug.Log("complete");
                    SceneManager.LoadScene("Game");
                }

            }
            else
            {
                Debug.LogWarning("Response had no 'message' field: " + jsonResponse);
            }
        }
    }
}
