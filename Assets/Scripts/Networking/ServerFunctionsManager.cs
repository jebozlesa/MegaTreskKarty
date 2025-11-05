using System;
using System.Collections;
using PlayFab;
using PlayFab.CloudScriptModels;
using UnityEngine;

public class ServerFunctionsManager : MonoBehaviour
{
    [Header("Network Error Indicator")]
    [Tooltip("GameObject ktorý sa zobrazí pri network error (napr. Image s error ikonou)")]
    public GameObject networkErrorIndicator;
    
    [Header("Retry Settings")]
    [Tooltip("Počet pokusov pri network error")]
    public int maxRetries = 3;
    
    [Tooltip("Delay medzi pokusmi v sekundách")]
    public float retryDelay = 1f;
    
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
        if (callback == null)
        {
            Debug.LogError($"CallFunction: callback is null! functionName={functionName}");
            return;
        }
        Debug.LogWarning($"CallFunction: {functionName}, parameters: {Newtonsoft.Json.JsonConvert.SerializeObject(parameters)}");
        var request = new ExecuteFunctionRequest
        {
            FunctionName = functionName,
            FunctionParameter = parameters,
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request, result => {
            Debug.LogWarning($"ExecuteFunction result: {Newtonsoft.Json.JsonConvert.SerializeObject(result.FunctionResult)}");
            
            // ✅ Úspech - skry error indikátor
            HideNetworkError();
            
            callback?.Invoke(result);
        }, error => {
            Debug.LogError(error.GenerateErrorReport());
            
            // ❌ Chyba - zobraz error indikátor
            ShowNetworkError($"Server error: {functionName}");
            
            callback?.Invoke(null);
        });
    }

    // Upravené: pripojenie do miestnosti podľa novej logiky s username
    public void JoinOrCreateRoom(string playerId, string username, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("JoinOrCreateRoom: callback is null!");
            return;
        }
        Debug.LogWarning($"JoinOrCreateRoom called with playerId: {playerId}, username: {username}");
        var parameters = new {
            playerId = playerId,
            username = username
        };
        CallFunction("joinOrCreateRoom", parameters, callback);
    }

    // Nová funkcia: získanie informácií o hráčoch v miestnosti
    public void GetRoomPlayersInfo(string roomCode, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("GetRoomPlayersInfo: callback is null!");
            return;
        }
        Debug.LogWarning($"GetRoomPlayersInfo called with roomCode: {roomCode}");
        var parameters = new {
            roomCode = roomCode
        };
        CallFunction("getRoomPlayersInfo", parameters, callback);
    }

    // Nová funkcia: aktualizácia informácií o hráčovi
    public void UpdatePlayerInfo(string playerId, string username, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("UpdatePlayerInfo: callback is null!");
            return;
        }
        Debug.LogWarning($"UpdatePlayerInfo called with playerId: {playerId}, username: {username}");
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
    public void LeaveRoom(string playerId, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("LeaveRoom: callback is null!");
            return;
        }
        Debug.LogWarning($"LeaveRoom called with playerId: {playerId}");
        var parameters = new {
            playerId = playerId
        };
        CallFunction("leaveRoom", parameters, callback);
    }

    // Nová funkcia: heartbeat (životný signál)
    public void Heartbeat(string playerId, Action<ExecuteFunctionResult> callback = null)
    {
        Debug.Log($"Heartbeat called with playerId: {playerId}");
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

    // Nová funkcia: označenie miestnosti ako completed
    public void MarkRoomAsCompleted(string roomCode, string playerId, Action<ExecuteFunctionResult> callback = null)
    {
        Debug.Log($"MarkRoomAsCompleted called with roomCode: {roomCode}, playerId: {playerId}");
        var parameters = new {
            roomCode = roomCode,
            playerId = playerId
        };
        CallFunction("markRoomAsCompleted", parameters, callback ?? (result => {
            if (result != null && result.FunctionResult != null)
            {
                Debug.Log($"Mark room completed result: {result.FunctionResult}");
            }
        }));
    }

    // Nová funkcia: získanie deckov v miestnosti cez getRoomDecks
    public void GetRoomDecks(string roomCode, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("GetRoomDecks: callback is null!");
            return;
        }
        Debug.LogWarning($"GetRoomDecks called with roomCode: {roomCode}");
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

        Debug.LogWarning($"SetSelectedCard called with roomCode: {roomCode}, playerId: {playerId}, cardId: {cardData.cardId}");
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

        // ✅ Použij retry mechanizmus - často failne ak hráč opustil room
        CallFunctionWithRetry("setSelectedCard", parameters, callback);
    }

    public void GetSelectedCards(string roomCode, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("GetSelectedCards: callback is null!");
            return;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogError("GetSelectedCards: roomCode is empty!");
            callback?.Invoke(null);
            return;
        }

        Debug.LogWarning($"GetSelectedCards called with roomCode: {roomCode}");
        var parameters = new
        {
            roomCode
        };
        // ✅ Retry - polling funkcia, môže failnúť pri network glitchoch
        CallFunctionWithRetry("getSelectedCards", parameters, callback);
    }

    public void ClearSelectedCards(string roomCode, Action<ExecuteFunctionResult> callback)
    {
        Debug.LogWarning($"ClearSelectedCards called with roomCode: {roomCode}");
        var parameters = new
        {
            roomCode
        };
        // ✅ Retry - kritická operácia pre card replacement!
        CallFunctionWithRetry("clearSelectedCards", parameters, callback ?? (_ => { }));
    }

    /// <summary>
    /// Vymaže mŕtvu kartu zo selectedCards na serveri (selective clear)
    /// </summary>
    public void ClearDeadCard(string roomCode, string cardIdToClear, Action<ExecuteFunctionResult> callback)
    {
        Debug.LogWarning($"[ServerFunctionsManager] ClearDeadCard called - roomCode: {roomCode}, cardId: {cardIdToClear}");
        var parameters = new
        {
            roomCode = roomCode,
            cardIdToClear = cardIdToClear
        };
        // ✅ Retry - SUPER KRITICKÉ! Ak failne, mŕtva karta ostane v DB!
        CallFunctionWithRetry("clearSelectedCards", parameters, callback ?? (_ => { }));
    }

    /// <summary>
    /// Vyčistí battle data po výmene karty - resetuje lastResult aby server vytvoril nový
    /// </summary>
    public void ClearBattleData(string roomCode, string playerId, Action<ExecuteFunctionResult> callback)
    {
        Debug.Log($"[ServerFunctionsManager] ClearBattleData called - roomCode: {roomCode}, playerId: {playerId}");
        var parameters = new
        {
            roomCode = roomCode,
            playerId = playerId
        };
        // ✅ Retry - kritická operácia pre turn cleanup!
        CallFunctionWithRetry("clearBattleData", parameters, callback ?? (_ => { }));
    }

    // ============================================================
    // ✅ V8: Attack Counts (Server Auto-Init + Auto-Decrement)
    // ============================================================
    // ❌ REMOVED: CalculateAttackCounts() - Server má auto-init v setSelectedCard
    // ❌ REMOVED: DecrementAttackCount() - Server má auto-decrement v executeBattle

    /// <summary>
    /// Načíta persisted attack counts z MongoDB (read-only)
    /// Server automaticky inicializuje counts pri setSelectedCard
    /// Server automaticky decrementuje counts pri executeBattle
    /// </summary>
    public void GetAttackCounts(string roomCode, string playerId, string cardId, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("GetAttackCounts: callback is null!");
            return;
        }

        Debug.LogWarning($"[ServerFunctionsManager] GetAttackCounts: roomCode={roomCode}, playerId={playerId}, cardId={cardId}");
        var parameters = new
        {
            roomCode = roomCode,
            playerId = playerId,
            cardId = cardId
        };
        // ✅ Retry - attack counts sú kritické pre UI
        CallFunctionWithRetry("getAttackCounts", parameters, callback);
    }

    /// <summary>
    /// Odošle útok na server a vráti výsledok battle
    /// </summary>
    public void ExecuteBattle(string roomCode, string playerId, AttackSubmission attackData, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("ExecuteBattle: callback is null!");
            return;
        }

        Debug.LogWarning($"ExecuteBattle called for room: {roomCode}, player: {playerId}");
        
        var parameters = new
        {
            roomCode = roomCode,
            playerId = playerId,
            attackData = attackData
        };
        
        // ✅ Retry - executeBattle je NAJKRITICKEJŠIA funkcia!
        CallFunctionWithRetry("executeBattle", parameters, callback);
    }
    
    /// <summary>
    /// Označí hráča ako ready pre ďalší turn
    /// </summary>
    public void MarkReadyForNextTurn(string roomCode, string playerId, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("MarkReadyForNextTurn: callback is null!");
            return;
        }

        Debug.LogWarning($"MarkReadyForNextTurn called for room: {roomCode}, player: {playerId}");
        
        var parameters = new
        {
            roomCode = roomCode,
            playerId = playerId
        };
        
        // ✅ Retry - turn synchronizácia je kritická
        CallFunctionWithRetry("markReadyForNextTurn", parameters, callback);
    }
    
    /// <summary>
    /// Skontroluje ready stav pre ďalší turn (polling)
    /// </summary>
    public void CheckNextTurnReady(string roomCode, Action<ExecuteFunctionResult> callback)
    {
        if (callback == null)
        {
            Debug.LogError("CheckNextTurnReady: callback is null!");
            return;
        }

        Debug.Log($"CheckNextTurnReady called for room: {roomCode}");
        
        var parameters = new
        {
            roomCode = roomCode
        };
        
        // ✅ Retry - polling funkcia pre next turn ready state
        CallFunctionWithRetry("checkNextTurnReady", parameters, callback);
    }
    
    /// <summary>
    /// Zobraz network error indikátor
    /// </summary>
    private void ShowNetworkError(string errorMessage = "Network error")
    {
        if (networkErrorIndicator != null)
        {
            networkErrorIndicator.SetActive(true);
            Debug.LogWarning($"[ServerFunctionsManager] 🔴 Network error shown: {errorMessage}");
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
            Debug.LogWarning("[ServerFunctionsManager] ✅ Network error hidden - connection OK");
        }
    }
    
    /// <summary>
    /// Zavolá funkciu s retry mechanikou pri network error
    /// </summary>
    public void CallFunctionWithRetry(string functionName, object parameters, Action<ExecuteFunctionResult> callback, int retriesLeft = -1)
    {
        if (retriesLeft == -1) retriesLeft = maxRetries;
        
        CallFunction(functionName, parameters, result =>
        {
            if (result != null)
            {
                // Úspech!
                callback?.Invoke(result);
            }
            else if (retriesLeft > 0)
            {
                // Neúspech - skús znova
                Debug.LogWarning($"[ServerFunctionsManager] 🔄 Retrying {functionName} ({retriesLeft} attempts left)...");
                StartCoroutine(RetryAfterDelay(functionName, parameters, callback, retriesLeft - 1));
            }
            else
            {
                // Vyčerpané pokusy
                Debug.LogError($"[ServerFunctionsManager] ❌ {functionName} failed after {maxRetries} retries!");
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
        CallFunctionWithRetry(functionName, parameters, callback, retriesLeft);
    }
}
