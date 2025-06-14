using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the deck creation button and loading existing deck presets.
/// </summary>

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

    // Loads saved deck presets from the server (or cache).
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

    // Called when the Create button is pressed.
    public void createButton()
    {
        createPanel.SetActive(true);
        deckManager.InitCardList();
    }
}
