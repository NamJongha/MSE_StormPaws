using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEditor.PackageManager.Requests;

/// <summary>
/// Entire of My Page Feature And Game Management Class
/// </summary>

public class GameManager : MonoBehaviour
{
    // Singletone Instance
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 테스트용 JWT 강제 저장
            PlayerPrefs.SetString("jwt", "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiI1M2ZkODNiNy00NTlkLTRmYjQtOTVlMy1lMzViMTEzNDIzYjYiLCJvYXV0aFR5cGUiOiJnb29nbGUiLCJvYXV0aElkIjoiMTA1MzUyMjQ0NTA1NjU2MDAwOTIzIiwiZW1haWwiOiJkYnNyYmFsczI2QGdtYWlsLmNvbSIsImlhdCI6MTc0NjExOTI5NywiZXhwIjoxNzUyMTY3Mjk3fQ.GPg7V9P0ezFrQvpkR-xWnWc8nfXz0RnwBOs-JaHEvXs");
            PlayerPrefs.Save();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // API Basic URL
    public string baseUrl = "http://localhost:8080";

    // Personal Information
    [System.Serializable]
    public class PersonalInfo
    {
        public string id;
        public string email;
        public string nickname;
    }

    // Card Data
    [System.Serializable]
    public class Card
    {
        public string id;
        public string name;
        public int attackPower;
        public int attackSpeed;
        public int health;
        public string cardType;
        public int additionalCoefficient;
    }

    [System.Serializable]
    public class DeckCard
    {
        public string id;
        public Card card;
        public int pos;
        public int cardQuantity;
    }

    [System.Serializable]
    public class DeckPreset
    {
        public string id;
        public string name;
        public List<DeckCard> decklist;
    }

    [System.Serializable]
    public class DeckListResponse
    {
        public bool success;
        public string message;
        public DeckListData data;
    }

    [System.Serializable]
    public class DeckListData
    {
        public List<DeckPreset> items;
    }

    [System.Serializable]
    public class BattleRecord
    {
        public string battleId;
        public string weather;
        public string opponent;
        public string myDeck;
        public string opponentDeck;
        public string result;
    }

    [System.Serializable]
    public class OpponentDeck
    {
        public string ownerName;
        public string id;
        public List<DeckCard> decklist;
    }

    [System.Serializable]
    public class SelectedOpponentResponse
    {
        public bool success;
        public string message;
        public OpponentDeck data;
    }

    [System.Serializable]
    public class CardListResponse
    {
        public bool success;
        public string message;
        public CardListData data;
    }

    [System.Serializable]
    public class CardListData
    {
        public List<Card> items;
    }

    [System.Serializable]
    public class SelectedMyDeckResponse
    {
        public bool success;
        public string message;
        public DeckPreset data;
    }

    // JSON Parsing
    [System.Serializable]
    public class PersonalInfoWrapper
    {
        public bool success;
        public string message;
        public PersonalInfo data;
    }

    [System.Serializable]
    public class BattleRecordListWrapper
    {
        public List<BattleRecord> data;
    }

    // Caching
    private PersonalInfo cachedInfo = null;
    private List<DeckPreset> cachedDeckPresets = null;

    // Personal Info UI
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI idText;

    private IEnumerator Start()
    {
        yield return null; // 한 프레임 기다리기

        FetchPersonalInfo(info =>
        {
            nameText.text = info.nickname;
            idText.text = info.id;
            mainText.text = info.nickname;
            emailText.text = info.email;
        });
    }


    // JWT Token Management
    public string GetAuthToken()
    {
        return PlayerPrefs.GetString("jwt");
        // 테스트용 하드코딩
        //return "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiI1M2ZkODNiNy00NTlkLTRmYjQtOTVlMy1lMzViMTEzNDIzYjYiLCJvYXV0aFR5cGUiOiJnb29nbGUiLCJvYXV0aElkIjoiMTA1MzUyMjQ0NTA1NjU2MDAwOTIzIiwiZW1haWwiOiJkYnNyYmFsczI2QGdtYWlsLmNvbSIsImlhdCI6MTc0NjExOTI5NywiZXhwIjoxNzUyMTY3Mjk3fQ.GPg7V9P0ezFrQvpkR-xWnWc8nfXz0RnwBOs-JaHEvXs";
    }

    // Callback Personal Information
    public void FetchPersonalInfo(Action<PersonalInfo> callback)
    { 
        if (cachedInfo != null)
        {
            callback?.Invoke(cachedInfo);
            return;
        }

        StartCoroutine(GetRequest($"{baseUrl}/user/me", (json) =>
        {
            PersonalInfoWrapper wrapper = JsonUtility.FromJson<PersonalInfoWrapper>(json);
            cachedInfo = wrapper.data;
            callback?.Invoke(wrapper.data);
        }));
    }

    // Searching My Deck List
    public void FetchDeckPresets(Action<List<DeckPreset>> callback)
    {
        if (cachedDeckPresets != null)
        {
            callback?.Invoke(cachedDeckPresets);
            return;
        }

        StartCoroutine(GetRequest($"{baseUrl}/user/me/decks?page=1&size=10", (json) =>
        {
            DeckListResponse wrapper = JsonUtility.FromJson<DeckListResponse>(json);
            cachedDeckPresets = wrapper.data.items;
            callback?.Invoke(cachedDeckPresets);
        }));
    }

    public void FetchBattleRecords(Action<List<BattleRecord>> callback)
    {
        StartCoroutine(GetRequest($"{baseUrl}/battle/logs", (json) =>
        {
            BattleRecordListWrapper wrapper = JsonUtility.FromJson<BattleRecordListWrapper>(json);
            callback?.Invoke(wrapper.data);
        }));
    }

    public void FetchSelectedOpponentDeck(Action<OpponentDeck> callback)
    {
        StartCoroutine(GetRequest($"{baseUrl}/battle/selected-opponent", (json) =>
        {
            SelectedOpponentResponse wrapper = JsonUtility.FromJson<SelectedOpponentResponse>(json);
            callback?.Invoke(wrapper.data);
        }));
    }

    public void FetchAllCards(Action<List<Card>> callback)
    {
        StartCoroutine(GetRequest($"{baseUrl}/cards?page=1&size=100", (json) =>
        {
            CardListResponse wrapper = JsonUtility.FromJson<CardListResponse>(json);
            callback?.Invoke(wrapper.data.items);
        }));
    }

    public void FetchSelectedMyDeck(System.Action<DeckPreset> callback)
    {
        StartCoroutine(GetRequest($"{baseUrl}/battle/selected-my-deck", (json) =>
        {
            SelectedMyDeckResponse wrapper = JsonUtility.FromJson<SelectedMyDeckResponse>(json);
            callback?.Invoke(wrapper.data);
        }));
    }

    private IEnumerator GetRequest(string url, Action<string> onSuccess, Action<string> onError = null)
    {
        Debug.Log($"[GET Request] {url}");

        string token = GetAuthToken();
        Debug.Log($"JWT Token: {token}");

        UnityWebRequest request = UnityWebRequest.Get(url);
        string authHeader = $"Bearer {token.Trim()}";
        request.SetRequestHeader("Authorization", authHeader);
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"Authorization Header Set: {authHeader}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError($"API Error: {request.error}");
            onError?.Invoke(request.error);
        }
    }

    // Sprite Loading
    private Dictionary<string, string> spriteNameMap = new Dictionary<string, string>
    {
        { "개구리", "frog" },
        { "타조", "ostrich" },
        { "아프리카 코끼리", "elephant" },
        { "햄스터", "hamster" },
        { "목도리 도마뱀", "geko" },
        { "사자", "lion" },
        { "흰수염고래", "whale" },
        { "원숭이", "monkey" },
        { "말", "horse" },
        { "호랑이", "tiger" },
        { "북극곰", "polarbear" },
        { "문어", "octopus" },
        { "나무늘보", "sloth" },
        { "하마", "hippo" },
        { "기린", "giraffe" }
    };

    public Sprite LoadAnimalSprite(string cardName)
    {
        if (spriteNameMap.TryGetValue(cardName, out string spriteName))
        {
            return Resources.Load<Sprite>($"Animals/{spriteName}");
        }

        Debug.LogWarning($"Cannot Find Sprite: {cardName}");
        return null;

        //return Resources.Load<Sprite>("Animals/default");
    }
}