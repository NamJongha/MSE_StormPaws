using UnityEngine;

/// <summary>
/// Loads animal sprites and prefabs from Resources based on card name.
/// </summary>

public class SpriteLoader
{
    public Sprite Load(string cardName)
    {
        string resourceKey = LanguageTranslate.GetResourceKey(cardName);
        Sprite sprite = Resources.Load<Sprite>($"Animals/{resourceKey}");

        if (sprite == null)
        {
            Debug.LogWarning($"[SpriteLoader] Sprite not found for: {cardName} (mapped: {resourceKey})");
        }

        return sprite;
    }

    public GameObject LoadAnimalPrefab(string cardName)
    {
        string resourceKey = LanguageTranslate.GetResourceKey(cardName);
        GameObject prefab = Resources.Load<GameObject>($"Animals/{resourceKey}");

        if (prefab == null)
        {
            Debug.LogWarning($"[SpriteLoader] Prefab not found for: {cardName} (mapped: {resourceKey})");
        }

        return prefab;
    }
}
