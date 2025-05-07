using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using static GameManager;

/// <summary>
/// Selecting Opponent Deck Randomly for The Battle
/// </summary>

public class OpponentSelect : MonoBehaviour
{
    public GameObject deckSlotPrefab;
    public Transform slotParent;
    public GameManager gameManager;
    private List<GameManager.OpponentDeck> deckList;

    void Start()
    {
        StartCoroutine(GetOpponentDecks());
    }

    IEnumerator GetOpponentDecks()
    {
        string token = gameManager.GetAuthToken();
        string url = $"{gameManager.baseUrl}/decks/random";

        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + token.Trim());
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<OpponentDeckListResponse>(req.downloadHandler.text);
            if (response == null)
            {
                Debug.LogError("JSON Parsing Fail");
                yield break;
            }

            if (response.success && response.data != null && response.data.items != null)
            {
                Debug.Log("Deck Count: " + response.data.items.Count);
                deckList = response.data.items;
                gameManager.SetOpponentDeckList(deckList);

                foreach (Transform child in slotParent)
                {
                    Destroy(child.gameObject);
                }

                for (int i = 0; i < deckList.Count; i++)
                {
                    GameObject slot = Instantiate(deckSlotPrefab, slotParent);
                    var ui = slot.GetComponent<OpponentDeckSlotUI>();
                    ui.SetDeck(deckList[i], i);
                }
            }
            else
            {
                Debug.LogError("Invalid Data " + response.message);
            }
        }
        else
        {
            Debug.LogError($"Fail: {req.error}");
        }
    }
}