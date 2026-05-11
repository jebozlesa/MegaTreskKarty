using System;
using TMPro;

public class SharedAttackSelectionFlow
{
    [Serializable]
    public class AttackCountsState
    {
        public int count1;
        public int count2;
        public int count3;
        public int count4;

        public static AttackCountsState FromResult(AttackCountsResult counts)
        {
            if (counts == null)
            {
                return null;
            }

            return new AttackCountsState
            {
                count1 = counts.count1,
                count2 = counts.count2,
                count3 = counts.count3,
                count4 = counts.count4,
            };
        }
    }

    private readonly AttackDescriptions attackDescriptions;
    private readonly TMP_Text dialogText;
    private readonly string waitingMessage;
    private readonly Func<PendingOngoingActionTurnData> pendingTurnProvider;
    private readonly Action<SelectedAttackData> submitAttackAction;
    private readonly Action<bool, bool, bool, bool> setAttackButtonsInteractable;
    private readonly Action<bool> setConfirmButtonState;

    private int selectedAttackType;
    private Kard currentCard;
    private AttackCountsState currentAttackCounts;

    public SharedAttackSelectionFlow(
        AttackDescriptions attackDescriptions,
        TMP_Text dialogText,
        string waitingMessage,
        Func<PendingOngoingActionTurnData> pendingTurnProvider,
        Action<SelectedAttackData> submitAttackAction,
        Action<bool, bool, bool, bool> setAttackButtonsInteractable,
        Action<bool> setConfirmButtonState)
    {
        this.attackDescriptions = attackDescriptions;
        this.dialogText = dialogText;
        this.waitingMessage = waitingMessage ?? string.Empty;
        this.pendingTurnProvider = pendingTurnProvider;
        this.submitAttackAction = submitAttackAction;
        this.setAttackButtonsInteractable = setAttackButtonsInteractable;
        this.setConfirmButtonState = setConfirmButtonState;
    }

    public int SelectedAttackType => selectedAttackType;

    public void PrepareAttackSelection(Kard card, AttackCountsState attackCounts, bool allowButtons)
    {
        bool cardChanged = currentCard != card;
        currentCard = card;
        currentAttackCounts = attackCounts;

        if (cardChanged)
        {
            selectedAttackType = 0;
        }

        if (TryHandlePendingOngoingActionTurn())
        {
            return;
        }

        if (allowButtons)
        {
            ApplyAttackButtonStates();
        }
        else
        {
            SetAllAttackButtonsInteractable(false);
        }

        if (cardChanged)
        {
            SetConfirmButtonState(false);
        }
    }

    public void OnAttackButtonClicked(int attackType)
    {
        if (currentCard == null)
        {
            return;
        }

        if (!CanSelectAttack(attackType))
        {
            selectedAttackType = 0;
            SetConfirmButtonState(false);
            return;
        }

        selectedAttackType = attackType;
        int attackCount = GetAttackCount(attackType);

        if (attackDescriptions != null && dialogText != null)
        {
            attackDescriptions.DisplayAttackMultiplayer(currentCard, attackType, dialogText, attackCount);
        }

        SetConfirmButtonState(true);
    }

    public void OnConfirmAttackClicked()
    {
        if (selectedAttackType == 0 || currentCard == null)
        {
            return;
        }

        ConfirmAttackSelection();
    }

    public void UpdateAttackCounts(AttackCountsState attackCounts, bool allowButtons)
    {
        currentAttackCounts = attackCounts;

        if (currentCard == null)
        {
            return;
        }

        if (TryHandlePendingOngoingActionTurn())
        {
            return;
        }

        if (allowButtons)
        {
            ApplyAttackButtonStates();
        }
        else
        {
            SetAllAttackButtonsInteractable(false);
            SetConfirmButtonState(false);
        }
    }

    public void ResetSelection(bool clearDialog = true)
    {
        selectedAttackType = 0;
        currentCard = null;
        currentAttackCounts = null;
        SetConfirmButtonState(false);
        SetAllAttackButtonsInteractable(false);

        if (clearDialog && dialogText != null)
        {
            dialogText.text = string.Empty;
        }
    }

    private bool TryHandlePendingOngoingActionTurn()
    {
        if (currentCard == null || pendingTurnProvider == null)
        {
            return false;
        }

        PendingOngoingActionTurnData pendingTurn = pendingTurnProvider();
        if (pendingTurn == null)
        {
            return false;
        }

        int attackType = ResolveAttackTypeForAttackId(currentCard, pendingTurn.sourceAttackId);
        if (attackType <= 0)
        {
            return false;
        }

        selectedAttackType = attackType;
        SetAllAttackButtonsInteractable(false);
        SetConfirmButtonState(false);

        if (dialogText != null)
        {
            dialogText.text = $"{GetOngoingActionLabel(pendingTurn.actionType)} continues automatically...";
        }

        ConfirmAttackSelection();
        return true;
    }

    private void ConfirmAttackSelection()
    {
        if (currentCard == null || submitAttackAction == null || selectedAttackType <= 0)
        {
            return;
        }

        var attackData = new SelectedAttackData
        {
            attackType = selectedAttackType,
            attackId = GetAttackId(selectedAttackType),
            attackCount = GetAttackCount(selectedAttackType),
            cardId = currentCard.cardId
        };

        submitAttackAction(attackData);
        DisableAttackSelection();
    }

    private void DisableAttackSelection()
    {
        SetAllAttackButtonsInteractable(false);
        SetConfirmButtonState(false);

        if (dialogText != null && !string.IsNullOrWhiteSpace(waitingMessage))
        {
            dialogText.text = waitingMessage;
        }
    }

    private void ApplyAttackButtonStates()
    {
        if (currentCard == null || currentAttackCounts == null)
        {
            SetAllAttackButtonsInteractable(false);
            return;
        }

        setAttackButtonsInteractable?.Invoke(
            currentCard.attack1 > 0 && currentAttackCounts.count1 > 0,
            currentCard.attack2 > 0 && currentAttackCounts.count2 > 0,
            currentCard.attack3 > 0 && currentAttackCounts.count3 > 0,
            currentCard.attack4 > 0 && currentAttackCounts.count4 > 0
        );
    }

    private void SetAllAttackButtonsInteractable(bool enabled)
    {
        setAttackButtonsInteractable?.Invoke(enabled, enabled, enabled, enabled);
    }

    private void SetConfirmButtonState(bool enabled)
    {
        setConfirmButtonState?.Invoke(enabled);
    }

    private bool CanSelectAttack(int attackType)
    {
        int attackId = GetAttackId(attackType);
        int attackCount = GetAttackCount(attackType);
        return attackId > 0 && attackCount > 0;
    }

    private int GetAttackId(int attackType)
    {
        if (currentCard == null)
        {
            return 0;
        }

        return attackType switch
        {
            1 => currentCard.attack1,
            2 => currentCard.attack2,
            3 => currentCard.attack3,
            4 => currentCard.attack4,
            _ => 0
        };
    }

    private int GetAttackCount(int attackType)
    {
        if (currentAttackCounts == null)
        {
            return 0;
        }

        return attackType switch
        {
            1 => currentAttackCounts.count1,
            2 => currentAttackCounts.count2,
            3 => currentAttackCounts.count3,
            4 => currentAttackCounts.count4,
            _ => 0
        };
    }

    private static int ResolveAttackTypeForAttackId(Kard card, int attackId)
    {
        if (card == null || attackId <= 0)
        {
            return 0;
        }

        if (card.attack1 == attackId) return 1;
        if (card.attack2 == attackId) return 2;
        if (card.attack3 == attackId) return 3;
        if (card.attack4 == attackId) return 4;
        return 0;
    }

    private static string GetOngoingActionLabel(string actionType)
    {
        if (string.IsNullOrEmpty(actionType))
        {
            return "Action";
        }

        return actionType.ToLowerInvariant() switch
        {
            "siege" => "Siege",
            "doubleenvelopment" => "Double Envelopment",
            "artinspiration" => "Art Inspiration",
            "autoportrait" => "Autoportrait",
            "buffalohorns" => "Buffalo Horns",
            "flintlockpistol" => "Flintlock Pistol",
            "trident" => "Retiarius",
            _ => char.ToUpperInvariant(actionType[0]) + actionType.Substring(1)
        };
    }
}
