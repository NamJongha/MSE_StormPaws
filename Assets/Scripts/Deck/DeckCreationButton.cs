using System.Collections.Generic;
using UnityEngine;

public class DeckCreationButton : MonoBehaviour
{
    [Header("UI References")]
    public GameObject createPanel;
    public DeckManager deckManager;

    [Header("Deck Slot")]
    public Transform deckSlotContainer;
    public GameObject deckSlotPrefab;
    public DeckDisplay deckDisplay;

    private List<DeckPreset> cachedDecks = null;

    private void Start()
    {
        LoadDeckPresets();
    }

    private void LoadDeckPresets()
    {
        if (cachedDecks != null)
        {
            deckManager.MyDeckSelect(cachedDecks);
            return;
        }

        GameManager.Instance.DeckService.FetchDeckPresets((deckList) =>
        {
            cachedDecks = deckList;
            deckManager.MyDeckSelect(deckList);
        });
    }

    public void createButton()
    {
        createPanel.SetActive(true);
        deckManager.InitCardList();
    }
}
