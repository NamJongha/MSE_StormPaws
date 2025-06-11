using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

//njh
public class TokenManager : MonoBehaviour
{
    public static TokenManager Instance { get; private set; }
    public static bool hasToken = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void SaveToken(string accessToken, string refreshToken)
    {
        PlayerPrefs.SetString("accessToken", accessToken);
        PlayerPrefs.SetString("refreshToken", refreshToken);
        PlayerPrefs.Save();
        hasToken = true;
        //Debug.Log("token saved successfully");
    }

    public static (string accessToken, string refreshToken) LoadTokens()
    {
        string accessToken = PlayerPrefs.GetString("accessToken", null);
        string refreshToken = PlayerPrefs.GetString("refreshToken", null);
        return (accessToken, refreshToken);
    }

    public static string getAccessToken()
    {
        string accessToken = PlayerPrefs.GetString("accessToken", "");
        return accessToken;
    }

    //Function to put access token in the header of the request
    /* How to use
     * When declaring API call, it always defined in type of 'UnityWebRequest request = UnityWebRequest.Get(URL)'
     * You can use this function with giving the defined UnityWebRequest as parameter to put access token in header
    */
    public static void SendServerToken(UnityWebRequest request)
    {
        if (string.IsNullOrEmpty(getAccessToken()))
        {
            Debug.LogWarning("access token missing");
        }
        else
        {
            request.SetRequestHeader("Authorization", "Bearer " + getAccessToken());
        }
    }

    //check expired state of access token
    public static bool isAccessTokenExpired(string accessToken)
    {
        string[] parts = accessToken.Split('.'); //jwt token is devided by '.' into 3 different information - header, payload, signature
        if (parts.Length < 2)
        {
            hasToken = false;
            return true;
        }

        string payloadJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(parts[1])));
        JwtPayload payload = JsonUtility.FromJson<JwtPayload>(payloadJson);

        long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if(currentUnixTime >= payload.exp)
        {
            hasToken = false;
        }
        return currentUnixTime >= payload.exp; //true if the token is expired

    }

    //Send refresh token to backend spring server, and get new access token as response
    private IEnumerator RefreshTokenCoroutine(Action<bool> onComplete)
    {
        string refreshToken = PlayerPrefs.GetString("refreshToken", "");
        if (string.IsNullOrEmpty(refreshToken))
        {
            Debug.LogError("No refresh token found.");
            onComplete(false);
            yield break;
        }

        string url = "http://localhost:8080/user/refreshToken";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        string json = JsonUtility.ToJson(new RefreshRequest { refreshToken = refreshToken });

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var jsonResponse = request.downloadHandler.text;
            var newTokenData = JsonUtility.FromJson<AuthDataWrapper>(jsonResponse);
            //if the refresh token is valid
            if (newTokenData.success)
            {
                SaveToken(newTokenData.data.accessToken, newTokenData.data.refreshToken);
                Debug.Log("Access token refreshed.");
                onComplete(true);
            }
            //if the refresh token is invalid
            else
            {
                Debug.Log("Refresh token invalid. Please login again");
                onComplete(false);
            }
        }
        else
        {
            Debug.LogError("Failed to refresh token: " + request.error);
            onComplete(false);
        }
    }

    /* Padding base64 incoded-string explanation
     * Unity's Convert.FromBase64String() only allows standard base64 string, but the return type of jwt token is in incoded-type, the base64Url
     */
    private static string PadBase64(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }
        return base64.Replace('-', '+').Replace('_', '/');
    }

    //method to return to login scene if the refersh token is expired
    private void LoadLoginScene()
    {
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.DeleteKey("refreshToken");

        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}

#region jwt token expiration time data
[Serializable]
public class JwtPayload
{
    public long exp;
}
#endregion

#region wrapper to send refresh token to server
[Serializable]
public class RefreshRequest
{
    public string refreshToken;
}
#endregion

#region authData wrapper
[Serializable]
public class AuthDataWrapper
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