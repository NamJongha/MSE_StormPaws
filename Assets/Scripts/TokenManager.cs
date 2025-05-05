using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TokenManager : MonoBehaviour
{
    public static TokenManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
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
        Debug.Log("token saved successfully");
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

    //서버에 요청을 보낼 때 헤더에 토큰을 넣어주는 함수
    /* 사용방법
     * 서버에 Api 요청을 할 때 UnityWebRequest request = UnityWebRequest.Get(URL) 이런 형식으로 request 선언문을 작성하게 됨
     * 이 선언문 다음 아래 함수를 호출하면서 매개변수로 이 request를 넣어서 사용하면 됩니다.
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

    //accessToken의 만료 검사
    public static bool isAccessTokenExpired(string accessToken)
    {
        string[] parts = accessToken.Split('.'); //jwt토큰은 .을 기반으로 header.payload.signature 로 나누어짐
        if (parts.Length < 2)
        {
            return true;
        }

        string payloadJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(parts[1])));
        JwtPayload payload = JsonUtility.FromJson<JwtPayload>(payloadJson);

        long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return currentUnixTime >= payload.exp;

    }

    //refresh token을 백엔드 서버로 넘기며 access token을 새로 발급받음
    private IEnumerator RefreshTokenCoroutine(Action<bool> onComplete)
    {
        string refreshToken = PlayerPrefs.GetString("refreshToken", "");
        if (string.IsNullOrEmpty(refreshToken))
        {
            Debug.LogError("No refresh token found.");
            onComplete(false);
            yield break;
        }

        string url = "http://localhost:8082/user/refreshToken"; // 백엔드 서버에 refresh토큰으로 access token을 반환해주는 엔드포인트 만들어야함
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
            if (newTokenData.success)//refresh유효한 경우
            {
                SaveToken(newTokenData.data.accessToken, newTokenData.data.refreshToken);
                Debug.Log("Access token refreshed.");
                onComplete(true);
            }
            else//refresh만료된 경우
            {
                Debug.Log("Refresh token expired. Please login again");
                onComplete(false);
            }
        }
        else
        {
            Debug.LogError("Failed to refresh token: " + request.error);
            onComplete(false);
        }
    }

    /*base64로 인코딩된 문자열을 보완함 설명
     * unity의 Convert.FromBase64String()가 표준 base64 문자열만 허용하지만 jwt토큰은 base64Url이라는 형태로 변형된 형식이기 때문
     */
    private static string PadBase64(string base64)
    {
        switch(base64.Length % 4){
            case 2: 
                base64 += "==";
                break;
            case 3: 
                base64 += "="; 
                break;
        }
            return base64.Replace('-', '+').Replace('_', '/');
    }

    //refresh만료시 다시 로그인 화면으로 이동
    private void LoadLoginScene()
    {
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.DeleteKey("refreshToken");

        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}

#region jwt토큰 만료 시간 데이터
[Serializable]
public class JwtPayload
{
    public long exp;
}
#endregion

#region 서버에 refresh토큰을 보내서 새 access token 요청을 수행할 객체
[Serializable]
public class RefreshRequest
{
    public string refreshToken;
}
#endregion

#region authData객체
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
