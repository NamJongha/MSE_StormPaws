using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    [Header("Managers")]
    public DeckManager deckManager;
    public RecordManager recordManager;

    [Header("Panels")]
    public GameObject infoPanel;
    public GameObject deckPanel;
    public GameObject recordPanel;
    public GameObject deckCreatePanel;

    [Header("Audio")]
    public AudioSource buttonClick;

    // Cache
    private List<DeckPreset> cachedDecks = null;
    private List<BattleRecord> cachedBattles = null;

    // Panel Switch
    private void SwitchToPanel(GameObject targetPanel)
    {
        infoPanel.SetActive(false);
        deckPanel.SetActive(false);
        recordPanel.SetActive(false);
        deckCreatePanel.SetActive(false);

        if (targetPanel != null)
            targetPanel.SetActive(true);
    }

    public void DeckButton()
    {
        if (cachedDecks != null)
        {
            deckManager.DisplayDeckList(cachedDecks);
        }
        else
        {
            GameManager.Instance.DeckService.FetchDeckPresets(deckList =>
            {
                cachedDecks = deckList;
                deckManager.DisplayDeckList(deckList);
            });
        }

        SwitchToPanel(deckPanel);
        buttonClick.Play();
    }

    public void InfoButton()
    {
        SwitchToPanel(infoPanel);
        buttonClick.Play();
    }

    public void deckCreateButton()
    {
        deckCreatePanel.SetActive(true);
        deckManager.InitCardList();
        buttonClick.Play();
    }

    public void recordButton()
    {
        if (cachedBattles != null)
        {
            recordManager.DisplayBattleRecords(cachedBattles);
        }
        else
        {
            GameManager.Instance.BattleService.FetchBattleRecords((battleList) =>
            {
                cachedBattles = battleList;

                recordManager.DisplayBattleRecords(battleList);
            });
        }

        recordPanel.SetActive(true);
        infoPanel.SetActive(false);
        deckPanel.SetActive(false);
        deckCreatePanel.SetActive(false);

        buttonClick.Play();
    }

    public void OnLanguageDropdownChanged(int index)
    {
        LanguageTranslate.CurrentLanguage = (LanguageTranslate.Language)index;

        RefreshAllUI();
    }

    private void RefreshAllUI()
    {
        if (cachedDecks != null)
        {
            deckManager.DisplayDeckList(cachedDecks);
        }

        if (cachedBattles != null)
        {
            recordManager.DisplayBattleRecords(cachedBattles);
        }

        if (deckCreatePanel.activeSelf)
        {
            deckManager.InitCardList();
        }
    }
}