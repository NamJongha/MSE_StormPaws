using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectedDeckView : MonoBehaviour
{
    [System.Serializable]
    public class AnimalSlot
    {
        public Image icon;
        public TMP_Text nameText;
    }

    public AnimalSlot[] animalSlots;

    void Start()
    {
        string deckId = GameManager.Instance.GetSelectedOpponentDeckId();
        var deck = GameManager.Instance.GetOpponentDeckList().Find(d => d.id == deckId);

        if (deck == null)
        {
            Debug.LogError("선택된 덱을 찾을 수 없습니다.");
            return;
        }

        for (int i = 0; i < animalSlots.Length; i++)
        {
            if (i < deck.decklist.Count)
            {
                var card = deck.decklist[i].card;
                animalSlots[i].icon.sprite = GameManager.Instance.LoadAnimalSprite(card.name);
                animalSlots[i].icon.color = Color.white;
                animalSlots[i].nameText.text = card.name;
            }
            else
            {
                animalSlots[i].icon.gameObject.SetActive(false);
                animalSlots[i].nameText.text = "";
            }
        }
    }
}
