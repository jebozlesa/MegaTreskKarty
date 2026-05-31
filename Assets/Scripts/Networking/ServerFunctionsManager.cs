using System;
using System.Collections;
using PlayFab;
using PlayFab.CloudScriptModels;
using UnityEngine;

public class ServerFunctionsManager : MonoBehaviour
{
    private const string MatchStateFunctionMissingMessage = "No function named getMatchState was found to execute";
    private const string AttackCountsNotInitializedMessage = "Attack counts not initialized - select card first!";
    private static bool VerboseFunctionLogs => false;
    private static readonly Action<ExecuteFunctionResult> NoOpCallback = _ => { };

    [Header("Network Error Indicator")]
    [Tooltip("GameObject ktorý sa zobrazí pri network error (napr. Image s error ikonou)")]
    public GameObject networkErrorIndicator;
    
    [Header("Retry Settings")]
    [Tooltip("Počet pokusov pri network error")]
    public int maxRetries = 3;
    
    [Tooltip("Delay medzi pokusmi v sekundách")]
    public float retryDelay = 1f;

    private bool isShuttingDown;

    private static void LogVerbose(string message)
    {
        if (VerboseFunctionLogs)
        {
            Debug.Log(message);
        }
    }

    private static void LogVerboseWarning(string message)
    {
        if (VerboseFunctionLogs)
        {
            Debug.LogWarning(message);
        }
    }

    private static bool ShouldAlwaysLogFunction(string functionName)
    {
        return string.Equals(functionName, "leaveRoom", StringComparison.OrdinalIgnoreCase)
            || string.Equals(functionName, "getMatchState", StringComparison.OrdinalIgnoreCase)
            || string.Equals(functionName, "cancelMatchmaking", StringComparison.OrdinalIgnoreCase);
    }

    // Nová funkcia: načítanie balíčkov hráčov do miestnosti
    public async System.Threading.Tasks.Task<ExecuteFunctionResult> LoadPlayerDecksIntoRoomAsync(string playerId, string roomCode)
    {
        var tcs = new System.Threading.Tasks.TaskCompletionSource<ExecuteFunctionResult>();
        var parameters = new {
            playerId = playerId,
            roomCode = roomCode
        };
        CallFunction("loadPlayerDecksIntoRoom", parameters, result =>
        {
            tcs.SetResult(result);
        });
        return await tcs.Task;
    }
    // Univerzálne volanie PlayFab funkcie
    public void CallFunction(string functionName, object parameters, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;
        LogVerboseWarning($"CallFunction: {functionName}, parameters: {Newtonsoft.Json.JsonConvert.SerializeObject(parameters)}");
        if (ShouldAlwaysLogFunction(functionName))
        {
            Debug.Log($"[ServerFunctionsManager] ExecuteFunction request: {functionName}, parameters={Newtonsoft.Json.JsonConvert.SerializeObject(parameters)}");
        }

        var request = new ExecuteFunctionRequest
        {
            FunctionName = functionName,
            FunctionParameter = parameters,
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request, result => {
            LogVerboseWarning($"ExecuteFunction result: {Newtonsoft.Json.JsonConvert.SerializeObject(result.FunctionResult)}");
            if (ShouldAlwaysLogFunction(functionName))
            {
                Debug.Log($"[ServerFunctionsManager] ExecuteFunction success: {functionName}, result={Newtonsoft.Json.JsonConvert.SerializeObject(result.FunctionResult)}");
            }
            
            // Success - hide network error indicator
            HideNetworkError();
            
            callback?.Invoke(result);
        }, error => {
            // Transient cloud/network failures are often resolved by retry.
            string errorReport = error?.GenerateErrorReport() ?? string.Empty;
            Debug.LogWarning(errorReport);
            if (ShouldAlwaysLogFunction(functionName))
            {
                Debug.LogError($"[ServerFunctionsManager] ExecuteFunction error: {functionName}, error={errorReport}");
            }

            if (ShouldShowNetworkError(functionName, errorReport))
            {
                ShowNetworkError($"Server error: {functionName}");
            }
            else
            {
                HideNetworkError();
                Debug.LogWarning($"[ServerFunctionsManager] Suppressed network indicator for expected {functionName} error");
            }
             
            callback?.Invoke(null);
        });
    }

    // Upravené: pripojenie do miestnosti podľa novej logiky s username
    public void JoinOrCreateRoom(string playerId, string username, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;
        LogVerboseWarning($"JoinOrCreateRoom called with playerId: {playerId}, username: {username}");
        var parameters = new {
            playerId = playerId,
            username = username
        };
        CallFunction("joinOrCreateRoom", parameters, callback);
    }

    // Legacy note: room/player polling now uses getMatchState, not getRoomPlayersInfo.

    public void GetMatchState(string roomCode, string playerId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        if (string.IsNullOrEmpty(roomCode) && string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("GetMatchState: roomCode or playerId is required!");
            callback?.Invoke(null);
            return;
        }

        Debug.Log($"[ServerFunctionsManager] GetMatchState requested: roomCode={roomCode}, playerId={playerId}");
        var parameters = new
        {
            roomCode,
            playerId
        };

        CallMatchStateFunction(parameters, callback, maxRetries);
    }

    public void CancelMatchmaking(string playerId, string roomCode, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        Debug.Log($"[ServerFunctionsManager] CancelMatchmaking requested: playerId={playerId}, roomCode={roomCode}");
        var parameters = new
        {
            playerId,
            roomCode
        };

        CallFunctionWithRetry("cancelMatchmaking", parameters, callback);
    }

    // Nová funkcia: aktualizácia informácií o hráčovi
    public void UpdatePlayerInfo(string playerId, string username, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;
        LogVerboseWarning($"UpdatePlayerInfo called with playerId: {playerId}, username: {username}");
        var parameters = new {
            playerId = playerId,
            username = username
        };
        CallFunction("updatePlayerInfo", parameters, callback);
    }

    // Preťažená verzia JoinOrCreateRoom pre spätnú kompatibilitu
    public void JoinOrCreateRoom(string playerId, Action<ExecuteFunctionResult> callback)
    {
        string username = PlayerPrefs.GetString("username", playerId);
        JoinOrCreateRoom(playerId, username, callback);
    }

    // Nová funkcia: opustenie miestnosti
    public void LeaveRoom(string playerId, Action<ExecuteFunctionResult> callback = null)
    {
        Debug.Log($"[ServerFunctionsManager] LeaveRoom requested: playerId={playerId}");
        var parameters = new {
            playerId = playerId
        };
        CallFunction("leaveRoom", parameters, callback ?? (_ => { }));
    }

    // Nová funkcia: heartbeat (životný signál)
    public void Heartbeat(string playerId, Action<ExecuteFunctionResult> callback = null)
    {
        LogVerbose($"Heartbeat called with playerId: {playerId}");
        var parameters = new {
            playerId = playerId
        };
        CallFunction("heartbeat", parameters, callback ?? (result => {
            // Tichý callback - heartbeat nemusí mať výstup
            if (result == null)
            {
                Debug.LogWarning("Heartbeat failed");
            }
        }));
    }

    // Nová funkcia: cleanup starých miestností
    public void CleanupRooms(Action<ExecuteFunctionResult> callback = null)
    {
        Debug.Log("CleanupRooms called");
        var parameters = new { }; // Prázdne parametre
        CallFunction("cleanupRooms", parameters, callback ?? (result => {
            if (result != null && result.FunctionResult != null)
            {
                Debug.Log($"Cleanup result: {result.FunctionResult}");
            }
        }));
    }

    // Nová funkcia: získanie deckov v miestnosti cez getRoomDecks
    public void GetRoomDecks(string roomCode, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;
        LogVerboseWarning($"GetRoomDecks called with roomCode: {roomCode}");
        var parameters = new {
            roomCode = roomCode
        };
        CallFunction("getRoomDecks", parameters, callback);
    }

    public void SetSelectedCard(string roomCode, string playerId, SelectedCardData cardData, Action<ExecuteFunctionResult> callback)
    {
        if (cardData == null)
        {
            Debug.LogError("SetSelectedCard: cardData is null!");
            callback?.Invoke(null);
            return;
        }

        if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("SetSelectedCard: roomCode or playerId missing!");
            callback?.Invoke(null);
            return;
        }

        LogVerboseWarning($"SetSelectedCard called with roomCode: {roomCode}, playerId: {playerId}, cardId: {cardData.cardId}");
        var parameters = new
        {
            roomCode,
            playerId,
            card = new
            {
                cardId = cardData.cardId,
                name = cardData.name,
                image = cardData.image,
                level = cardData.level,
                health = cardData.health,
                maxHealth = cardData.maxHealth,
                styleId = cardData.styleId,
                strength = cardData.strength,
                speed = cardData.speed,
                attack = cardData.attack,
                defense = cardData.defense,
                knowledge = cardData.knowledge,
                charisma = cardData.charisma,
                experience = cardData.experience,
                attack1 = cardData.attack1,
                attack2 = cardData.attack2,
                attack3 = cardData.attack3,
                attack4 = cardData.attack4,
                color = cardData.color ?? new int[] { 255, 255, 255 }
            }
        };

        // Use retry for this request because it can fail when a player leaves the room.
        CallFunctionWithRetry("setSelectedCard", parameters, callback);
    }

    // Legacy note: selected-card refresh and replacement flow now use getMatchState.
    // Do not add new client flow-control calls to getSelectedCards/clearSelectedCards/clearBattleData.

    // ============================================================
    // V8: Attack counts (server auto-init and auto-decrement)
    // ============================================================
    // Removed: CalculateAttackCounts() because the server auto-initializes counts in setSelectedCard.
    // Removed: DecrementAttackCount() because the server auto-decrements counts in executeBattle.

    /// <summary>
    /// Načíta persisted attack counts z MongoDB (read-only)
    /// Server automaticky inicializuje counts pri setSelectedCard
    /// Server automaticky decrementuje counts pri executeBattle
    /// </summary>
    public void GetAttackCounts(string roomCode, string playerId, string cardId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"[ServerFunctionsManager] GetAttackCounts: roomCode={roomCode}, playerId={playerId}, cardId={cardId}");
        var parameters = new
        {
            roomCode = roomCode,
            playerId = playerId,
            cardId = cardId
        };
        // Retry because attack counts are critical for the UI.
        CallFunctionWithRetry("getAttackCounts", parameters, callback);
    }

    /// <summary>
    /// Odošle útok na server a vráti výsledok battle
    /// </summary>
    public void ExecuteBattle(string roomCode, string playerId, AttackSubmission attackData, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"ExecuteBattle called for room: {roomCode}, player: {playerId}");
        
        var parameters = new
        {
            roomCode = roomCode,
            playerId = playerId,
            attackData = attackData
        };
        
        // Retry because executeBattle is the most critical battle request.
        CallFunctionWithRetry("executeBattle", parameters, callback);
    }

    /// <summary>
    /// Polling status po submitnuti utoku cez dedicated getBattleStatus endpoint.
    /// </summary>
    public void GetBattleStatus(string roomCode, string playerId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"GetBattleStatus called for room: {roomCode}, player: {playerId}");

        var parameters = new
        {
            roomCode = roomCode,
            playerId = playerId
        };

        CallFunctionWithRetry("getBattleStatus", parameters, callback);
    }
    
    /// <summary>
    /// Označí hráča ako ready pre ďalší turn
    /// </summary>
    public void MarkReadyForNextTurn(string roomCode, string playerId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"MarkReadyForNextTurn called for room: {roomCode}, player: {playerId}");
        
        var parameters = new
        {
            roomCode = roomCode,
            playerId = playerId
        };
        
        // Retry because turn synchronization is critical.
        CallFunctionWithRetry("markReadyForNextTurn", parameters, callback);
    }
    
    // Legacy note: next-turn readiness polling now uses getMatchState.
    
    /// <summary>
    /// Zobraz network error indikátor
    /// </summary>
    public void CreateRoyalRumbleSession(string playerId, string username, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"CreateRoyalRumbleSession called for player: {playerId}");
        var parameters = new
        {
            playerId = playerId,
            username = username
        };

        CallFunctionWithRetry("createRoyalRumbleSession", parameters, callback);
    }

    public void GetRoyalRumbleSession(string sessionId, string playerId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"GetRoyalRumbleSession called for session: {sessionId}, player: {playerId}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId
        };

        CallFunctionWithRetry("getRoyalRumbleSession", parameters, callback);
    }

    public void LoadRoyalRumblePlayerDeck(string sessionId, string playerId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"LoadRoyalRumblePlayerDeck called for session: {sessionId}, player: {playerId}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId
        };

        CallFunctionWithRetry("loadRoyalRumblePlayerDeck", parameters, callback);
    }

    public void LoadRoyalRumbleEnemyDeck(string sessionId, string playerId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"LoadRoyalRumbleEnemyDeck called for session: {sessionId}, player: {playerId}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId
        };

        CallFunctionWithRetry("loadRoyalRumbleEnemyDeck", parameters, callback);
    }

    public void SelectRoyalRumbleCard(string sessionId, string playerId, string cardId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"SelectRoyalRumbleCard called for session: {sessionId}, player: {playerId}, card: {cardId}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId,
            cardId = cardId
        };

        CallFunctionWithRetry("selectRoyalRumbleCard", parameters, callback);
    }

    public void SubmitRoyalRumbleAttack(string sessionId, string playerId, int attackSlot, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"SubmitRoyalRumbleAttack called for session: {sessionId}, player: {playerId}, slot: {attackSlot}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId,
            attackSlot = attackSlot
        };

        CallFunctionWithRetry("submitRoyalRumbleAttack", parameters, callback);
    }

    public void CreateCampaignSession(string playerId, string campaignId, int missionId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"CreateCampaignSession called for player: {playerId}, campaign: {campaignId}, mission: {missionId}");
        var parameters = new
        {
            playerId = playerId,
            campaignId = campaignId,
            missionId = missionId
        };

        CallFunctionWithRetry("createCampaignSession", parameters, callback);
    }

    public void LoadCampaignPlayerDeck(string sessionId, string playerId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"LoadCampaignPlayerDeck called for session: {sessionId}, player: {playerId}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId
        };

        CallFunctionWithRetry("loadCampaignPlayerDeck", parameters, callback);
    }

    public void LoadCampaignEnemyDeck(string sessionId, string playerId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"LoadCampaignEnemyDeck called for session: {sessionId}, player: {playerId}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId
        };

        CallFunctionWithRetry("loadCampaignEnemyDeck", parameters, callback);
    }

    public void SelectCampaignCard(string sessionId, string playerId, string cardId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"SelectCampaignCard called for session: {sessionId}, player: {playerId}, card: {cardId}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId,
            cardId = cardId
        };

        CallFunctionWithRetry("selectCampaignCard", parameters, callback);
    }

    public void SubmitCampaignAttack(string sessionId, string playerId, int attackSlot, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"SubmitCampaignAttack called for session: {sessionId}, player: {playerId}, slot: {attackSlot}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId,
            attackSlot = attackSlot
        };

        CallFunctionWithRetry("submitCampaignAttack", parameters, callback);
    }

    public void AbandonRoyalRumbleSession(string sessionId, string playerId, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;

        LogVerboseWarning($"AbandonRoyalRumbleSession called for session: {sessionId}, player: {playerId}");
        var parameters = new
        {
            sessionId = sessionId,
            playerId = playerId
        };

        CallFunctionWithRetry("abandonRoyalRumbleSession", parameters, callback);
    }

    private void ShowNetworkError(string errorMessage = "Network error")
    {
        if (networkErrorIndicator != null)
        {
            networkErrorIndicator.SetActive(true);
            Debug.LogWarning($"[ServerFunctionsManager] Network error shown: {errorMessage}");
        }
    }
    
    /// <summary>
    /// Skry network error indikátor
    /// </summary>
    private void HideNetworkError()
    {
        if (networkErrorIndicator != null && networkErrorIndicator.activeSelf)
        {
            networkErrorIndicator.SetActive(false);
            Debug.LogWarning("[ServerFunctionsManager] Network error hidden - connection OK");
        }
    }
    
    /// <summary>
    /// Zavolá funkciu s retry mechanikou pri network error
    /// </summary>
    public void CallFunctionWithRetry(string functionName, object parameters, Action<ExecuteFunctionResult> callback, int retriesLeft = -1)
    {
        if (this == null || isShuttingDown || !isActiveAndEnabled)
        {
            callback?.Invoke(null);
            return;
        }

        if (retriesLeft == -1) retriesLeft = maxRetries;
        
        CallFunction(functionName, parameters, result =>
        {
            if (this == null || isShuttingDown || !isActiveAndEnabled)
            {
                callback?.Invoke(null);
                return;
            }

            if (result != null)
            {
                // Úspech!
                callback?.Invoke(result);
            }
            else if (retriesLeft > 0)
            {
                // Neúspech - skús znova
                Debug.LogWarning($"[ServerFunctionsManager] Retrying {functionName} ({retriesLeft} attempts left)...");
                StartCoroutine(RetryAfterDelay(functionName, parameters, callback, retriesLeft - 1));
            }
            else
            {
                // Vyčerpané pokusy
                Debug.LogError($"[ServerFunctionsManager] Failed: {functionName} failed after {maxRetries} retries!");
                callback?.Invoke(null);
            }
        });
    }
    
    /// <summary>
    /// Počká a potom retry
    /// </summary>
    private IEnumerator RetryAfterDelay(string functionName, object parameters, Action<ExecuteFunctionResult> callback, int retriesLeft)
    {
        yield return new WaitForSeconds(retryDelay);
        if (this == null || isShuttingDown || !isActiveAndEnabled)
        {
            callback?.Invoke(null);
            yield break;
        }

        CallFunctionWithRetry(functionName, parameters, callback, retriesLeft);
    }

    private void CallMatchStateFunction(object parameters, Action<ExecuteFunctionResult> callback, int retriesLeft)
    {
        Debug.Log($"[ServerFunctionsManager] ExecuteFunction request: getMatchState, parameters={Newtonsoft.Json.JsonConvert.SerializeObject(parameters)}, retriesLeft={retriesLeft}");
        var request = new ExecuteFunctionRequest
        {
            FunctionName = "getMatchState",
            FunctionParameter = parameters,
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request, result =>
        {
            LogVerboseWarning($"ExecuteFunction result: {Newtonsoft.Json.JsonConvert.SerializeObject(result.FunctionResult)}");
            Debug.Log($"[ServerFunctionsManager] ExecuteFunction success: getMatchState, result={Newtonsoft.Json.JsonConvert.SerializeObject(result.FunctionResult)}");
            HideNetworkError();
            callback?.Invoke(result);
        }, error =>
        {
            string errorReport = error?.GenerateErrorReport() ?? string.Empty;
            bool isMissingFunction = errorReport.IndexOf(MatchStateFunctionMissingMessage, StringComparison.OrdinalIgnoreCase) >= 0;

            if (isMissingFunction)
            {
                Debug.LogError("[ServerFunctionsManager] getMatchState is not registered in PlayFab.");
            }

            Debug.LogWarning(errorReport);
            Debug.LogError($"[ServerFunctionsManager] ExecuteFunction error: getMatchState, error={errorReport}");
            ShowNetworkError("Server error: getMatchState");

            if (retriesLeft > 0)
            {
                Debug.LogWarning($"[ServerFunctionsManager] Retrying getMatchState ({retriesLeft} attempts left)...");
                StartCoroutine(RetryMatchStateAfterDelay(parameters, callback, retriesLeft - 1));
            }
            else
            {
                Debug.LogError($"[ServerFunctionsManager] Failed: getMatchState failed after {maxRetries} retries!");
                callback?.Invoke(null);
            }
        });
    }

    private static bool ShouldShowNetworkError(string functionName, string errorReport)
    {
        if (string.IsNullOrEmpty(errorReport))
        {
            return true;
        }

        if (string.Equals(functionName, "getAttackCounts", StringComparison.OrdinalIgnoreCase)
            && errorReport.IndexOf(AttackCountsNotInitializedMessage, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return false;
        }

        return true;
    }

    private IEnumerator RetryMatchStateAfterDelay(object parameters, Action<ExecuteFunctionResult> callback, int retriesLeft)
    {
        yield return new WaitForSeconds(retryDelay);
        if (this == null || isShuttingDown || !isActiveAndEnabled)
        {
            callback?.Invoke(null);
            yield break;
        }

        CallMatchStateFunction(parameters, callback, retriesLeft);
    }

    private void OnDestroy()
    {
        isShuttingDown = true;
        StopAllCoroutines();
    }
    
    // ====================================
    // V11: SERVER-SIDE CARD PACK GENERATION
    // ====================================
    
    /// <summary>
    /// V11: Open a card pack on the server (server-authoritative generation).
    /// Server vygeneruje 6 kariet z themed packu a vráti ich
    /// </summary>
    /// <param name="playFabId">PlayFab ID hráča</param>
    /// <param name="packIndex">Index themed packu (0, 1, 2)</param>
    /// <param name="callback">Callback s vygenerovanými kartami</param>
    public void OpenCardPack(string playFabId, int packIndex, Action<ExecuteFunctionResult> callback)
    {
        callback ??= NoOpCallback;
        
        Debug.LogWarning($"[ServerFunctionsManager] OpenCardPack: playFabId={playFabId}, packIndex={packIndex}");
        
        var parameters = new
        {
            playFabId = playFabId,
            packIndex = packIndex
        };
        
        // Call with retry (network safety)
        CallFunctionWithRetry("openCardPack", parameters, callback);
    }
}



