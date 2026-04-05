using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;

/// <summary>
/// Zodpovedny za odosielanie utokov na server a cakanie na vysledky
/// </summary>
public class BattleSubmitter : MonoBehaviour
{
    [Header("Dependencies")]
    public ServerFunctionsManager serverFunctionsManager;
    public FightSystemMultiplayer fightSystem;
    public BattleResultProcessor resultProcessor;
    
    private bool isWaitingForBattle = false;
    private int pollAttempts = 0;
    private const int MAX_POLL_ATTEMPTS = 60; // 60 sekund timeout
    
    /// <summary>
    /// Odosle utok na server a spusti polling pre vysledok
    /// </summary>
    public void SubmitAttack(string roomCode, string playerId, string cardId, int attackId, int attackSlot)
    {
        if (isWaitingForBattle)
        {
            Debug.LogWarning("[BattleSubmitter] Already waiting for battle result");
            return;
        }
        
        isWaitingForBattle = true;
        pollAttempts = 0;
        
        Debug.Log($"[BattleSubmitter] Submitting attack: roomCode={roomCode}, cardId={cardId}, attackId={attackId}, attackSlot={attackSlot}");
        
        // Minimalny submit kontrakt: klient posiela iba identifikaciu utoku.
        // Server nacita zivy card state zo selectedCards v roomke.
        var submission = new AttackSubmission
        {
            playerId = playerId,
            roomCode = roomCode,
            cardId = cardId,
            attackId = attackId,
            attackSlot = attackSlot
        };
        
        // Odosli na server
        serverFunctionsManager.ExecuteBattle(roomCode, playerId, submission, result =>
        {
            OnBattleResponse(result);
        });
    }
    
    /// <summary>
    /// Callback po odpovedi zo servera
    /// </summary>
    private void OnBattleResponse(PlayFab.CloudScriptModels.ExecuteFunctionResult result)
    {
        if (result == null || result.FunctionResult == null)
        {
            Debug.LogError("[BattleSubmitter] Server response is null");
            isWaitingForBattle = false;
            return;
        }
        
        var resultData = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
            .DeserializeObject<Dictionary<string, object>>(result.FunctionResult.ToString());
        
        bool success = resultData.ContainsKey("success") && (bool)resultData["success"];
        bool bothReady = resultData.ContainsKey("bothPlayersReady") && (bool)resultData["bothPlayersReady"];
        
        if (success && bothReady)
        {
            Debug.Log("[BattleSubmitter] Both players ready - battle executed!");
            
            // Spracuj vysledok
            if (resultData.ContainsKey("battleResult"))
            {
                var battleResult = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
                    .DeserializeObject<Dictionary<string, object>>(resultData["battleResult"].ToString());
                
                resultProcessor.ProcessBattleResult(battleResult);
            }
            
            isWaitingForBattle = false;
        }
        else
        {
            // Cakaj na druheho hraca
            int playersReadyCount = resultData.ContainsKey("playersReadyCount") ? int.Parse(resultData["playersReadyCount"].ToString()) : 0;
            Debug.Log($"[BattleSubmitter] Waiting for opponent... ({playersReadyCount}/2)");
            
            // Pokracuj v pollingu
            StartCoroutine(PollForBattleResult());
        }
    }
    
    /// <summary>
    /// Polling - caka kym nie su obaja hraci ready
    /// </summary>
    private IEnumerator PollForBattleResult()
    {
        yield return new WaitForSeconds(1f);
        
        pollAttempts++;
        
        if (pollAttempts >= MAX_POLL_ATTEMPTS)
        {
            Debug.LogError("[BattleSubmitter] Timeout waiting for opponent");
            isWaitingForBattle = false;
            yield break;
        }
        
        serverFunctionsManager.GetBattleStatus(fightSystem.roomCode, fightSystem.myPlayerId, result =>
        {
            OnBattleResponse(result);
        });
    }
}

