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
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Animals/{animalName}");

            if (prefab != null)
            {
                Instantiate(prefab, spawnPoints[i].position, Quaternion.identity, spawnPoints[i]);
            }
            else
            {
                Debug.LogWarning($"{animalName} No Prefab");
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
}
