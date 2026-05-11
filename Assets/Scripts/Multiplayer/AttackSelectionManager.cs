using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Spravuje vyber utokov v multiplayerovom rezime
/// </summary>
public class AttackSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text dialogText;
    public Button confirmButton;

    [Header("Attack Buttons")]
    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    [Header("Dependencies")]
    public AttackDescriptions attackDescriptions;
    public FightSystemMultiplayer fightSystem;

    private SharedAttackSelectionFlow selectionFlow;
    private Kard preparedCard;
    private AttackCountsResult preparedAttackCounts;

    void Start()
    {
        // Pripoj listenery na tlacidla utokov
        if (button1 != null) button1.onClick.AddListener(() => OnAttackButtonClicked(1));
        if (button2 != null) button2.onClick.AddListener(() => OnAttackButtonClicked(2));
        if (button3 != null) button3.onClick.AddListener(() => OnAttackButtonClicked(3));
        if (button4 != null) button4.onClick.AddListener(() => OnAttackButtonClicked(4));

        // Pripoj listener na confirm tlacidlo
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmAttackClicked);
        }

        // Zacni s disablovanym confirm tlacidlom
        SetConfirmButtonState(false);

        selectionFlow = new SharedAttackSelectionFlow(
            attackDescriptions,
            dialogText,
            MultiplayerUI.MSG_WAITING_OPPONENT,
            GetPendingOngoingAction,
            SubmitAttackSelection,
            SetAttackButtonsInteractable,
            SetConfirmButtonState
        );
    }

    /// <summary>
    /// Pripravi UI pre vyber utoku
    /// </summary>
    /// <param name="card">Karta, ktora bude utocit</param>
    /// <param name="attackCounts">Pocty utokov pre danu kartu</param>
    public void PrepareAttackSelection(Kard card, AttackCountsResult attackCounts)
    {
        preparedCard = card;
        preparedAttackCounts = attackCounts;
        bool allowButtons = fightSystem != null && fightSystem.state == FightStateMultiplayer.TURN;
        selectionFlow?.PrepareAttackSelection(card, SharedAttackSelectionFlow.AttackCountsState.FromResult(attackCounts), allowButtons);
        Debug.Log("[AttackSelectionManager] Attack selection prepared for card: " + card.cardName);
    }

    /// <summary>
    /// Handler pre kliknutie na tlacidlo utoku
    /// </summary>
    private void OnAttackButtonClicked(int attackType)
    {
        selectionFlow?.OnAttackButtonClicked(attackType);
    }

    /// <summary>
    /// Handler pre potvrdenie vyberu utoku
    /// </summary>
    private void OnConfirmAttackClicked()
    {
        selectionFlow?.OnConfirmAttackClicked();
    }

    /// <summary>
    /// Nastav interaktivitu tlacidiel utokov
    /// </summary>
    private void SetAttackButtonsInteractable(bool button1Enabled, bool button2Enabled, bool button3Enabled, bool button4Enabled)
    {
        if (button1 != null) button1.interactable = button1Enabled;
        if (button2 != null) button2.interactable = button2Enabled;
        if (button3 != null) button3.interactable = button3Enabled;
        if (button4 != null) button4.interactable = button4Enabled;
    }
    
    /// <summary>
    /// Public metoda pre update attack counts (volane po decrement)
    /// </summary>
    public void UpdateAttackCounts(AttackCountsResult newCounts)
    {
        if (newCounts == null)
        {
            Debug.LogWarning("[AttackSelectionManager] UpdateAttackCounts called with null counts");
            return;
        }

        preparedAttackCounts = newCounts;
        bool allowButtons = fightSystem != null && fightSystem.state == FightStateMultiplayer.TURN;
        selectionFlow?.UpdateAttackCounts(SharedAttackSelectionFlow.AttackCountsState.FromResult(newCounts), allowButtons);
        Debug.LogWarning($"[AttackSelectionManager] [OK] Attack counts updated: {newCounts.count1}, {newCounts.count2}, {newCounts.count3}, {newCounts.count4}");
    }

    /// <summary>
    /// Nastav stav confirm tlacidla
    /// </summary>
    private void SetConfirmButtonState(bool enabled)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = enabled;
        }
    }

    /// <summary>
    /// Disable UI po potvrdeni vyberu
    /// </summary>
    private void DisableAttackSelection()
    {
        SetAllAttackButtonsInteractable(false);

        SetConfirmButtonState(false);

        if (dialogText != null)
        {
            dialogText.text = MultiplayerUI.MSG_WAITING_OPPONENT;
        }
    }

    /// <summary>
    /// Enable attack buttony po reveal-nuti oboch kariet (vola sa z RevealCards)
    /// </summary>
    public void EnableAttackButtonsAfterReveal()
    {
        if (preparedCard == null || preparedAttackCounts == null)
        {
            Debug.Log("[AttackSelectionManager] Reveal completed before attack counts were ready; buttons will enable after PrepareAttackSelection");
            return;
        }

        selectionFlow?.PrepareAttackSelection(
            preparedCard,
            SharedAttackSelectionFlow.AttackCountsState.FromResult(preparedAttackCounts),
            allowButtons: true
        );
        Debug.LogWarning("[AttackSelectionManager] [OK] Attack buttons enabled - both cards revealed!");
    }
    
    /// <summary>
    /// Reset selection state pre novy turn
    /// </summary>
    public void ResetSelection()
    {
        preparedCard = null;
        preparedAttackCounts = null;
        selectionFlow?.ResetSelection();
        Debug.Log("[AttackSelectionManager] Selection reset for next turn");
    }
    
    /// <summary>
    /// Nastavi vsetky attack tlacidla na enabled/disabled
    /// </summary>
    private void SetAllAttackButtonsInteractable(bool enabled)
    {
        if (button1 != null) button1.interactable = enabled;
        if (button2 != null) button2.interactable = enabled;
        if (button3 != null) button3.interactable = enabled;
        if (button4 != null) button4.interactable = enabled;
    }

    /// <summary>
    /// Ziskaj aktualne vybrany typ utoku
    /// </summary>
    public int GetSelectedAttackType()
    {
        return selectionFlow != null ? selectionFlow.SelectedAttackType : 0;
    }

    private PendingOngoingActionTurnData GetPendingOngoingAction()
    {
        if (fightSystem == null || !fightSystem.TryGetPendingOngoingActionTurn(out var pendingTurn) || pendingTurn == null)
        {
            return null;
        }

        return new PendingOngoingActionTurnData
        {
            actionType = pendingTurn.actionType,
            sourceAttackId = pendingTurn.sourceAttackId,
            targetCardId = pendingTurn.targetCardId,
            turnsRemaining = pendingTurn.turnsRemaining
        };
    }

    private void SubmitAttackSelection(SelectedAttackData attackData)
    {
        if (fightSystem == null)
        {
            Debug.LogError("[AttackSelectionManager] FightSystemMultiplayer reference missing");
            return;
        }

        fightSystem.OnAttackConfirmed(attackData);
    }
}
