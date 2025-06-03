using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Battle Record Script
/// </summary>

public class RecordManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject battleRecordPrefab;
    public Transform battleRecordContainer;
    public GameObject detailPanel;
    public TMP_Text detailText;

    [System.Serializable]
    public class BattleDetailDTO
    {
        public string battleId;
        public string date;
        public string result;
        public List<BattleRoundDTO> rounds;
    }

    [System.Serializable]
    public class BattleRoundDTO
    {
        public int round;
        public string action;
        public int damage;
    }

    public void ShowBattleDetail(string battleId)
    {
        StartCoroutine(FetchBattleDetail(battleId));
    }

    private IEnumerator FetchBattleDetail(string battleId)
    {
        string url = $"{GameManager.Instance.baseUrl}/battle/detail/{battleId}";

        UnityWebRequest req = UnityWebRequest.Get(url);
        TokenManager.SendServerToken(req);
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            BattleDetailDTO detail = JsonUtility.FromJson<BattleDetailDTO>(req.downloadHandler.text);
            detailText.text = FormatBattleDetail(detail);
            detailPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Fail: " + req.error);
        }
    }

    // for Detail Screen
    private string FormatBattleDetail(BattleDetailDTO detail)
    {
        string formatted = $"Battle ID: {detail.battleId}\nDate: {detail.date}\nResult: {detail.result}\n\n[Round]\n";
        foreach (var round in detail.rounds)
        {
            formatted += $" - {round.round}R: {round.action} (Damage: {round.damage})\n";
        }
        return formatted;
    }

    public void DisplayBattleRecords(List<BattleRecord> records)
    {
        foreach (Transform child in battleRecordContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            GameObject item = Instantiate(battleRecordPrefab, battleRecordContainer);
            BattleRecordUI ui = item.GetComponent<BattleRecordUI>();

            ui.numberText.text = (i + 1).ToString();
            ui.opponentText.text = record.opponent;
            ui.weatherText.text = record.weather;
            ui.resultText.text = record.result;

            string[] myDeck = record.myDeck.Split(',');

            for (int j = 0; j < ui.myDeckSlots.Length; j++)
            {
                if (j < myDeck.Length)
                {
                    string animalName = myDeck[j];
                    ui.myDeckSlots[j].icon.sprite = GameManager.Instance.SpriteLoader.Load(animalName);
                    ui.myDeckSlots[j].icon.gameObject.SetActive(true);
                }
                else
                {
                    ui.myDeckSlots[j].icon.gameObject.SetActive(false);
                }
            }

            string[] opponentDeck = record.opponentDeck.Split(',');

            for (int j = 0; j < ui.opponentSlots.Length; j++)
            {
                if (j < opponentDeck.Length)
                {
                    string animalName = opponentDeck[j];
                    ui.myDeckSlots[j].icon.sprite = GameManager.Instance.SpriteLoader.Load(animalName);
                    ui.opponentSlots[j].icon.gameObject.SetActive(true);
                }
                else
                {
                    ui.opponentSlots[j].icon.gameObject.SetActive(false);
                }
            }


            string capturedBattleId = record.battleId;
            ui.detailsButton.onClick.RemoveAllListeners();
            ui.detailsButton.onClick.AddListener(() => ShowBattleDetail(capturedBattleId));
        }
    }
}
