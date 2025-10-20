using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Spravuje výber útokov v multiplayerovom režime
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

    // Aktuálne vybraný útok
    private int selectedAttackType = 0; // 1-4
    private Kard currentCard = null;
    private AttackCountsResult currentAttackCounts = null;

    void Start()
    {
        // Pripoj listenery na tlačidlá útokov
        if (button1 != null) button1.onClick.AddListener(() => OnAttackButtonClicked(1));
        if (button2 != null) button2.onClick.AddListener(() => OnAttackButtonClicked(2));
        if (button3 != null) button3.onClick.AddListener(() => OnAttackButtonClicked(3));
        if (button4 != null) button4.onClick.AddListener(() => OnAttackButtonClicked(4));

        // Pripoj listener na confirm tlačidlo
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmAttackClicked);
        }

        // Začni s disablovaným confirm tlačidlom
        SetConfirmButtonState(false);
    }

    /// <summary>
    /// Pripraví UI pre výber útoku
    /// </summary>
    /// <param name="card">Karta, ktorá bude útočiť</param>
    /// <param name="attackCounts">Počty útokov pre danú kartu</param>
    public void PrepareAttackSelection(Kard card, AttackCountsResult attackCounts)
    {
        currentCard = card;
        currentAttackCounts = attackCounts;
        selectedAttackType = 0;

        if (dialogText != null)
        {
            dialogText.text = "Choose an attack";
        }

        // Nastav interaktivitu tlačidiel podľa toho, či majú count > 0
        SetAttackButtonsInteractable(attackCounts);
        SetConfirmButtonState(false);

        Debug.Log("[AttackSelectionManager] Attack selection prepared for card: " + card.cardName);
    }

    /// <summary>
    /// Handler pre kliknutie na tlačidlo útoku
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

        // Ulož vybraný útok
        selectedAttackType = attackType;

        // Zobraz popis útoku v dialogu
        if (attackDescriptions != null && dialogText != null)
        {
            attackDescriptions.DisplayAttackMultiplayer(currentCard, attackType, dialogText, GetAttackCount(attackType));
        }

        // Aktivuj confirm tlačidlo
        SetConfirmButtonState(true);

        Debug.Log($"[AttackSelectionManager] Attack {attackType} selected");
    }

    /// <summary>
    /// Handler pre potvrdenie výberu útoku
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

        // Tu bude logika pre odoslanie útoku na server
        // TODO: Implement server communication
        ConfirmAttackSelection();
    }

    /// <summary>
    /// Potvrď výber útoku a priprav ho na odoslanie
    /// </summary>
    private void ConfirmAttackSelection()
    {
        if (fightSystem == null)
        {
            Debug.LogError("[AttackSelectionManager] FightSystemMultiplayer reference missing");
            return;
        }

        // Vytvor dáta o útoku
        var attackData = new SelectedAttackData
        {
            attackType = selectedAttackType,
            attackId = GetAttackId(selectedAttackType),
            attackCount = GetAttackCount(selectedAttackType),
            cardId = currentCard.cardId
        };

        // Pošli dáta do fight systému na ďalšie spracovanie
        fightSystem.OnAttackConfirmed(attackData);

        // Disable UI po potvrdení
        DisableAttackSelection();
    }

    /// <summary>
    /// Získaj ID útoku pre daný typ (1-4)
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
    /// Získaj count útoku pre daný typ
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
    /// Skontroluj, či je možné vybrať daný útok
    /// </summary>
    private bool CanSelectAttack(int attackType)
    {
        int attackId = GetAttackId(attackType);
        int attackCount = GetAttackCount(attackType);

        return attackId > 0 && attackCount > 0;
    }

    /// <summary>
    /// Nastav interaktivitu tlačidiel útokov
    /// </summary>
    private void SetAttackButtonsInteractable(AttackCountsResult counts)
    {
        if (button1 != null) button1.interactable = (currentCard.attack1 > 0 && counts.count1 > 0);
        if (button2 != null) button2.interactable = (currentCard.attack2 > 0 && counts.count2 > 0);
        if (button3 != null) button3.interactable = (currentCard.attack3 > 0 && counts.count3 > 0);
        if (button4 != null) button4.interactable = (currentCard.attack4 > 0 && counts.count4 > 0);
    }

    /// <summary>
    /// Nastav stav confirm tlačidla
    /// </summary>
    private void SetConfirmButtonState(bool enabled)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = enabled;
        }
    }

    /// <summary>
    /// Disable UI po potvrdení výberu
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
            dialogText.text = "Waiting for opponent...";
        }
    }

    /// <summary>
    /// Resetuj výber útoku pre ďalší turn
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

        // ✅ Reset tlačidiel do disabled stavu
        SetConfirmButtonState(false);
        SetAllAttackButtonsInteractable(false);
        
        Debug.Log("[AttackSelectionManager] Selection reset for next turn");
    }
    
    /// <summary>
    /// Nastaví všetky attack tlačidlá na enabled/disabled
    /// </summary>
    private void SetAllAttackButtonsInteractable(bool enabled)
    {
        if (button1 != null) button1.interactable = enabled;
        if (button2 != null) button2.interactable = enabled;
        if (button3 != null) button3.interactable = enabled;
        if (button4 != null) button4.interactable = enabled;
    }

    /// <summary>
    /// Získaj aktuálne vybraný typ útoku
    /// </summary>
    public int GetSelectedAttackType()
    {
        return selectedAttackType;
    }
}
