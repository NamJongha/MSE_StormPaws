using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public Transform backgroundContainer;
    private GameManager gameManager;

    public TMP_Text weatherText;
    public TMP_Text cityText;

    void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene!");
        }
    }

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

            weatherText.text = weather;
            cityText.text = city;

            string prefabPath = $"Backgrounds/{weather}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab != null)
            {
                Instantiate(prefab, backgroundContainer);
            }
            else
            {
                Debug.LogWarning("Missing background prefab: " + prefabPath);

                GameObject fallback = Resources.Load<GameObject>("Backgrounds/default");
                if (fallback != null)
                    Instantiate(fallback, backgroundContainer);
            }
        }
        else
        {
            Debug.LogError("Fail: " + request.error);
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
