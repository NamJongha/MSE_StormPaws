using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;

public class OpponentSelect : MonoBehaviour
{
    public GameObject deckSlotPrefab;
    public Transform slotParent;

    private List<GameManager.OpponentDeck> deckList;

    void Start()
    {
        StartCoroutine(GetOpponentDecks());
    }

    IEnumerator GetOpponentDecks()
    {
        string token = GameManager.Instance.GetAuthToken();
        string url = $"{GameManager.Instance.baseUrl}/decks/random?page=1&size=10";
        Debug.Log($"[URL] {url}");
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + token.Trim());
        Debug.Log($"[TOKEN] {GameManager.Instance.GetAuthToken()}");
        Debug.Log($"[RESPONSE] {req.responseCode} / {req.downloadHandler.text}");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<OpponentDeckListResponse>(req.downloadHandler.text);
            deckList = response.data.items;

            foreach (var deck in deckList)
            {
                GameObject slot = Instantiate(deckSlotPrefab, slotParent);
                var ui = slot.GetComponent<OpponentDeckSlotUI>();
                ui.SetDeck(deck);
            }
        }
        else
        {
            Debug.LogError("상대 덱 불러오기 실패");
        }
    }
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
    public List<GameManager.OpponentDeck> items;
    public int totalItems;
    public int totalPages;
    public int currentPage;
    public int pageSize;
    public bool hasPrevious;
    public bool hasNext;
}

