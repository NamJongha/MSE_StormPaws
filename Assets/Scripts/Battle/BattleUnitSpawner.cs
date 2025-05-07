using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class BattleUnitSpawner : MonoBehaviour
{
    private GameManager gameManager;

    public Transform[] mySpawnPoints;
    public Transform[] opponentSpawnPoints;

    private Dictionary<string, string> modelMap = new Dictionary<string, string>
    {
        { "������", "frog" },
        { "Ÿ��", "ostrich" },
        { "������ī �ڳ���", "elephant" },
        { "�ܽ���", "hamster" },
        { "�񵵸� ������", "geko" },
        { "����", "lion" },
        { "�������", "whale" },
        { "������", "monkey" },
        { "��", "horse" },
        { "ȣ����", "tiger" },
        { "�ϱذ�", "polarbear" },
        { "����", "octopus" },
        { "�����ú�", "sloth" },
        { "�ϸ�", "hippo" },
        { "�⸰", "giraffe" }
    };

    void Awake()
    {

    }

    void Start()
    {
        StartCoroutine(SpawnAllUnits());
    }

    private IEnumerator SpawnAllUnits()
    {
        gameManager.FetchSelectedMyDeck(myDeck =>
        {
            for (int i = 0; i < myDeck.decklist.Count && i < mySpawnPoints.Length; i++)
            {
                string modelName = GetModelName(myDeck.decklist[i].card.name);
                GameObject prefab = LoadAnimalPrefab(modelName);
                if (prefab != null)
                {
                    Instantiate(prefab, mySpawnPoints[i].position, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning("No Prefab: " + modelName);
                }
            }
        });

        yield return new WaitForSeconds(0.2f);

        //gameManager.FetchSelectedOpponentDeck(opponentDeck =>
        //{
        //    for (int i = 0; i < opponentDeck.decklist.Count && i < opponentSpawnPoints.Length; i++)
        //    {
        //        string modelName = GetModelName(opponentDeck.decklist[i].card.name);
        //        GameObject prefab = LoadAnimalPrefab(modelName);
        //        if (prefab != null)
        //        {
        //            Quaternion rotation = Quaternion.Euler(0, 180f, 0f);
        //            Instantiate(prefab, opponentSpawnPoints[i].position, rotation);
        //        }
        //        else
        //        {
        //            Debug.LogWarning("No Prefab: " + modelName);
        //        }
        //    }
        //});
    }

    private string GetModelName(string cardName)
    {
        if (modelMap.TryGetValue(cardName, out string name))
        {
            return name;
        }
        return null;
    }

    private GameObject LoadAnimalPrefab(string modelName)
    {
        if (string.IsNullOrEmpty(modelName)) return null;
        return Resources.Load<GameObject>($"AnimalModels/{modelName}");
    }
}
