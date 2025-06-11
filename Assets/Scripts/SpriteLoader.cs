using UnityEngine;

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
        //Debug.Log("cardname:" + cardName);
        string resourceKey = LanguageTranslate.GetResourceKey(cardName);
        //Debug.Log("resource key:" + resourceKey);
        GameObject prefab = Resources.Load<GameObject>($"Animals/{resourceKey}");

        if (prefab == null)
        {
            Debug.LogWarning($"[SpriteLoader] Prefab not found for: {cardName} (mapped: {resourceKey})");
        }

        return prefab;
    }
}
