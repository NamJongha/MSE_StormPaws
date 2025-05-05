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

    //������ ��û�� ���� �� ����� ��ū�� �־��ִ� �Լ�
    /* �����
     * ������ Api ��û�� �� �� UnityWebRequest request = UnityWebRequest.Get(URL) �̷� �������� request ������ �ۼ��ϰ� ��
     * �� ���� ���� �Ʒ� �Լ��� ȣ���ϸ鼭 �Ű������� �� request�� �־ ����ϸ� �˴ϴ�.
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

    //accessToken�� ���� �˻�
    public static bool isAccessTokenExpired(string accessToken)
    {
        string[] parts = accessToken.Split('.'); //jwt��ū�� .�� ������� header.payload.signature �� ��������
        if (parts.Length < 2)
        {
            return true;
        }

        string payloadJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(parts[1])));
        JwtPayload payload = JsonUtility.FromJson<JwtPayload>(payloadJson);

        long currentUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return currentUnixTime >= payload.exp;

    }

    //refresh token�� �鿣�� ������ �ѱ�� access token�� ���� �߱޹���
    private IEnumerator RefreshTokenCoroutine(Action<bool> onComplete)
    {
        string refreshToken = PlayerPrefs.GetString("refreshToken", "");
        if (string.IsNullOrEmpty(refreshToken))
        {
            Debug.LogError("No refresh token found.");
            onComplete(false);
            yield break;
        }

        string url = "http://localhost:8082/user/refreshToken"; // �鿣�� ������ refresh��ū���� access token�� ��ȯ���ִ� ��������Ʈ ��������
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
            if (newTokenData.success)//refresh��ȿ�� ���
            {
                SaveToken(newTokenData.data.accessToken, newTokenData.data.refreshToken);
                Debug.Log("Access token refreshed.");
                onComplete(true);
            }
            else//refresh����� ���
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

    /*base64�� ���ڵ��� ���ڿ��� ������ ����
     * unity�� Convert.FromBase64String()�� ǥ�� base64 ���ڿ��� ��������� jwt��ū�� base64Url�̶�� ���·� ������ �����̱� ����
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

    //refresh����� �ٽ� �α��� ȭ������ �̵�
    private void LoadLoginScene()
    {
        PlayerPrefs.DeleteKey("accessToken");
        PlayerPrefs.DeleteKey("refreshToken");

        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}

#region jwt��ū ���� �ð� ������
[Serializable]
public class JwtPayload
{
    public long exp;
}
#endregion

#region ������ refresh��ū�� ������ �� access token ��û�� ������ ��ü
[Serializable]
public class RefreshRequest
{
    public string refreshToken;
}
#endregion

#region authData��ü
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
