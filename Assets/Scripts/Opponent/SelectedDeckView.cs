using UnityEngine;

public class SelectedDeckView : MonoBehaviour
{
    [Header("Animal Slots")]
    public AnimalSlotUI[] animalSlots;

    private void Start()
    {
        string deckId = PlayerPrefs.GetString("SelectedOpponentDeckId", "");

        if (string.IsNullOrEmpty(deckId))
        {
            return;
        }

        GameManager.Instance.DeckService.FetchDeckById(deckId, OnDeckLoaded);
    }

    // Callback when deck data is loaded
    private void OnDeckLoaded(DeckPreset deck)
    {
        if (deck == null)
        {
            Debug.LogWarning("Fail");
            return;
        }

        for (int i = 0; i < animalSlots.Length; i++)
        {
            if (i < deck.decklist.Count)
            {
                var card = deck.decklist[i].card;
                Sprite sprite = GameManager.Instance.SpriteLoader.Load(card.name);

                animalSlots[i].icon.sprite = sprite;
                animalSlots[i].icon.color = Color.white;
                animalSlots[i].nameText.text = LanguageTranslate.GetDisplayName(card.name);
                animalSlots[i].icon.gameObject.SetActive(true);
            }
            else
            {
                animalSlots[i].icon.gameObject.SetActive(false);
                animalSlots[i].nameText.text = "";
            }
        }
    }
}
