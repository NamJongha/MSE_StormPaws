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
        { "°³±¸¸®", "Frog" }, { "Å¸Á¶", "Ostrich" }, { "¾ÆÇÁ¸®Ä« ÄÚ³¢¸®", "Elephant" },
        { "ÇÜ½ºÅÍ", "Hamster" }, { "¸ñµµ¸® µµ¸¶¹ì", "Geko" }, { "»çÀÚ", "Lion" },
        { "Èò¼ö¿°°í·¡", "Whale" }, { "¿ø¼þÀÌ", "Monkey" }, { "¸»", "Horse" },
        { "È£¶ûÀÌ", "Tiger" }, { "ºÏ±Ø°õ", "Polarbear" }, { "¹®¾î", "Octopus" },
        { "³ª¹«´Ãº¸", "Sloth" }, { "ÇÏ¸¶", "Hippo" }, { "±â¸°", "Giraffe" }
    };

    void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
            Debug.LogError("GameManager not found in scene!");
    }

    void Start()
    {
        StartCoroutine(SpawnAllUnits());
    }

    private IEnumerator SpawnAllUnits()
    {
        bool myDone = false;
        bool opponentDone = false;

        gameManager.FetchSelectedMyDeck(myDeck =>
        {
            if (myDeck == null)
            {
                Debug.LogWarning("myDeck is null");
                myDone = true;
                return;
            }

            if (myDeck.decklist == null || myDeck.decklist.Count == 0)
            {
                Debug.LogWarning("myDeck.decklist is null or empty");
                myDone = true;
                return;
            }

            Debug.Log($"³» µ¦ À¯´Ö °³¼ö: {myDeck.decklist.Count}");

            for (int i = 0; i < myDeck.decklist.Count && i < mySpawnPoints.Length; i++)
            {
                string modelName = GetModelName(myDeck.decklist[i].card.name);
                Debug.Log($"³» À¯´Ö ½ºÆù ½Ãµµ: {modelName}");

                GameObject prefab = LoadAnimalPrefab(modelName);
                if (prefab != null)
                {
                    Vector3 spawnPos = mySpawnPoints[i].position;
                    spawnPos.y += 2f;
                    Instantiate(prefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning("ÇÁ¸®ÆÕ ¾øÀ½: " + modelName);
                }
            }

            myDone = true;
        });

        gameManager.FetchSelectedOpponentDeck(opponentDeck =>
        {
            if (opponentDeck?.decklist == null)
            {
                Debug.LogWarning("Àû µ¦ Á¤º¸ ¾øÀ½");
                opponentDone = true;
                return;
            }

            for (int i = 0; i < opponentDeck.decklist.Count && i < opponentSpawnPoints.Length; i++)
            {
                string modelName = GetModelName(opponentDeck.decklist[i].card.name);
                GameObject prefab = LoadAnimalPrefab(modelName);
                if (prefab != null)
                {
                    Vector3 spawnPos = opponentSpawnPoints[i].position;
                    spawnPos.y += 2f;

                    Instantiate(prefab, spawnPos, Quaternion.Euler(0, 180f, 0f));
                }
                else
                {
                    Debug.LogWarning("Àû À¯´Ö ÇÁ¸®ÆÕ ¾øÀ½: " + modelName);
                }
            }

            opponentDone = true;
        });

        yield return new WaitUntil(() => myDone && opponentDone);
    }

    private string GetModelName(string cardName)
    {
        if (modelMap.TryGetValue(cardName, out string name))
            return name;
        return null;
    }

    private GameObject LoadAnimalPrefab(string modelName)
    {
        if (string.IsNullOrEmpty(modelName)) return null;
        return Resources.Load<GameObject>($"Animals/{modelName}");
    }
}
