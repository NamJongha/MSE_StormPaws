using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Battle Record After Battle
/// </summary>

public class BattleResultUI : MonoBehaviour
{
    [Header("UI Text")]
    public TMP_Text resultText;
    public TMP_Text weatherText;
    public TMP_Text cityText;
    public TMP_Text timestampText;
    public TMP_Text myDeckTitle;
    public TMP_Text opponentDeckTitle;

    [Header("Deck Previews")]
    public Transform myDeckContainer;
    public Transform opponentDeckContainer;
    public GameObject cardPreviewSlotPrefab;

    private void Start()
    {
        //StartCoroutine(FetchBattleResult());
    }

    private IEnumerator FetchBattleResult()
    {
        string url = $"{GameManager.Instance.baseUrl}/battle/last-result";
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Authorization", "Bearer " + GameManager.Instance.GetAuthToken());

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            BattleResultWrapper wrapper = JsonUtility.FromJson<BattleResultWrapper>(req.downloadHandler.text);
            if (wrapper != null && wrapper.success && wrapper.data != null)
            {
                SetUI(wrapper.data);
            }
            else
            {
                Debug.LogWarning("Fail");
            }
        }
        else
        {
            Debug.LogError("Fail: " + req.error);
        }
    }

    public void SetUI(BattleResult result)
    {
        resultText.text = result.result == "WIN" ? "WIN!" : "Lose...";
        weatherText.text = $"Weather: {result.weather}";
        cityText.text = $"City: {result.city}";
        timestampText.text = $"Time: {result.timestamp}";

        myDeckTitle.text = "My Deck";
        opponentDeckTitle.text = "Opponent Deck";

        SetDeckPreview(result.myDeckList, myDeckContainer);
        SetDeckPreview(result.opponentDeckList, opponentDeckContainer);
    }

    private void SetDeckPreview(List<DeckCard> deckList, Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        foreach (DeckCard deckCard in deckList)
        {
            GameObject slot = Instantiate(cardPreviewSlotPrefab, container);
            Image icon = slot.transform.Find("Icon").GetComponent<Image>();
            TMP_Text name = slot.transform.Find("Name").GetComponent<TMP_Text>();

            string cardName = deckCard.card.name;
            icon.sprite = GameManager.Instance.SpriteLoader.Load(cardName);
            name.text = cardName;
        }
    }
}

[System.Serializable]
public class BattleResultWrapper
{
    public bool success;
    public string message;
    public BattleResult data;
}

[System.Serializable]
public class BattleResult
{
    public string result;
    public string battleId;
    public string weather;
    public string city;
    public string timestamp;
    public List<DeckCard> myDeckList;
    public List<DeckCard> opponentDeckList;
}