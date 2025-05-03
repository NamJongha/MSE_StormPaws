using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

/// <summary>
/// Battle Scene Manager Script
/// </summary>

public class BattleManager : MonoBehaviour
{
    public Transform backgroundContainer;
    public GameManager gameManager;

    public TMP_Text weatherText;
    public TMP_Text cityText;

    void Start()
    {
        StartCoroutine(FetchBattleEnvironment());
    }

    private IEnumerator FetchBattleEnvironment()
    {
        string url = $"{gameManager.baseUrl}/battle/environment";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + gameManager.GetAuthToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            BattleEnvResponse response = JsonUtility.FromJson<BattleEnvResponse>(request.downloadHandler.text);
            string weather = response.data.weather.ToLower();
            string city = response.data.city;

            weatherText.text = GetWeatherKorean(weather);
            cityText.text = city;

            string prefabPath = $"Backgrounds/{weather}_{city}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab != null)
            {
                Instantiate(prefab, backgroundContainer);
            }
            else
            {
                Debug.LogWarning("No Prefab: " + prefabPath);
            }
        }
        else
        {
            Debug.LogError("Fail: " + request.error);
        }
    }

    private string GetWeatherKorean(string weather)
    {
        switch (weather.ToLower())
        {
            case "rain": return "ºñ";
            case "sun": return "¸¼À½";
            case "snow": return "´«";
            case "cloud": return "Èå¸²";
            default: return weather;
        }
    }
}

[System.Serializable]
public class BattleEnvResponse
{
    public bool success;
    public string message;
    public BattleEnvData data;
}

[System.Serializable]
public class BattleEnvData
{
    public string weather;
    public string city;
}
