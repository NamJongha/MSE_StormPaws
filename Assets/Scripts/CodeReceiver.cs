using System.Net;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine.Networking;

public class CodeReceiver : MonoBehaviour
{
    private HttpListener httpListener;
    private const string RedirectUri = "http://localhost:5000/";
    private Queue<string> receivedCodes = new Queue<string>();

    void Start()
    {
        StartServer();
    }

    void Update()
    {
        if (receivedCodes.Count > 0)
        {
            string code = receivedCodes.Dequeue();
            Debug.Log("Main thread received code: " + code);
            StartCoroutine(SendCodeCoroutine(code));
        }
    }

    //httpListener는 백그라운드 스레드, UnityWebRequest는 메인 스레드에서 실행되야 함
    public void StartServer()
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add(RedirectUri);
        httpListener.Start();
        Debug.Log("OAuth Redirect Server started.");
        Task.Run(() => WaitForRequest());
    }

    private async Task WaitForRequest()
    {
        var context = await httpListener.GetContextAsync();
        var request = context.Request;

        // 인증 코드 직접 파싱
        string code = null;
        string query = request.Url.Query;
        if (query.StartsWith("?"))
            query = query.Substring(1);

        foreach (var param in query.Split('&'))
        {
            var kvp = param.Split('=');
            if (kvp.Length == 2 && kvp[0] == "code")
            {
                code = Uri.UnescapeDataString(kvp[1]);
                break;
            }
        }

        Debug.Log("Received OAuth Code (in background thread): " + code);

        var response = context.Response;
        string responseString = "<html><body>Login successful! You can close this window.</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();

        // 메인 스레드로 전달
        lock (receivedCodes)
        {
            receivedCodes.Enqueue(code);
        }
        //코루틴의 start 자체는 메인 스레드에서 이루어지기 때문에 코루틴에 넣어줄 code를 메인으로 이동시켜줘야한다.
    }

    private IEnumerator SendCodeCoroutine(string code)
    {
        string backendUrl = "http://localhost:8082/user/getToken/google";

        CodePayload payload = new CodePayload { code = code };
        string json = JsonUtility.ToJson(payload);

        UnityWebRequest request = new UnityWebRequest(backendUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log(json);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Backend login success!");
            Debug.Log(request.downloadHandler.text);

            string jsonResponse = request.downloadHandler.text;
            ResponseFromServer responseFromServer = JsonUtility.FromJson<ResponseFromServer>(jsonResponse);

            string accessToken = responseFromServer.data.accessToken;
            string refreshToken = responseFromServer.data.refreshToken;

            Debug.Log("access: " + accessToken);
            Debug.Log("refresh: " + refreshToken);
        }
        else
        {
            Debug.LogError("Login failed: " + request.error);
        }
    }

    #region JsonUtilityParameter
    [Serializable]
    public class CodePayload
    {
        public string code;
    }
    #endregion

    #region JsonResponseParameter
    [Serializable]
    public class ResponseFromServer
    {
        public bool success;
        public string message;
        public AuthDataDTO data;
    }

    [Serializable]
    public class AuthDataDTO
    {
        public string accessToken;
        public string refreshToken;
    }
    #endregion
}
