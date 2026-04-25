using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class MultiplayerBoardManager : MonoBehaviour
{
    [SerializeField] private FightSystemMultiplayer fightSystem;

    [Header("Turn Management")]
    [SerializeField] private float selectedCardPollIntervalSeconds = 1.5f;
    [SerializeField] private int selectedCardPollAttempts = 30;

    private Kard localSelectedCard;
    private MultiplayerCardDrag localSelectedCardDrag;
    private SelectedCardData localSelectedCardData;
    private SelectedCardData opponentSelectedCardData;
    private bool isSubmittingSelection;
    public CancellationTokenSource opponentSelectionCancellation; // [OK] public for BattleResultProcessor reuse
    public bool opponentCardRevealed; // [OK] public for BattleResultProcessor reuse

    public bool IsProcessingSelection => isSubmittingSelection;

    private Player Player => fightSystem != null ? fightSystem.player : null;
    private Player Enemy => fightSystem != null ? fightSystem.enemy : null;
    private GameObject PlayerBoard => fightSystem != null ? fightSystem.playerBoard : null;
    private GameObject EnemyBoard => fightSystem != null ? fightSystem.enemyBoard : null;
    private HealthBar PlayerLifeBar => fightSystem != null ? fightSystem.playerLifeBar : null;
    private HealthBar EnemyLifeBar => fightSystem != null ? fightSystem.enemyLifeBar : null;
    private MultiplayerHandManager HandManager => fightSystem != null ? fightSystem.multiplayerHandManager : null;
    private MultiplayerService MultiplayerService => fightSystem != null ? fightSystem.multiplayerService : null;
    private MultiplayerUI MultiplayerUI => fightSystem != null ? fightSystem.multiplayerUI : null;
    private string RoomCode => fightSystem != null ? fightSystem.roomCode : null;
    private string MyPlayerId => fightSystem != null ? fightSystem.myPlayerId : null;

    private void Awake()
    {
        if (fightSystem == null)
        {
            fightSystem = GetComponent<FightSystemMultiplayer>() ?? GetComponentInParent<FightSystemMultiplayer>();
        }

        if (fightSystem == null)
        {
            Debug.LogError("[MultiplayerBoardManager] FightSystemMultiplayer reference is missing.");
        }
    }

    public void Configure(FightSystemMultiplayer owner)
    {
        fightSystem = owner;
    }

    public async Task HandleCardSelectedAsync(Kard card, MultiplayerCardDrag dragHandler)
    {
        isSubmittingSelection = true;
        localSelectedCard = card;
        localSelectedCardDrag = dragHandler;
        opponentCardRevealed = false;

        try
        {
            if (Player == null)
            {
                Debug.LogError("[FightSystemMultiplayer] Player reference is missing when selecting a card");
                dragHandler?.ResetToOriginalPosition();
                return;
            }

            Player.PlayCard(card, PlayerBoard);
            Player.cardInGame.isDragable = false;
            PlayerLifeBar?.SetBar(Player.cardInGame);
            LockPlayerHand();
            if (fightSystem != null)
            {
                fightSystem.state = FightStateMultiplayer.START;
            }

            var definition = HandManager != null ? HandManager.GetCardDefinition(card.cardId) : null;
            localSelectedCardData = SelectedCardData.FromCard(card, definition, MyPlayerId);

            if (localSelectedCardData == null)
            {
                Debug.LogError("[FightSystemMultiplayer] Failed to create payload for selected card");
                dragHandler?.ResetToOriginalPosition();
                ResetLocalSelectionState(false);
                return;
            }

            if (MultiplayerService != null)
            {
                await MultiplayerService.SubmitSelectedCardAsync(RoomCode, MyPlayerId, localSelectedCardData);
            }

            if (fightSystem != null)
            {
                fightSystem.LoadAttackCounts(card);
            }

            MultiplayerUI?.ShowStatus(MultiplayerUI.MSG_WAITING_OPPONENT);

            await WaitForOpponentSelectionAsync();

            if (opponentSelectedCardData != null)
            {
                RevealCards();
            }
            else
            {
                Debug.LogWarning("[FightSystemMultiplayer] Opponent selection timed out");
                MultiplayerUI?.ShowStatus("Enemy was too scared of you!");
                ResetLocalSelectionState(false);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FightSystemMultiplayer] Error while handling card selection: {ex.Message}");
            dragHandler?.ResetToOriginalPosition();
        }
        finally
        {
            isSubmittingSelection = false;
        }
    }

    // [OK] public for BattleResultProcessor reuse (enemy card replacement after death)
    public async Task WaitForOpponentSelectionAsync()
    {
        opponentSelectedCardData = null;

        opponentSelectionCancellation?.Cancel();
        opponentSelectionCancellation = new CancellationTokenSource();
        var token = opponentSelectionCancellation.Token;

        try
        {
            await WaitForOpponentSelectionViaMatchStateAsync(token);
        }
        finally
        {
            opponentSelectionCancellation?.Dispose();
            opponentSelectionCancellation = null;
        }
    }

    private async Task WaitForOpponentSelectionViaMatchStateAsync(CancellationToken token)
    {
        for (int attempt = 0; attempt < selectedCardPollAttempts; attempt++)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (MultiplayerService == null)
            {
                Debug.LogError("[FightSystemMultiplayer] MultiplayerService missing when waiting for opponent selection");
                return;
            }

            Debug.Log($"[WaitForOpponentSelectionAsync] MatchState poll attempt {attempt + 1}/{selectedCardPollAttempts}");

            MatchStateDto matchState = await MultiplayerService.GetMatchStateAsync(RoomCode, MyPlayerId);
            if (matchState == null)
            {
                await Task.Delay(System.TimeSpan.FromSeconds(selectedCardPollIntervalSeconds), token);
                continue;
            }

            ApplySelectedCardsFromMatchState(matchState);

            if (TryGetOpponentSelectionFromMatchState(matchState, out var opponentCard))
            {
                opponentSelectedCardData = opponentCard;
                Debug.Log($"[FightSystemMultiplayer] Opponent selected replacement card {opponentCard.cardId} via matchState");
                return;
            }

            await Task.Delay(System.TimeSpan.FromSeconds(selectedCardPollIntervalSeconds), token);
        }

        Debug.LogWarning("[WaitForOpponentSelectionAsync] Opponent replacement selection timed out while polling matchState");
    }

    private void ApplySelectedCardsFromMatchState(MatchStateDto matchState)
    {
        if (matchState?.selectedCards == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(MyPlayerId)
            && matchState.selectedCards.TryGetValue(MyPlayerId, out var mine)
            && mine != null)
        {
            localSelectedCardData = mine;
        }
    }

    private bool TryGetOpponentSelectionFromMatchState(MatchStateDto matchState, out SelectedCardData opponentCard)
    {
        opponentCard = null;

        if (matchState?.selectedCards == null || matchState.selectedCards.Count == 0)
        {
            return false;
        }

        string opponentId = !string.IsNullOrEmpty(matchState.opponentPlayerId)
            ? matchState.opponentPlayerId
            : GetOpponentPlayerId(matchState.selectedCards);

        if (string.IsNullOrEmpty(opponentId))
        {
            return false;
        }

        if (matchState.replacementRequiredPlayerIds != null
            && matchState.replacementRequiredPlayerIds.Contains(opponentId))
        {
            return false;
        }

        if (!matchState.selectedCards.TryGetValue(opponentId, out var candidate) || !IsAliveCardSelection(candidate))
        {
            return false;
        }

        opponentCard = candidate;
        return true;
    }

    private static bool IsAliveCardSelection(SelectedCardData cardData)
    {
        return cardData != null && cardData.health > 0;
    }

    private string GetOpponentPlayerId(Dictionary<string, SelectedCardData> selectedCards)
    {
        foreach (var entry in selectedCards)
        {
            if (!string.IsNullOrEmpty(MyPlayerId) && entry.Key != MyPlayerId)
            {
                return entry.Key;
            }
        }

        return string.Empty;
    }

    // [OK] public for BattleResultProcessor reuse (enemy card replacement after death)
    public void RevealCards()
    {
        if (opponentCardRevealed)
        {
            return;
        }

        if (localSelectedCardData == null || opponentSelectedCardData == null)
        {
            Debug.LogWarning("[FightSystemMultiplayer] RevealCards called without both cards present");
            return;
        }

        Debug.Log($"[FightSystemMultiplayer] Revealing opponent card: mine={localSelectedCardData.cardId}, opponent={opponentSelectedCardData.cardId}");

        if (Enemy == null || EnemyBoard == null)
        {
            Debug.LogError("[FightSystemMultiplayer] Enemy references missing, cannot reveal opponent card");
            return;
        }

        if (Enemy.cardInGame != null)
        {
            if (Enemy.cardInGame.gameObject != null)
            {
                Destroy(Enemy.cardInGame.gameObject);
            }
            Enemy.cardInGame = null;
        }

        Enemy.hand.Clear();

        var opponentGeneratedCard = opponentSelectedCardData.ToGeneratedCard();
        var enemyCard = HandManager != null ? HandManager.CreateCardInGame(opponentGeneratedCard, Enemy.gameObject, Enemy, true, false, EnemyBoard) : null;
        if (enemyCard == null)
        {
            Debug.LogError("[FightSystemMultiplayer] Failed to instantiate opponent card");
            return;
        }

        enemyCard.isDragable = false;
        Enemy.PlayCard(enemyCard, EnemyBoard);
        EnemyLifeBar?.SetBar(Enemy.cardInGame);

        MultiplayerUI?.ShowStatus(MultiplayerUI.MSG_CHOOSE_ATTACK);
        opponentCardRevealed = true;

        if (fightSystem != null)
        {
            fightSystem.state = FightStateMultiplayer.TURN;
            
            // [OK] Teraz povol attack buttony - obe karty su revealed
            if (fightSystem.attackSelectionManager != null)
            {
                fightSystem.attackSelectionManager.EnableAttackButtonsAfterReveal();
            }
        }

        // Note: selectedCards remain in room for ongoing battle state synchronization
        // They will be cleared only when the round/battle ends
    }

    private void ResetLocalSelectionState(bool keepCardOnBoard)
    {
        if (!keepCardOnBoard && localSelectedCard != null)
        {
            if (Player != null)
            {
                Vector3 returnPosition = localSelectedCardDrag != null ? localSelectedCardDrag.GetOriginalLocalPosition() : Vector3.zero;
                Player.ReturnCardToHand(localSelectedCard, returnPosition);
                localSelectedCard.isDragable = true;
                UnlockPlayerHand();
            }
            else
            {
                var dragHandler = localSelectedCardDrag;
                dragHandler?.ResetToOriginalPosition();
            }
        }

        localSelectedCardDrag = null;
        if (!keepCardOnBoard)
        {
            localSelectedCard = null;
        }
        localSelectedCardData = null;
        opponentSelectedCardData = null;
        opponentCardRevealed = false;
    }

    public void UnlockPlayerHand()
    {
        SetHandInteractivity(true);
    }

    public void LockPlayerHand()
    {
        SetHandInteractivity(false);
    }

    private void SetHandInteractivity(bool isEnabled)
    {
        if (Player == null)
        {
            return;
        }

        foreach (var kard in Player.hand)
        {
            if (kard == null)
            {
                continue;
            }

            kard.isDragable = isEnabled;
            var dragHandler = kard.GetComponent<MultiplayerCardDrag>();
            if (dragHandler != null)
            {
                dragHandler.enabled = isEnabled;
            }
        }
    }
}
