using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Display 3D Model Deck Script
/// </summary>
/// 
public class DeckDisplay : MonoBehaviour
{
    public Transform[] spawnPoints = new Transform[5];

    public void DisplayDeck(List<GameManager.DeckCard> cards)
    {
        ClearSlots();

        for (int i = 0; i < cards.Count && i < spawnPoints.Length; i++)
        {
            string animalName = cards[i].card.name;
            if (spriteNameMap.TryGetValue(animalName, out string prefabName))
            {
                GameObject prefab = Resources.Load<GameObject>($"Animals/{prefabName}");

                if (prefab != null)
                {
                    Vector3 spawnPos = spawnPoints[i].position + new Vector3(0, 0.5f, 0);
                    Quaternion rotation = Quaternion.Euler(0, 180f, 0); // Y축 회전
                    Instantiate(prefab, spawnPos, rotation, spawnPoints[i]);
                }
                else
                {
                    Debug.LogWarning($"{animalName} No Prefab");
                }
            }
        }
    }

    public void ClearSlots()
    {
        foreach (Transform point in spawnPoints)
        {
            for (int i = point.childCount - 1; i >= 0; i--)
            {
                Destroy(point.GetChild(i).gameObject);
            }
        }
    }

    // Sprite Loading
    private Dictionary<string, string> spriteNameMap = new Dictionary<string, string>
    {
        { "개구리", "Frog" },
        { "타조", "Ostrich" },
        { "아프리카 코끼리", "Elephant" },
        { "햄스터", "Hamster" },
        { "목도리 도마뱀", "Geko" },
        { "사자", "Lion" },
        { "흰수염고래", "Whale" },
        { "원숭이", "Monkey" },
        { "말", "Horse" },
        { "호랑이", "Tiger" },
        { "북극곰", "Polarbear" },
        { "문어", "Octopus" },
        { "나무늘보", "Sloth" },
        { "하마", "Hippo" },
        { "기린", "Giraffe" }
    };
}
