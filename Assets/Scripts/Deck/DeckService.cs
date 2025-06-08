using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckService
{
    private List<DeckPreset> cachedDeckPresets;
    public List<OpponentDeck> opponentDeckList = new List<OpponentDeck>();

    private string selectedMyDeckId;
    private string selectedOpponentDeckId;

    private string selectedOpponentUserId;

    public void FetchAllCards(Action<List<Card>> callback)
    {
        GameManager.Instance.StartCoroutine(
            GameManager.Instance.GetRequest($"{GameManager.Instance.baseUrl}/cards?page=1&size=10",
            (json) =>
            {
                CardListResponse wrapper = JsonUtility.FromJson<CardListResponse>(json);
                callback?.Invoke(wrapper.data.items);
            },
            (error) =>
            {
                callback?.Invoke(null);
            }));
    }

    public void FetchDeckById(string deckId, Action<DeckPreset> callback)
    {
        string url = $"{GameManager.Instance.baseUrl}/decks/{deckId}";
        GameManager.Instance.StartCoroutine(
            GameManager.Instance.GetRequest(url,
            (json) =>
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

    // Searching My Deck List
    public void FetchDeckPresets(Action<List<DeckPreset>> callback)
    {
        if (cachedDeckPresets != null)
        {
            callback?.Invoke(cachedDeckPresets);
            return;
        }

        GameManager.Instance.StartCoroutine(GameManager.Instance.GetRequest($"{GameManager.Instance.baseUrl}/user/me/decks?page=1&size=100", (json) =>
        {
            DeckListResponse wrapper = JsonUtility.FromJson<DeckListResponse>(json);
            cachedDeckPresets = wrapper.data.items;
            callback?.Invoke(cachedDeckPresets);
        }));
    }

    public void SetSelectedOpponentDeck(OpponentDeck deck)
    {
        selectedOpponentDeckId = deck.id;
        selectedOpponentUserId = deck.user.id;

        PlayerPrefs.SetString("SelectedOpponentDeckId", deck.id);
        PlayerPrefs.SetString("SelectedOpponentUserId", deck.user.id);

        Debug.Log("oppo" + selectedOpponentUserId);
        PlayerPrefs.Save();
    }

    public void SetSelectedMyDeck(DeckPreset myDeck)
    {
        selectedMyDeckId = myDeck.id;

        PlayerPrefs.SetString("SelectedMyDeckId", selectedMyDeckId);
        PlayerPrefs.Save();
    }

    public string GetSelectedMyDeckId()
    {
        return PlayerPrefs.GetString("SelectedMyDeckId", "");
    }

    public string GetSelectedOpponentDeckId()
    {
        return PlayerPrefs.GetString("SelectedOpponentDeckId", "");
    }

    public void SetOpponentDeckList(List<OpponentDeck> list)
    {
        opponentDeckList = list;
    }

    public List<OpponentDeck> GetOpponentDeckList()
    {
        return opponentDeckList;
    }

    public void ClearDeckCache()
    {
        cachedDeckPresets = null;
    }
}
