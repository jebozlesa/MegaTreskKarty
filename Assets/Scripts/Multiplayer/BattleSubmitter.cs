using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;

/// <summary>
/// Zodpovedny za odosielanie utokov na server a cakanie na vysledky
/// </summary>
public class BattleSubmitter : MonoBehaviour
{
    private const string MatchPhaseWaitingForResultAck = "waiting_for_result_ack";
    private const string MatchPhaseWaitingForReplacement = "waiting_for_replacement";
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

            StartNextBattlePoll();
        }
    }

    private void StartNextBattlePoll()
    {
        StartCoroutine(PollForBattleResultViaMatchState());
    }

    private static bool IsBattleResultReady(MatchStateDto matchState)
    {
        if (matchState == null)
        {
            return false;
        }

        if (matchState.battle != null && matchState.battle.hasLastResult)
        {
            return true;
        }

        return string.Equals(matchState.phase, MatchPhaseWaitingForResultAck, System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(matchState.phase, MatchPhaseWaitingForReplacement, System.StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Polling battle resultu prebieha iba cez matchState, finálny payload sa doťahuje cez getBattleStatus.
    /// </summary>
    private IEnumerator PollForBattleResultViaMatchState()
    {
        yield return new WaitForSeconds(1f);

        pollAttempts++;

        if (pollAttempts >= MAX_POLL_ATTEMPTS)
        {
            Debug.LogError("[BattleSubmitter] Timeout waiting for opponent");
            isWaitingForBattle = false;
            yield break;
        }

        if (fightSystem?.multiplayerService == null)
        {
            Debug.LogError("[BattleSubmitter] MultiplayerService missing, aborting battle polling");
            isWaitingForBattle = false;
            yield break;
        }

        var matchStateTask = fightSystem.multiplayerService.GetMatchStateAsync(fightSystem.roomCode, fightSystem.myPlayerId);
        yield return new WaitUntil(() => matchStateTask.IsCompleted);

        MatchStateDto matchState = null;
        if (matchStateTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion)
        {
            matchState = matchStateTask.Result;
        }

        if (matchState == null)
        {
            Debug.LogWarning($"[BattleSubmitter] MatchState battle poll {pollAttempts}/{MAX_POLL_ATTEMPTS} returned null");

            StartCoroutine(PollForBattleResultViaMatchState());
            yield break;
        }

        Debug.Log($"[BattleSubmitter] MatchState battle poll {pollAttempts}/{MAX_POLL_ATTEMPTS}: phase={matchState.phase}, hasLastResult={matchState.battle?.hasLastResult ?? false}");

        if (IsBattleResultReady(matchState))
        {
            Debug.Log("[BattleSubmitter] MatchState indicates battle result is ready, fetching final battle payload");
            serverFunctionsManager.GetBattleStatus(fightSystem.roomCode, fightSystem.myPlayerId, result =>
            {
                HandlePolledBattleStatusResult(result);
            });
            yield break;
        }

        StartCoroutine(PollForBattleResultViaMatchState());
    }

    private void HandlePolledBattleStatusResult(PlayFab.CloudScriptModels.ExecuteFunctionResult result)
    {
        if (result == null || result.FunctionResult == null)
        {
            Debug.LogWarning("[BattleSubmitter] Battle status fetch returned null after retries; continuing polling instead of aborting flow");

            if (!isWaitingForBattle)
            {
                return;
            }

            StartCoroutine(PollForBattleResultViaMatchState());
            return;
        }

        OnBattleResponse(result);
    }
}

