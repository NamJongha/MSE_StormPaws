using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Game Manager Script
/// Get Request & Token Method
/// </summary>

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public UserService UserService { get; private set; }
    public DeckService DeckService { get; private set; }
    public SpriteLoader SpriteLoader { get; private set; }
    public BattleService BattleService { get; private set; }

    public string baseUrl => "https://stormpaws.duckdns.org";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        UserService = new UserService();
        DeckService = new DeckService();
        SpriteLoader = new SpriteLoader();
        BattleService = new BattleService();
    }

    private void Start()
    {
        StartCoroutine(
            GetRequest($"{baseUrl}/user/me", (json) =>
            {
                PersonalInfoWrapper wrapper = JsonUtility.FromJson<PersonalInfoWrapper>(json);
                string playerId = wrapper.data.id;
                PlayerPrefs.SetString("PlayerId", playerId);
                Debug.Log(playerId + "saved successfully");
            }));
    }

    public string GetAuthToken()
    {
        return PlayerPrefs.GetString("accessToken");
    }

    public static void SendServerToken(UnityWebRequest request)
    {
        string token = PlayerPrefs.GetString("accessToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token.Trim()}");
        }
    }

    public IEnumerator GetRequest(string url, Action<string> onSuccess, Action<string> onError = null)
    {
        string token = GetAuthToken();

        UnityWebRequest request = UnityWebRequest.Get(url);
        string authHeader = $"Bearer {token.Trim()}";
        TokenManager.SendServerToken(request);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }
}
