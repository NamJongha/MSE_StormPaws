using System.Collections.Generic;
using UnityEngine;

public class SpriteLoader
{
    // Have to remove
    private readonly Dictionary<string, string> nameMap = new()
    {
        { "������", "Frog" }, { "Ÿ��", "Ostrich" }, { "������ī �ڳ���", "Elephant" },
        { "�ܽ���", "Hamster" }, { "�񵵸� ������", "Geko" }, { "����", "Lion" },
        { "�������", "Whale" }, { "������", "Monkey" }, { "��", "Horse" },
        { "ȣ����", "Tiger" }, { "�ϱذ�", "Polarbear" }, { "����", "Octopus" },
        { "�����ú�", "Sloth" }, { "�ϸ�", "Hippo" }, { "�⸰", "Giraffe" }
    };

    public Sprite Load(string cardName)
    {
        if (nameMap.TryGetValue(cardName, out var spriteName))
        {
            return Resources.Load<Sprite>($"Animals/{spriteName}");
        }

        Debug.LogWarning($"Sprite not found: {cardName}");

        return null;
    }

    public GameObject LoadAnimalPrefab(string cardName)
    {
        if (nameMap.TryGetValue(cardName, out var prefabName))
        {
            return Resources.Load<GameObject>($"Animals/{prefabName}");
        }

        Debug.LogWarning($"Prefab not found: {cardName}");

        return null;
    }
}

