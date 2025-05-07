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
                    Quaternion rotation = Quaternion.Euler(0, 180f, 0); // Y�� ȸ��
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
        { "������", "Frog" },
        { "Ÿ��", "Ostrich" },
        { "������ī �ڳ���", "Elephant" },
        { "�ܽ���", "Hamster" },
        { "�񵵸� ������", "Geko" },
        { "����", "Lion" },
        { "�������", "Whale" },
        { "������", "Monkey" },
        { "��", "Horse" },
        { "ȣ����", "Tiger" },
        { "�ϱذ�", "Polarbear" },
        { "����", "Octopus" },
        { "�����ú�", "Sloth" },
        { "�ϸ�", "Hippo" },
        { "�⸰", "Giraffe" }
    };
}
