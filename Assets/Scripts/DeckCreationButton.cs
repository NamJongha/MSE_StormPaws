using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static GameManager;

public class DeckCreationButton : MonoBehaviour
{
    public GameObject createPanel;
    public GameManager gameManager;
    public DeckManager deckManager;

    public Transform deckSlotContainer;
    public GameObject deckSlotPrefab;
    public DeckDisplay deckDisplay;

    private List<DeckPreset> cachedDecks = null;


    private void Start()
    {
        if (cachedDecks != null)
        {
            deckManager.MyDeckSelect(cachedDecks);

            return;
        }

        gameManager.FetchDeckPresets((deckList) =>
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
