using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DeckManager;

/// <summary>
/// Battle Record Slot UI Component
/// </summary>

public class BattleRecordUI : MonoBehaviour
{
    public TMP_Text numberText;
    public TMP_Text opponentText;
    public TMP_Text weatherText;
    public TMP_Text resultText;

    public AnimalSlotUI[] opponentSlots;
    public AnimalSlotUI[] myDeckSlots;

    public Button detailsButton;
}