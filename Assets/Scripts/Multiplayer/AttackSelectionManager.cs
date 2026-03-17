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

    // Aktualne vybrany utok
    private int selectedAttackType = 0; // 1-4
    private Kard currentCard = null;
    private AttackCountsResult currentAttackCounts = null;

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
    }

    /// <summary>
    /// Pripravi UI pre vyber utoku
    /// </summary>
    /// <param name="card">Karta, ktora bude utocit</param>
    /// <param name="attackCounts">Pocty utokov pre danu kartu</param>
    public void PrepareAttackSelection(Kard card, AttackCountsResult attackCounts)
    {
        currentCard = card;
        currentAttackCounts = attackCounts;
        selectedAttackType = 0;

        // [OK] NEVYPLNAJ dialogText tu - spravy nastavuju MultiplayerBoardManager a BattleResultProcessor
        // Predchadzajuca sprava je vzdy relevantna ("Choose your attack", "Choose fighter!", atd.)

        // [OK] OCHRANA: Povol buttony IBA ak je stav TURN (obe karty su revealed)
        if (fightSystem != null && fightSystem.state == FightStateMultiplayer.TURN)
        {
            // Nastav interaktivitu tlacidiel podla toho, ci maju count > 0
            SetAttackButtonsInteractable(attackCounts);
            Debug.Log("[AttackSelectionManager] [OK] Attack selection enabled - both cards revealed");
        }
        else
        {
            // Obe karty este nie su revealed - drz buttony disabled
            DisableAttackSelection();
            Debug.LogWarning("[AttackSelectionManager]  Attack buttons disabled - waiting for opponent card reveal");
        }
        
        SetConfirmButtonState(false);

        Debug.Log("[AttackSelectionManager] Attack selection prepared for card: " + card.cardName);
    }

    /// <summary>
    /// Handler pre kliknutie na tlacidlo utoku
    /// </summary>
    private void OnAttackButtonClicked(int attackType)
    {
        if (currentCard == null)
        {
            Debug.LogWarning("[AttackSelectionManager] No card selected");
            return;
        }

        if (!CanSelectAttack(attackType))
        {
            Debug.LogWarning($"[AttackSelectionManager] Cannot select attack {attackType}");
            return;
        }

        // Uloz vybrany utok
        selectedAttackType = attackType;

        // Zobraz popis utoku v dialogu
        if (attackDescriptions != null && dialogText != null)
        {
            attackDescriptions.DisplayAttackMultiplayer(currentCard, attackType, dialogText, GetAttackCount(attackType));
        }

        // Aktivuj confirm tlacidlo
        SetConfirmButtonState(true);

        Debug.Log($"[AttackSelectionManager] Attack {attackType} selected");
    }

    /// <summary>
    /// Handler pre potvrdenie vyberu utoku
    /// </summary>
    private void OnConfirmAttackClicked()
    {
        if (selectedAttackType == 0)
        {
            Debug.LogWarning("[AttackSelectionManager] No attack selected to confirm");
            return;
        }

        if (currentCard == null)
        {
            Debug.LogError("[AttackSelectionManager] Current card is null");
            return;
        }

        Debug.Log($"[AttackSelectionManager] Confirming attack {selectedAttackType}");

        // Tu bude logika pre odoslanie utoku na server
        // TODO: Implement server communication
        ConfirmAttackSelection();
    }

    /// <summary>
    /// Potvrd vyber utoku a priprav ho na odoslanie
    /// </summary>
    private void ConfirmAttackSelection()
    {
        if (fightSystem == null)
        {
            Debug.LogError("[AttackSelectionManager] FightSystemMultiplayer reference missing");
            return;
        }

        // Vytvor data o utoku
        var attackData = new SelectedAttackData
        {
            attackType = selectedAttackType,
            attackId = GetAttackId(selectedAttackType),
            attackCount = GetAttackCount(selectedAttackType),
            cardId = currentCard.cardId
        };

        // Posli data do fight systemu na dalsie spracovanie
        fightSystem.OnAttackConfirmed(attackData);

        // Disable UI po potvrdeni
        DisableAttackSelection();
    }

    /// <summary>
    /// Ziskaj ID utoku pre dany typ (1-4)
    /// </summary>
    private int GetAttackId(int attackType)
    {
        return attackType switch
        {
            1 => currentCard.attack1,
            2 => currentCard.attack2,
            3 => currentCard.attack3,
            4 => currentCard.attack4,
            _ => 0
        };
    }

    /// <summary>
    /// Ziskaj count utoku pre dany typ
    /// </summary>
    private int GetAttackCount(int attackType)
    {
        if (currentAttackCounts == null) return 0;

        return attackType switch
        {
            1 => currentAttackCounts.count1,
            2 => currentAttackCounts.count2,
            3 => currentAttackCounts.count3,
            4 => currentAttackCounts.count4,
            _ => 0
        };
    }

    /// <summary>
    /// Skontroluj, ci je mozne vybrat dany utok
    /// </summary>
    private bool CanSelectAttack(int attackType)
    {
        int attackId = GetAttackId(attackType);
        int attackCount = GetAttackCount(attackType);

        return attackId > 0 && attackCount > 0;
    }

    /// <summary>
    /// Nastav interaktivitu tlacidiel utokov
    /// </summary>
    private void SetAttackButtonsInteractable(AttackCountsResult counts)
    {
        if (button1 != null) button1.interactable = (currentCard.attack1 > 0 && counts.count1 > 0);
        if (button2 != null) button2.interactable = (currentCard.attack2 > 0 && counts.count2 > 0);
        if (button3 != null) button3.interactable = (currentCard.attack3 > 0 && counts.count3 > 0);
        if (button4 != null) button4.interactable = (currentCard.attack4 > 0 && counts.count4 > 0);
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
        
        currentAttackCounts = newCounts;
        
        // Re-enable buttony s novymi counts
        if (currentCard != null && fightSystem != null && fightSystem.state == FightStateMultiplayer.TURN)
        {
            SetAttackButtonsInteractable(newCounts);
            Debug.LogWarning($"[AttackSelectionManager] [OK] Attack counts updated: {newCounts.count1}, {newCounts.count2}, {newCounts.count3}, {newCounts.count4}");
        }
        else
        {
            Debug.LogWarning("[AttackSelectionManager] Attack counts updated but buttons remain disabled (not in TURN state)");
        }
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
        if (button1 != null) button1.interactable = false;
        if (button2 != null) button2.interactable = false;
        if (button3 != null) button3.interactable = false;
        if (button4 != null) button4.interactable = false;

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
        if (currentCard == null || currentAttackCounts == null)
        {
            Debug.LogWarning("[AttackSelectionManager] Cannot enable buttons - no card/counts prepared");
            return;
        }
        
        // Teraz su obe karty revealed - povol attack buttony
        SetAttackButtonsInteractable(currentAttackCounts);
        Debug.LogWarning("[AttackSelectionManager] [OK] Attack buttons enabled - both cards revealed!");
    }
    
    /// <summary>
    /// Reset selection state pre novy turn
    /// </summary>
    public void ResetSelection()
    {
        selectedAttackType = 0;
        currentCard = null;
        currentAttackCounts = null;

        if (dialogText != null)
        {
            dialogText.text = "";
        }

        // [OK] Reset tlacidiel do disabled stavu
        SetConfirmButtonState(false);
        SetAllAttackButtonsInteractable(false);
        
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
        return selectedAttackType;
    }
}
