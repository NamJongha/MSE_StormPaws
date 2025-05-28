using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEditor.PackageManager.Requests;

/// <summary>
/// Entire of My Page Feature And Game Management Class
/// I will restruct later
/// </summary>

public class GameManager : MonoBehaviour
{
    // API Basic URL
    public string baseUrl = "http://localhost:8080";

    // Personal Information
    [System.Serializable]
    public class PersonalInfo
    {
        public string id;
        public string email;
        public string name;
    }

    // Card Data
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
        public string deckName;
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
        public string id;
        public string name;
        public User user;
        public List<DeckCard> decklist;

        public string ownerName => user != null ? user.name : "Unknown";

        [System.Serializable]
        public class User
        {
            public string id;
            public string name;
        }
    }


    [System.Serializable]
    public class SelectedOpponentResponse
    {
        public bool success;
        public string message;
        public OpponentDeck data;
    }

    [System.Serializable]
    public class Card
    {
        public string id;
        public string name;
        public int attackPower;
        public int attackSpeed;
        public int health;
        public string cardType;
        public float additionalCoefficient;
    }

    [System.Serializable]
    public class CardListData
    {
        public List<Card> items;
        public int totalItems;
        public int pageSize;
        public int currentPage;
        public int totalPages;
        public bool hasPrevious;
        public bool hasNext;
    }

    [System.Serializable]
    public class CardListResponse
    {
        public bool success;
        public string message;
        public CardListData data;
    }

    [System.Serializable]
    public class SelectedMyDeckResponse
    {
        public bool success;
        public string message;
        public DeckPreset data;
    }

    [System.Serializable]
    public class OpponentDeckListResponse
    {
        public bool success;
        public string message;
        public OpponentDeckListData data;
    }

    [System.Serializable]
    public class OpponentDeckListData
    {
        public List<OpponentDeck> items;
        public int totalItems;
        public int totalPages;
        public int currentPage;
        public int pageSize;
        public bool hasPrevious;
        public bool hasNext;
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

    private List<OpponentDeck> opponentDeckList = new List<OpponentDeck>();
    private string selectedOpponentDeckId;

    private string selectedMyDeckId;

    // JWT Token Management
    public string GetAuthToken()
    {
        return PlayerPrefs.GetString("accessToken");
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

    //njh save selected my deck id to use it on battle
    public void SetSelectedMyDeck(DeckPreset myDeck)
    {
        selectedMyDeckId = myDeck.id;

        PlayerPrefs.SetString("SelectedMyDeckId", selectedMyDeckId);
        PlayerPrefs.Save();
    }

    public void SetSelectedOpponentDeck(OpponentDeck deck)
    {
        selectedOpponentDeckId = deck.id;

        PlayerPrefs.SetString("SelectedOpponentDeckId", selectedOpponentDeckId);
        PlayerPrefs.Save();
    }

    public void SetOpponentDeckList(List<OpponentDeck> list)
    {
        opponentDeckList = list;
    }

    public List<OpponentDeck> GetOpponentDeckList()
    {
        return opponentDeckList;
    }

    public void FetchDeckById(string deckId, Action<DeckPreset> callback)
    {
        string url = $"{baseUrl}/decks/{deckId}";
        StartCoroutine(GetRequest(url, (json) =>
        {
            var response = JsonUtility.FromJson<SelectedMyDeckResponse>(json);
            callback?.Invoke(response.data);
        },
        (error) =>
        {
            Debug.LogError("Fail: " + error);
            callback?.Invoke(null);
        }));
    }

    public void FetchAllCards(Action<List<Card>> callback)
    {
        StartCoroutine(GetRequest($"{baseUrl}/cards?page=1&size=100", (json) =>
        {
            CardListResponse wrapper = JsonUtility.FromJson<CardListResponse>(json);
            callback?.Invoke(wrapper.data.items);
        },
        (error) =>
        {
            callback?.Invoke(null);
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
        //request.SetRequestHeader("Authorization", authHeader);
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

    // Sprite Loading
    private Dictionary<string, string> spriteNameMap = new Dictionary<string, string>
    {
        { "°³±¸¸®", "Frog" },
        { "Å¸Á¶", "Ostrich" },
        { "¾ÆÇÁ¸®Ä« ÄÚ³¢¸®", "Elephant" },
        { "ÇÜ½ºÅÍ", "Hamster" },
        { "¸ñµµ¸® µµ¸¶¹ì", "Geko" },
        { "»çÀÚ", "Lion" },
        { "Èò¼ö¿°°í·¡", "Whale" },
        { "¿ø¼þÀÌ", "Monkey" },
        { "¸»", "Horse" },
        { "È£¶ûÀÌ", "Tiger" },
        { "ºÏ±Ø°õ", "Polarbear" },
        { "¹®¾î", "Octopus" },
        { "³ª¹«´Ãº¸", "Sloth" },
        { "ÇÏ¸¶", "Hippo" },
        { "±â¸°", "Giraffe" }
    };

    public Sprite LoadAnimalSprite(string cardName)
    {

        if (spriteNameMap.TryGetValue(cardName, out string spriteName))
        {
            return Resources.Load<Sprite>($"Animals/{spriteName}");
        }

        Debug.LogWarning($"Cannot Find Sprite: {cardName}");
        return null;
    }
}