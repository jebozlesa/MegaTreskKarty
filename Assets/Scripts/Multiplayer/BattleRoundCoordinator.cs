using System.Collections;
using System.Collections.Generic;
using TMPro;
using PlayFab;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BattleRoundCoordinator
{
    private const string MatchPhaseSelectingAttacks = "selecting_attacks";
    private const string MatchPhaseWaitingForAttacks = "waiting_for_attacks";
    private const string MatchPhaseResolvingBattle = "resolving_battle";
    private const string MatchPhaseWaitingForResultAck = "waiting_for_result_ack";
    private readonly FightSystemMultiplayer fightSystem;
    private readonly MultiplayerService multiplayerService;
    private readonly MultiplayerKillCounterManager killCounterManager;
    private readonly TMP_Text dialogText;

    public BattleRoundCoordinator(
        FightSystemMultiplayer fightSystem,
        MultiplayerService multiplayerService,
        MultiplayerKillCounterManager killCounterManager,
        TMP_Text dialogText)
    {
        this.fightSystem = fightSystem;
        this.multiplayerService = multiplayerService;
        this.killCounterManager = killCounterManager;
        this.dialogText = dialogText;
    }

    public IEnumerator PrepareNextTurn()
    {
        yield return MarkReadyForNextTurn();
        ResetAttackSelectionUI();
        if (dialogText != null)
        {
            dialogText.text = MultiplayerUI.MSG_CHOOSE_ATTACK;
        }
    }

    public IEnumerator MarkReadyForNextTurn()
    {
        var serverFunctions = fightSystem.serverFunctionsManager;
        if (serverFunctions == null)
        {
            Debug.LogError("[BattleRoundCoordinator] ServerFunctionsManager not found!");
            yield break;
        }

        Debug.Log("[BattleRoundCoordinator] Marking ready for next turn...");

        bool isCompleted = false;
        bool bothReady = false;

        serverFunctions.MarkReadyForNextTurn(fightSystem.roomCode, fightSystem.myPlayerId, result => {
            if (result?.FunctionResult != null)
            {
                var resultData = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
                    .DeserializeObject<Dictionary<string, object>>(result.FunctionResult.ToString());

                if (resultData.ContainsKey("bothPlayersReady"))
                {
                    bothReady = (bool)resultData["bothPlayersReady"];
                    Debug.Log($"[BattleRoundCoordinator] Ready check result: bothReady={bothReady}");
                }
            }
            isCompleted = true;
        });

        yield return new WaitUntil(() => isCompleted);

        if (bothReady)
        {
            Debug.Log("[BattleRoundCoordinator] Both players ready immediately - no polling needed!");
            yield break;
        }

        if (dialogText != null)
        {
            dialogText.text = "Waiting for opponent to be ready...";
        }

        yield return PollForNextTurnReady();
    }

    public IEnumerator PollForNextTurnReady()
    {
        var serverFunctions = fightSystem.serverFunctionsManager;
        yield return PollForNextTurnReadyViaMatchState(serverFunctions);
    }

    private IEnumerator PollForNextTurnReadyViaMatchState(ServerFunctionsManager serverFunctions)
    {
        int pollAttempts = 0;
        const int MAX_POLL_ATTEMPTS = 30;
        const int RETRY_MARK_READY_AFTER_POLLS = 3;

        while (pollAttempts < MAX_POLL_ATTEMPTS)
        {
            yield return new WaitForSeconds(1f);
            pollAttempts++;

            var matchStateTask = multiplayerService.GetMatchStateAsync(fightSystem.roomCode, fightSystem.myPlayerId);
            yield return new WaitUntil(() => matchStateTask.IsCompleted);

            MatchStateDto matchState = null;
            if (matchStateTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
            {
                matchState = matchStateTask.Result;
            }

            if (matchState == null)
            {
                Debug.LogWarning($"[BattleRoundCoordinator] MatchState next-turn poll {pollAttempts}/{MAX_POLL_ATTEMPTS} returned null");
                continue;
            }

            bool bothReady = IsBothPlayersReadyFromMatchState(matchState);
            bool iAmMarkedReady = IsPlayerMarkedReadyInMatchState(matchState, fightSystem.myPlayerId);

            Debug.Log($"[BattleRoundCoordinator] MatchState next-turn poll {pollAttempts}/{MAX_POLL_ATTEMPTS}: phase={matchState.phase}, bothReady={bothReady}, iAmMarkedReady={iAmMarkedReady}");

            if (bothReady)
            {
                Debug.Log("[BattleRoundCoordinator] Both players ready after MatchState polling!");
                yield break;
            }

            if (HasNextTurnReadyPhaseAdvanced(matchState))
            {
                Debug.Log($"[BattleRoundCoordinator] Next turn already advanced to phase '{matchState.phase}', stopping ready polling");
                yield break;
            }

            if (!iAmMarkedReady
                && IsWaitingForResultAcknowledgement(matchState)
                && pollAttempts % RETRY_MARK_READY_AFTER_POLLS == 0)
            {
                Debug.LogWarning($"[BattleRoundCoordinator] Retrying MarkReadyForNextTurn via MatchState flow (attempt {pollAttempts / RETRY_MARK_READY_AFTER_POLLS})");

                bool retryCompleted = false;
                serverFunctions.MarkReadyForNextTurn(fightSystem.roomCode, fightSystem.myPlayerId, result => {
                    if (result?.FunctionResult != null)
                    {
                        Debug.Log($"[BattleRoundCoordinator] Retry result: {result.FunctionResult}");
                    }
                    retryCompleted = true;
                });

                yield return new WaitUntil(() => retryCompleted);
            }
        }

        Debug.LogError("[BattleRoundCoordinator] Timeout waiting for opponent to be ready");
        if (dialogText != null)
        {
            dialogText.text = "Opponent disconnected?";
        }
    }

    public void ResetAttackSelectionUI()
    {
        var attackSelectionManager = fightSystem.attackSelectionManager;
        if (attackSelectionManager != null)
        {
            attackSelectionManager.ResetSelection();
            Debug.Log("[BattleRoundCoordinator] Attack selection UI reset for next turn");
        }
        else
        {
            Debug.LogWarning("[BattleRoundCoordinator] AttackSelectionManager not found!");
        }

        Kard myCard = fightSystem.player?.cardInGame;
        if (myCard != null && fightSystem.attackCountLoader != null)
        {
            fightSystem.LoadAttackCounts(myCard);
        }
    }

    public IEnumerator HandleCardDeath(Kard deadCard, bool isMyCard)
    {
        if (deadCard == null)
        {
            Debug.LogWarning("[BattleRoundCoordinator] HandleCardDeath called with null card!");
            yield break;
        }

        string cardId = deadCard.cardId;
        string cardName = deadCard.cardName;

        Debug.Log($"[BattleRoundCoordinator] Card died: {cardName} (ID: {cardId}, isMyCard: {isMyCard})");

        yield return new WaitForSeconds(1f);

        Player owner = isMyCard ? fightSystem.player : fightSystem.enemy;
        if (owner != null)
        {
            Debug.Log($"[BattleRoundCoordinator] Removing {cardName} from {(isMyCard ? "player" : "enemy")} board");
            owner.RemoveCardFromBoard(deadCard);
        }
        else
        {
            Debug.LogWarning($"[BattleRoundCoordinator] Owner not found for card {cardName}!");
            Object.Destroy(deadCard.gameObject);
        }

        Debug.Log($"[BattleRoundCoordinator] Skipping explicit dead-card clear for {cardName} (ID: {cardId}); replacement flow now relies on matchState dead-card inference");

        Debug.Log($"[BattleRoundCoordinator] Card death handling complete for {cardName}");
    }

    public IEnumerator HandlePlayerCardDeath(Kard myCard)
    {
        if (killCounterManager != null)
        {
            Debug.Log("[BattleRoundCoordinator] Player card died - incrementing enemy kill count");
            killCounterManager.OnEnemyKilledPlayerCard();
        }
        else
        {
            Debug.LogWarning("[BattleRoundCoordinator] KillCounterManager not assigned!");
        }

        yield return HandleCardDeath(myCard, isMyCard: true);

        Player player = fightSystem?.player;
        if (player == null)
        {
            Debug.LogError("[BattleRoundCoordinator] Player reference is null!");
            fightSystem.state = FightStateMultiplayer.LOST;
            yield break;
        }

        int remainingCards = player.hand.Count;
        Debug.Log($"[BattleRoundCoordinator] Player has {remainingCards} cards remaining in hand");

        if (remainingCards > 0)
        {
            if (dialogText != null)
            {
                dialogText.text = "Choose new fighter!";
            }
            fightSystem.state = FightStateMultiplayer.PLAYERDEATH;

            Debug.Log("[BattleRoundCoordinator] Unlocking hand for new card selection");

            var boardManager = fightSystem.multiplayerBoardManager;
            if (boardManager != null)
            {
                boardManager.UnlockPlayerHand();
            }
            else
            {
                Debug.LogError("[BattleRoundCoordinator] MultiplayerBoardManager not found - cannot unlock hand!");
            }
        }
        else
        {
            if (dialogText != null)
            {
                dialogText.text = "You Lost! No cards left!";
            }
            fightSystem.state = FightStateMultiplayer.LOST;
            Debug.Log("[BattleRoundCoordinator] Player lost - no cards remaining");
        }
    }

    public IEnumerator HandleEnemyCardDeath(Kard enemyCard)
    {
        if (killCounterManager != null)
        {
            Debug.Log("[BattleRoundCoordinator] Enemy card died - incrementing player kill count");
            killCounterManager.OnPlayerKilledEnemyCard();
        }
        else
        {
            Debug.LogWarning("[BattleRoundCoordinator] KillCounterManager not assigned!");
        }

        yield return HandleCardDeath(enemyCard, isMyCard: false);

        if (dialogText != null)
        {
            dialogText.text = "Opponent choosing new fighter...";
        }
        Debug.Log("[BattleRoundCoordinator] Waiting for opponent to select new card...");

        var boardManager = fightSystem.multiplayerBoardManager;
        if (boardManager == null)
        {
            Debug.LogError("[BattleRoundCoordinator] MultiplayerBoardManager not found!");
            if (dialogText != null)
            {
                dialogText.text = "You Won!";
            }
            fightSystem.state = FightStateMultiplayer.WON;
            yield break;
        }

        boardManager.opponentCardRevealed = false;

        var waitTask = boardManager.WaitForOpponentSelectionAsync();
        yield return new WaitUntil(() => waitTask.IsCompleted);

        boardManager.RevealCards();

        Debug.Log("[BattleRoundCoordinator] Enemy card revealed! Battle continues.");

        yield return new WaitForSeconds(0.5f);
        Debug.Log("[BattleRoundCoordinator] Replacement selection completed - continuing without legacy ClearBattleData cleanup");

        fightSystem.state = FightStateMultiplayer.TURN;
        Debug.Log($"[BattleRoundCoordinator] State set to TURN. Current state: {fightSystem.state}");

        var myCard = fightSystem.player.cardInGame;
        if (myCard != null && fightSystem.attackCountLoader != null)
        {
            Debug.Log($"[BattleRoundCoordinator] Reloading attack counts for {myCard.cardName}");
            fightSystem.LoadAttackCounts(myCard);
        }
    }

    public IEnumerator HandleBothCardsDeath(Kard myCard, Kard enemyCard)
    {
        if (killCounterManager != null)
        {
            Debug.Log("[BattleRoundCoordinator] Both cards died - incrementing both kill counts");
            killCounterManager.OnEnemyKilledPlayerCard();
            killCounterManager.OnPlayerKilledEnemyCard();
        }
        else
        {
            Debug.LogWarning("[BattleRoundCoordinator] KillCounterManager not assigned!");
        }

        yield return HandleCardDeath(myCard, isMyCard: true);
        yield return HandleCardDeath(enemyCard, isMyCard: false);

        Player player = fightSystem?.player;
        if (player == null)
        {
            if (dialogText != null)
            {
                dialogText.text = "Draw!";
            }
            fightSystem.state = FightStateMultiplayer.WON;
            yield break;
        }

        int remainingCards = player.hand.Count;
        Debug.Log($"[BattleRoundCoordinator] Both died - Player has {remainingCards} cards remaining");

        if (remainingCards > 0)
        {
            if (dialogText != null)
            {
                dialogText.text = "Both destroyed! Choose new fighter!";
            }
            fightSystem.state = FightStateMultiplayer.PLAYERDEATH;

            Debug.Log("[BattleRoundCoordinator] Unlocking hand after mutual destruction");

            var boardManager = fightSystem.multiplayerBoardManager;
            if (boardManager != null)
            {
                boardManager.UnlockPlayerHand();
            }
            else
            {
                Debug.LogError("[BattleRoundCoordinator] MultiplayerBoardManager not found in both cards death!");
            }
        }
        else
        {
            if (dialogText != null)
            {
                dialogText.text = "Draw! No cards left!";
            }
            fightSystem.state = FightStateMultiplayer.WON;
            Debug.Log("[BattleRoundCoordinator] Draw - both players out of cards");
        }
    }

    private static bool TryGetSuccessFlag(object functionResult, out bool success)
    {
        success = false;
        Dictionary<string, object> data = DeserializeToDictionary(functionResult);
        if (data == null || !data.TryGetValue("success", out var successObj) || successObj == null)
        {
            return false;
        }

        if (successObj is bool b)
        {
            success = b;
            return true;
        }

        if (bool.TryParse(successObj.ToString(), out var parsedBool))
        {
            success = parsedBool;
            return true;
        }

        return false;
    }

    private static bool IsBothPlayersReadyFromMatchState(MatchStateDto matchState)
    {
        if (matchState == null)
        {
            return false;
        }

        if (matchState.nextTurnReady != null && matchState.nextTurnReady.bothReady)
        {
            return true;
        }

        return string.Equals(matchState.phase, MatchPhaseSelectingAttacks, System.StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasNextTurnReadyPhaseAdvanced(MatchStateDto matchState)
    {
        if (matchState == null || string.IsNullOrEmpty(matchState.phase))
        {
            return false;
        }

        return string.Equals(matchState.phase, MatchPhaseSelectingAttacks, System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(matchState.phase, MatchPhaseWaitingForAttacks, System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(matchState.phase, MatchPhaseResolvingBattle, System.StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWaitingForResultAcknowledgement(MatchStateDto matchState)
    {
        return matchState != null
            && string.Equals(matchState.phase, MatchPhaseWaitingForResultAck, System.StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPlayerMarkedReadyInMatchState(MatchStateDto matchState, string playerId)
    {
        if (matchState == null || string.IsNullOrEmpty(playerId))
        {
            return true;
        }

        if (matchState.nextTurnReady?.readyByPlayerId != null
            && matchState.nextTurnReady.readyByPlayerId.TryGetValue(playerId, out var readyById))
        {
            return readyById;
        }

        if (matchState.seats != null)
        {
            for (int i = 0; i < matchState.seats.Count; i++)
            {
                MatchSeatDto seat = matchState.seats[i];
                if (seat != null && seat.playerId == playerId)
                {
                    return seat.readyForNextTurn;
                }
            }
        }

        return true;
    }

    private static string TryGetErrorMessage(object functionResult)
    {
        Dictionary<string, object> data = DeserializeToDictionary(functionResult);
        if (data == null)
        {
            return null;
        }

        if (data.TryGetValue("error", out var errorObj) && errorObj != null)
        {
            return errorObj.ToString();
        }

        if (data.TryGetValue("message", out var messageObj) && messageObj != null)
        {
            return messageObj.ToString();
        }

        return null;
    }

    private static Dictionary<string, object> DeserializeToDictionary(object value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is Dictionary<string, object> dict)
        {
            return dict;
        }

        if (value is JObject jObject)
        {
            return jObject.ToObject<Dictionary<string, object>>();
        }

        try
        {
            return PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
                .DeserializeObject<Dictionary<string, object>>(value.ToString());
        }
        catch
        {
            return null;
        }
    }
}
