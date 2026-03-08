using System.Collections;
using System.Collections.Generic;
using TMPro;
using PlayFab;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class BattleRoundCoordinator
{
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
        int pollAttempts = 0;
        const int MAX_POLL_ATTEMPTS = 30;
        const int RETRY_MARK_READY_AFTER_POLLS = 3;

        while (pollAttempts < MAX_POLL_ATTEMPTS)
        {
            yield return new WaitForSeconds(1f);
            pollAttempts++;

            bool isCompleted = false;
            bool bothReady = false;
            bool iAmMarkedReady = true;

            serverFunctions.CheckNextTurnReady(fightSystem.roomCode, result => {
                if (result?.FunctionResult != null)
                {
                    var resultData = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
                        .DeserializeObject<Dictionary<string, object>>(result.FunctionResult.ToString());

                    if (resultData.ContainsKey("bothPlayersReady"))
                    {
                        bothReady = (bool)resultData["bothPlayersReady"];
                    }

                    if (resultData.ContainsKey("playersReady"))
                    {
                        var playersReady = resultData["playersReady"] as Dictionary<string, object>;
                        if (playersReady != null && playersReady.ContainsKey(fightSystem.myPlayerId))
                        {
                            iAmMarkedReady = (bool)playersReady[fightSystem.myPlayerId];
                            if (!iAmMarkedReady)
                            {
                                Debug.LogWarning($"[BattleRoundCoordinator] Poll #{pollAttempts}: I'm NOT marked ready in DB!");
                            }
                        }
                    }
                }
                isCompleted = true;
            });

            yield return new WaitUntil(() => isCompleted);

            if (bothReady)
            {
                Debug.Log("[BattleRoundCoordinator] Both players ready after polling!");
                break;
            }

            if (!iAmMarkedReady && pollAttempts % RETRY_MARK_READY_AFTER_POLLS == 0)
            {
                Debug.LogWarning($"[BattleRoundCoordinator] Retrying MarkReadyForNextTurn (attempt {pollAttempts / RETRY_MARK_READY_AFTER_POLLS})");

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

        if (pollAttempts >= MAX_POLL_ATTEMPTS)
        {
            Debug.LogError("[BattleRoundCoordinator] Timeout waiting for opponent to be ready");
            if (dialogText != null)
            {
                dialogText.text = "Opponent disconnected?";
            }
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

        var serverFunctions = fightSystem.serverFunctionsManager;
        if (serverFunctions == null)
        {
            Debug.LogError("[BattleRoundCoordinator] ServerFunctionsManager not found! Cannot clear dead card from server.");
            yield break;
        }

        string roomCode = multiplayerService?.RoomCode;
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogError("[BattleRoundCoordinator] RoomCode is null/empty! Cannot clear dead card from server.");
            yield break;
        }

        Debug.Log($"[BattleRoundCoordinator] Calling server to clear dead card - roomCode: {roomCode}, cardId: {cardId}");

        bool serverCallCompleted = false;
        bool serverCallSuccess = false;

        serverFunctions.ClearDeadCard(roomCode, cardId, (result) =>
        {
            serverCallCompleted = true;

            if (result != null && result.FunctionResult != null)
            {
                if (TryGetSuccessFlag(result.FunctionResult, out var successFlag))
                {
                    serverCallSuccess = successFlag;

                    if (serverCallSuccess)
                    {
                        Debug.Log($"[BattleRoundCoordinator] Dead card cleared from server: {cardName} (ID: {cardId})");
                    }
                    else
                    {
                        string errorMsg = TryGetErrorMessage(result.FunctionResult) ?? "Unknown error";
                        Debug.LogError($"[BattleRoundCoordinator] Server failed to clear dead card: {errorMsg}");
                    }
                }
                else
                {
                    Debug.LogWarning("[BattleRoundCoordinator] Could not parse clear-dead-card response format");
                }
            }
            else
            {
                Debug.LogError("[BattleRoundCoordinator] Server call returned null result!");
            }
        });

        float timeout = 10f;
        float elapsed = 0f;
        while (!serverCallCompleted && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (!serverCallCompleted)
        {
            Debug.LogError($"[BattleRoundCoordinator] Server call timeout after {timeout}s - dead card may still be in selectedCards!");
        }
        else if (!serverCallSuccess)
        {
            Debug.LogWarning("[BattleRoundCoordinator] Server call completed but failed - check server logs");
        }

        // Guard against callback races: ensure dead card is really gone from selectedCards before continuing.
        if (!serverCallSuccess)
        {
            yield return WaitForCardRemovalFromSelectedCards(roomCode, cardId, 10f);
        }

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

        var serverFunctions = fightSystem.serverFunctionsManager;
        if (serverFunctions != null)
        {
            Debug.Log("[BattleRoundCoordinator] Clearing old battle result from server...");

            bool clearCompleted = false;
            bool clearSuccess = false;

            serverFunctions.ClearBattleData(fightSystem.roomCode, fightSystem.myPlayerId, result =>
            {
                clearCompleted = true;
                clearSuccess = result != null &&
                               result.FunctionResult != null &&
                               TryGetSuccessFlag(result.FunctionResult, out var success) &&
                               success;

                if (clearSuccess)
                {
                    Debug.Log("[BattleRoundCoordinator] Old battle result cleared successfully!");
                }
                else
                {
                    Debug.LogWarning("[BattleRoundCoordinator] Failed to clear battle result - may cause issues!");
                }
            });

            float waitTime = 0f;
            while (!clearCompleted && waitTime < 5f)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }

            if (!clearCompleted)
            {
                Debug.LogWarning("[BattleRoundCoordinator] ClearBattleData timeout after 5s - continuing anyway");
            }
        }
        else
        {
            Debug.LogError("[BattleRoundCoordinator] ServerFunctionsManager not found!");
        }

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

    private IEnumerator WaitForCardRemovalFromSelectedCards(string roomCode, string deadCardId, float timeoutSeconds)
    {
        var serverFunctions = fightSystem.serverFunctionsManager;
        if (serverFunctions == null)
        {
            yield break;
        }

        float elapsed = 0f;
        const float pollInterval = 0.5f;

        while (elapsed < timeoutSeconds)
        {
            bool completed = false;
            bool cardStillPresent = true;

            serverFunctions.GetSelectedCards(roomCode, result =>
            {
                cardStillPresent = IsCardPresentInSelectedCards(result?.FunctionResult, deadCardId);
                completed = true;
            });

            yield return new WaitUntil(() => completed);

            if (!cardStillPresent)
            {
                Debug.Log($"[BattleRoundCoordinator] Confirmed dead card removal from selectedCards: {deadCardId}");
                yield break;
            }

            yield return new WaitForSeconds(pollInterval);
            elapsed += pollInterval;
        }

        Debug.LogWarning($"[BattleRoundCoordinator] Timed out waiting for dead card removal from selectedCards: {deadCardId}");
    }

    private static bool IsCardPresentInSelectedCards(object functionResult, string cardId)
    {
        if (string.IsNullOrEmpty(cardId))
        {
            return false;
        }

        Dictionary<string, object> payload = DeserializeToDictionary(functionResult);
        if (payload == null || !payload.TryGetValue("selectedCards", out var selectedCardsObj))
        {
            return true;
        }

        Dictionary<string, object> selectedCards = DeserializeToDictionary(selectedCardsObj);
        if (selectedCards == null)
        {
            return true;
        }

        foreach (var kvp in selectedCards)
        {
            Dictionary<string, object> cardData = DeserializeToDictionary(kvp.Value);
            if (cardData == null)
            {
                continue;
            }

            if (cardData.TryGetValue("cardId", out var idObj) && idObj != null && idObj.ToString() == cardId)
            {
                return true;
            }
        }

        return false;
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
