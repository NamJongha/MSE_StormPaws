using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static GameManager;

public class BattleUnitSpawner : MonoBehaviour
{
    [SerializeField]
    BattleManager battleManager;

    public Transform[] mySpawnPoints;
    public Transform[] opponentSpawnPoints;

    public GameObject hpBarPrefab;
    public Transform canvasTransform;

    private Dictionary<string, string> modelMap = new Dictionary<string, string>
    {
        { "������", "Frog" },
        { "Ÿ��", "Ostrich" },
        { "������ī �ڳ���", "Elephant" },
        { "�ܽ���", "Hamster" },
        { "�񵵸� ������", "Geko" },
        { "����", "Lion" },
        { "����� ��", "Whale" },
        { "������", "Monkey" },
        { "��", "Horse" },
        { "ȣ����", "Tiger" },
        { "�ϱذ�", "Polarbear" },
        { "����", "Octopus" },
        { "�����ú�", "Sloth" },
        { "�ϸ�", "Hippo" },
        { "�⸰", "Giraffe" }
    };

    void Start()
    {
        StartCoroutine(SpawnAllUnits());
    }

    private IEnumerator SpawnAllUnits()
    {
        string myDeckId = PlayerPrefs.GetString("SelectedMyDeckId", "");

        GameManager.Instance.DeckService.FetchDeckById(myDeckId, myDeck =>
        {
            for (int i = 0; i < myDeck.decklist.Count && i < opponentSpawnPoints.Length; i++)
            {
                string modelName = GetModelName(myDeck.decklist[i].card.name);
                GameObject prefab = GameManager.Instance.SpriteLoader.LoadAnimalPrefab(modelName);

                if (prefab != null)
                {
                    // Instantiate unit and rotate it
                    Quaternion rotation = Quaternion.Euler(0, 180f, 0f);
                    GameObject unit = Instantiate(prefab, mySpawnPoints[i].position, rotation);
                    GameManager.Instance.BattleService.SetMyDeck(unit);

                    // Instantiate and assign HP bar
                    GameObject hpBar = Instantiate(hpBarPrefab, canvasTransform);
                    var hpBarScript = hpBar.GetComponent<HPBar>();
                    hpBarScript.SetTarget(unit.transform);
                    GameManager.Instance.BattleService.RegisterHPBar(unit, hpBarScript);
                }
                else
                {
                    Debug.LogWarning("No Prefab: " + modelName);
                }
            }
        });

        yield return new WaitForSeconds(0.2f);

        //njh
        //selected opponent id is saved when the player selected the opponent's deck
        //Get That deck information with the id
        string opponentDeckId = PlayerPrefs.GetString("SelectedOpponentDeckId", "");
        GameManager.Instance.DeckService.FetchDeckById(opponentDeckId, opponentDeck =>
        {
            for (int i = 0; i < opponentDeck.decklist.Count && i < opponentSpawnPoints.Length; i++)
            {
                string modelName = GetModelName(opponentDeck.decklist[i].card.name);
                GameObject prefab = GameManager.Instance.SpriteLoader.LoadAnimalPrefab(modelName);
                if (prefab != null)
                {
                    Quaternion rotation = Quaternion.Euler(0, 180f, 0f);
                    GameObject unit = Instantiate(prefab, opponentSpawnPoints[i].position, rotation);
                    GameManager.Instance.BattleService.SetOpponentDeck(unit);

                    GameObject hpBar = Instantiate(hpBarPrefab, canvasTransform);
                    var hpBarScript = hpBar.GetComponent<HPBar>();
                    hpBarScript.SetTarget(unit.transform);
                    GameManager.Instance.BattleService.RegisterHPBar(unit, hpBarScript);
                }
                else
                {
                    Debug.LogWarning("No Prefab: " + modelName);
                }
            }
        });
    }


    // Helper to convert card name to model prefab name
    private string GetModelName(string cardName)
    {
        if (modelMap.TryGetValue(cardName, out string name))
        {
            return name;
        }
        return null;
    }
}