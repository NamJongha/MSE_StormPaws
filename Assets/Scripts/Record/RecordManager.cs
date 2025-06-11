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

    private void Start()
    {
        GameManager.Instance.BattleService.FetchBattleRecords((records) =>
        {
            DisplayBattleRecords(records);
        });
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
                if (j < myDeck.Length && !string.IsNullOrWhiteSpace(myDeck[j]))
                {
                    string animalName = LanguageTranslate.GetDisplayName(myDeck[j].Trim());
                    var sprite = GameManager.Instance.SpriteLoader.Load(animalName);

                    ui.myDeckSlots[j].icon.sprite = sprite;
                    ui.myDeckSlots[j].icon.gameObject.SetActive(true);
                }
            }

            string[] opponentDeck = record.opponentDeck.Split(',');

            for (int j = 0; j < ui.opponentSlots.Length; j++)
            {
                if (j < opponentDeck.Length && !string.IsNullOrWhiteSpace(opponentDeck[j]))
                {
                    string animalName = LanguageTranslate.GetDisplayName(opponentDeck[j].Trim());
                    var sprite = GameManager.Instance.SpriteLoader.Load(animalName);

                    ui.opponentSlots[j].icon.sprite = sprite;
                    ui.opponentSlots[j].icon.gameObject.SetActive(sprite);
                }
            }

            string capturedBattleId = record.battleId;
        }
    }
}