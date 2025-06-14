using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
            GameManager.Instance.GetRequest($"{GameManager.Instance.baseUrl}/cards?page=1&size=100",
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

    public void DeleteDecks(List<string> deckIds, Action<List<DeckPreset>> onComplete = null)
    {
        GameManager.Instance.StartCoroutine(DeleteDecksCoroutine(deckIds, onComplete));
    }

    private IEnumerator DeleteDecksCoroutine(List<string> deckIds, Action<List<DeckPreset>> onComplete = null)
    {
        string url = $"{GameManager.Instance.baseUrl}/user/me/decks";

        var requestBody = new DeleteDeckRequest { deckIds = deckIds };
        string json = JsonUtility.ToJson(requestBody);

        UnityWebRequest request = new UnityWebRequest(url, "DELETE");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.GetAuthToken());

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Deck(s) deleted");

            ClearDeckCache();
            FetchDeckPresets(onComplete);
        }
        else
        {
            Debug.LogError("Delete failed: " + request.error);
        }
    }

    [Serializable]
    private class DeleteDeckRequest
    {
        public List<string> deckIds;
    }

}
