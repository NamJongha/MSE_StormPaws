using System.Collections.Generic;
using UnityEngine;

public class SpriteLoader
{
    // Have to remove
    private readonly Dictionary<string, string> nameMap = new()
    {
        { "°³±¸¸®", "Frog" }, { "Å¸Á¶", "Ostrich" }, { "¾ÆÇÁ¸®Ä« ÄÚ³¢¸®", "Elephant" },
        { "ÇÜ½ºÅÍ", "Hamster" }, { "¸ñµµ¸® µµ¸¶¹ì", "Geko" }, { "»çÀÚ", "Lion" },
        { "Èò¼ö¿°°í·¡", "Whale" }, { "¿ø¼þÀÌ", "Monkey" }, { "¸»", "Horse" },
        { "È£¶ûÀÌ", "Tiger" }, { "ºÏ±Ø°õ", "Polarbear" }, { "¹®¾î", "Octopus" },
        { "³ª¹«´Ãº¸", "Sloth" }, { "ÇÏ¸¶", "Hippo" }, { "±â¸°", "Giraffe" }
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

