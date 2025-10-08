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
    private CancellationTokenSource opponentSelectionCancellation;
    private bool opponentCardRevealed;

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
            MultiplayerUI?.ShowStatus("Čaká sa na súpera...");

            await WaitForOpponentSelectionAsync();

            if (opponentSelectedCardData != null)
            {
                await RevealCardsAsync();
            }
            else
            {
                Debug.LogWarning("[FightSystemMultiplayer] Opponent selection timed out");
                MultiplayerUI?.ShowStatus("Súper nevybral kartu včas");
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

    private async Task WaitForOpponentSelectionAsync()
    {
        opponentSelectedCardData = null;

        opponentSelectionCancellation?.Cancel();
        opponentSelectionCancellation = new CancellationTokenSource();
        var token = opponentSelectionCancellation.Token;

        for (int attempt = 0; attempt < selectedCardPollAttempts; attempt++)
        {
            if (token.IsCancellationRequested)
            {
                opponentSelectionCancellation?.Dispose();
                opponentSelectionCancellation = null;
                return;
            }

            if (MultiplayerService == null)
            {
                Debug.LogError("[FightSystemMultiplayer] MultiplayerService missing when waiting for opponent selection");
                return;
            }

            var selectedCards = await MultiplayerService.GetSelectedCardsAsync(RoomCode);
            if (selectedCards != null)
            {
                if (!string.IsNullOrEmpty(MyPlayerId) && selectedCards.TryGetValue(MyPlayerId, out var mine) && mine != null)
                {
                    localSelectedCardData = mine;
                }

                var opponentId = GetOpponentPlayerId(selectedCards);
                if (!string.IsNullOrEmpty(opponentId) && selectedCards.TryGetValue(opponentId, out var opponentCard) && opponentCard != null)
                {
                    opponentSelectedCardData = opponentCard;
                    Debug.Log($"[FightSystemMultiplayer] Opponent selected card {opponentCard.cardId}");
                    opponentSelectionCancellation?.Cancel();
                    opponentSelectionCancellation?.Dispose();
                    opponentSelectionCancellation = null;
                    return;
                }
            }

            await Task.Delay(System.TimeSpan.FromSeconds(selectedCardPollIntervalSeconds));
        }

        opponentSelectionCancellation?.Dispose();
        opponentSelectionCancellation = null;
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

    private async Task RevealCardsAsync()
    {
        if (opponentCardRevealed)
        {
            return;
        }

        if (localSelectedCardData == null || opponentSelectedCardData == null)
        {
            Debug.LogWarning("[FightSystemMultiplayer] RevealCardsAsync called without both cards present");
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

        MultiplayerUI?.ShowStatus("Obaja hráči sú pripravení – zvoľ útok");
        opponentCardRevealed = true;

        if (MultiplayerService != null)
        {
            await MultiplayerService.ClearSelectedCardsAsync(RoomCode);
        }

        if (fightSystem != null)
        {
            fightSystem.state = FightStateMultiplayer.TURN;
        }
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
}
