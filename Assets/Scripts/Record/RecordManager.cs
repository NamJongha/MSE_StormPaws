using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Displays battle record list using saved data.
/// </summary>

public class RecordManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject battleRecordPrefab;
    public Transform battleRecordContainer;

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

        int displayIndex = 0;

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];

            if (string.IsNullOrWhiteSpace(record.myDeck))
            {
                Debug.LogWarning($"Skipping record {i} - myDeck is null or empty");
                continue;
            }

            if (string.IsNullOrWhiteSpace(record.opponentDeck))
            {
                Debug.LogWarning($"Skipping record {i} - opponentDeck is null or empty");
                continue;
            }

            string[] myDeckArray = null;

            string backupKey = $"BackupDeck_{record.myDeck}";
            if (PlayerPrefs.HasKey(backupKey))
            {
                string json = PlayerPrefs.GetString(backupKey);
                DeckPreset backup = JsonUtility.FromJson<DeckPreset>(json);

                if (backup != null && backup.decklist != null)
                {
                    myDeckArray = new string[backup.decklist.Count];
                    for (int j = 0; j < backup.decklist.Count; j++)
                    {
                        myDeckArray[j] = backup.decklist[j]?.card?.name ?? "";
                    }
                }
            }
            else if (record.myDeck.Contains(","))
            {
                myDeckArray = record.myDeck.Split(',');
            }
            else
            {
                Debug.LogWarning($"Skipping record {i} - backup key not found and deck is not valid: {record.myDeck}");
                continue;
            }

            GameObject item = Instantiate(battleRecordPrefab, battleRecordContainer);
            BattleRecordUI ui = item.GetComponent<BattleRecordUI>();

            ui.numberText.text = (displayIndex + 1).ToString();
            displayIndex++;

            ui.opponentText.text = record.opponent;
            ui.weatherText.text = record.weather;
            ui.resultText.text = record.result;

            for (int j = 0; j < ui.myDeckSlots.Length; j++)
            {
                var slot = ui.myDeckSlots[j];
                if (slot == null)
                {
                    continue;
                }

                if (j < myDeckArray.Length && !string.IsNullOrWhiteSpace(myDeckArray[j]))
                {
                    string rawName = myDeckArray[j].Trim();
                    string displayName = LanguageTranslate.GetDisplayName(rawName);
                    Sprite sprite = GameManager.Instance.SpriteLoader.Load(rawName);

                    if (slot.nameText != null)
                        slot.nameText.text = !string.IsNullOrEmpty(displayName) ? displayName : "Unknown";

                    if (slot.icon != null)
                    {
                        if (sprite != null)
                        {
                            slot.icon.sprite = sprite;
                            slot.icon.gameObject.SetActive(true);
                        }
                        else
                        {
                            slot.icon.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (slot.nameText != null)
                    {
                        slot.nameText.text = "";
                    }

                    if (slot.icon != null)
                    {
                        slot.icon.gameObject.SetActive(false);
                    }
                }
            }

            string[] opponentDeckArray = record.opponentDeck.Split(',');
            for (int j = 0; j < ui.opponentSlots.Length; j++)
            {
                var slot = ui.opponentSlots[j];
                if (slot == null) continue;

                if (j < opponentDeckArray.Length && !string.IsNullOrWhiteSpace(opponentDeckArray[j]))
                {
                    string rawName = opponentDeckArray[j].Trim();
                    string displayName = LanguageTranslate.GetDisplayName(rawName);
                    Sprite sprite = GameManager.Instance.SpriteLoader.Load(rawName);

                    if (slot.nameText != null)
                    {
                        slot.nameText.text = !string.IsNullOrEmpty(displayName) ? displayName : "Unknown";
                    }

                    if (slot.icon != null)
                    {
                        if (sprite != null)
                        {
                            slot.icon.sprite = sprite;
                            slot.icon.gameObject.SetActive(true);
                        }
                        else
                        {
                            slot.icon.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (slot.nameText != null)
                    {
                        slot.nameText.text = "";
                    }

                    if (slot.icon != null)
                    {
                        slot.icon.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}