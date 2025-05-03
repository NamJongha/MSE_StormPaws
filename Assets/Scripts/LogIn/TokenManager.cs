using UnityEngine;

public class TokenManager : MonoBehaviour
{
    public static void SaveToken(string accessToken, string refreshToken)
    {
        PlayerPrefs.SetString("accessToken", accessToken);
        PlayerPrefs.SetString("refreshToken", refreshToken);
        PlayerPrefs.Save();
    }

    public static (string accessToken, string refreshToken) LoadTokens()
    {
        string accessToken = PlayerPrefs.GetString("accessToken", null);
        string refreshToken = PlayerPrefs.GetString("refreshToken", null);
        return (accessToken, refreshToken);
    }
}
