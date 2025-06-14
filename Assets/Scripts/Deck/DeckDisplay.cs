using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays a deck of animal units at predefined spawn points.
/// </summary>

public class DeckDisplay : MonoBehaviour
{
    // 5 spawn points to show up to 5 units
    public Transform[] spawnPoints = new Transform[5];

    public void DisplayDeck(List<DeckCard> cards)
    {
        ClearSlots();

        for (int i = 0; i < cards.Count && i < spawnPoints.Length; i++)
        {
            string animalName = cards[i].card.name;

            GameObject prefab = GameManager.Instance.SpriteLoader.LoadAnimalPrefab(animalName);
            if (prefab != null)
            {
                Vector3 spawnPos = spawnPoints[i].position + new Vector3(0, 0.5f, 0);
                Quaternion rotation = Quaternion.Euler(0, 180f, 0);
                Instantiate(prefab, spawnPos, rotation, spawnPoints[i]);
            }
            else
            {
                Debug.LogWarning($"{animalName} No Prefab Found");
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
