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
    private string lastSubmittedCardId; // [OK] Ulozeny cardId pre polling
    
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
        lastSubmittedCardId = cardId; // [OK] Uloz cardId pre polling
        
        Debug.Log($"[BattleSubmitter] Submitting attack: roomCode={roomCode}, cardId={cardId}, attackId={attackId}, attackSlot={attackSlot}");
        
        // [OK] V3 - MINIMALNY PAYLOAD: iba cardId + attackId + attackSlot
        // Server trackuje HP v room.battleState.playerHealths
        var submission = new AttackSubmission
        {
            playerId = playerId,
            roomCode = roomCode,
            cardId = cardId,
            attackId = attackId,
            attackSlot = attackSlot,  // [OK] NOVE - pre attack count decrement
            
            // DEPRECATED - server v3 tieto fieldy IGNORUJE
            // Ponechane kvoli backward compatibility s starsimi server verziami
            currentHealth = 0,  // Server ma HP v battleState
            attackerHealth = 0,
            defenderHealth = 0,
            attackerMaxHealth = 0,
            attackerStrength = 0,
            attackerDefense = 0,
            attackerSpeed = 0,
            attackerMagic = 0,
            defenderMaxHealth = 0,
            defenderStrength = 0,
            defenderDefense = 0,
            defenderSpeed = 0,
            defenderMagic = 0
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
            int playersReady = resultData.ContainsKey("playersReady") ? int.Parse(resultData["playersReady"].ToString()) : 0;
            Debug.Log($"[BattleSubmitter] Waiting for opponent... ({playersReady}/2)");
            
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
        
        // Dummy request na zistenie stavu
        var dummySubmission = new AttackSubmission
        {
            playerId = fightSystem.myPlayerId,
            roomCode = fightSystem.roomCode,
            cardId = lastSubmittedCardId, // [OK] Pouzij ulozeny cardId!
            attackId = 0, // Special value pre "check status only"
            attackSlot = 0 // Dummy value pre polling
        };
        
        serverFunctionsManager.ExecuteBattle(fightSystem.roomCode, fightSystem.myPlayerId, dummySubmission, result =>
        {
            OnBattleResponse(result);
        });
    }
}

