using System;
using System.Threading.Tasks;
using PlayFab.CloudScriptModels;
using UnityEngine;

public class CampaignOnlineService : MonoBehaviour
{
    private static bool VerboseCampaignServiceLogs => true;

    [Header("Server Manager")]
    public ServerFunctionsManager serverFunctionsManager;

    private void Awake()
    {
        EnsureServerFunctionsManager();
    }

    public async Task<CampaignOnlineSessionEnvelopeDto> CreateSessionAsync(string playerId, string campaignId, int missionId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.CreateCampaignSession(playerId, campaignId, missionId, callback),
            CampaignOnlineSessionParser.ParseSessionEnvelope,
            "CreateSessionAsync"
        );
    }

    public async Task<CampaignOnlineProgressEnvelopeDto> GetProgressAsync(string playerId, string campaignId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.GetCampaignProgress(playerId, campaignId, callback),
            CampaignOnlineSessionParser.ParseProgressEnvelope,
            "GetProgressAsync"
        );
    }

    public async Task<CampaignOnlineSessionEnvelopeDto> LoadPlayerDeckAsync(string sessionId, string playerId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.LoadCampaignPlayerDeck(sessionId, playerId, callback),
            CampaignOnlineSessionParser.ParseSessionEnvelope,
            "LoadPlayerDeckAsync"
        );
    }

    public async Task<CampaignOnlineSessionEnvelopeDto> LoadEnemyDeckAsync(string sessionId, string playerId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.LoadCampaignEnemyDeck(sessionId, playerId, callback),
            CampaignOnlineSessionParser.ParseSessionEnvelope,
            "LoadEnemyDeckAsync"
        );
    }

    public async Task<CampaignOnlineSessionEnvelopeDto> SelectCardAsync(string sessionId, string playerId, string cardId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.SelectCampaignCard(sessionId, playerId, cardId, callback),
            CampaignOnlineSessionParser.ParseSessionEnvelope,
            "SelectCardAsync"
        );
    }

    public async Task<CampaignOnlineBattleEnvelopeDto> SubmitAttackAsync(string sessionId, string playerId, int attackSlot)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.SubmitCampaignAttack(sessionId, playerId, attackSlot, callback),
            CampaignOnlineSessionParser.ParseBattleEnvelope,
            "SubmitAttackAsync"
        );
    }

    private async Task<TDto> ExecuteAsync<TDto>(Action<Action<ExecuteFunctionResult>> request, Func<object, TDto> parser, string operationName)
        where TDto : class
    {
        var tcs = new TaskCompletionSource<TDto>();

        EnsureServerFunctionsManager();
        if (serverFunctionsManager == null)
        {
            Debug.LogError($"[CampaignOnlineService] {operationName}: serverFunctionsManager is null");
            tcs.TrySetResult(null);
            return await tcs.Task;
        }

        request(result =>
        {
            if (result?.FunctionResult == null)
            {
                Debug.LogWarning($"[CampaignOnlineService] {operationName}: No result returned from server");
                tcs.TrySetResult(null);
                return;
            }

            try
            {
                TDto dto = parser(result.FunctionResult);
                if (dto == null)
                {
                    Debug.LogWarning($"[CampaignOnlineService] {operationName}: Failed to parse server response");
                }
                else
                {
                    LogEnvelopeSuccess(operationName, dto);
                    LogEnvelopeWarnings(operationName, dto);
                }

                tcs.TrySetResult(dto);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CampaignOnlineService] {operationName}: Error parsing response - {ex.Message}");
                tcs.TrySetResult(null);
            }
        });

        return await tcs.Task;
    }

    private static void LogEnvelopeWarnings<TDto>(string operationName, TDto dto)
        where TDto : class
    {
        switch (dto)
        {
            case CampaignOnlineSessionEnvelopeDto sessionEnvelope:
                if (!sessionEnvelope.success || sessionEnvelope.session == null)
                {
                    Debug.LogWarning(
                        $"[CampaignOnlineService] {operationName}: success={sessionEnvelope.success}, " +
                        $"sessionNull={sessionEnvelope.session == null}, error={sessionEnvelope.error}, message={sessionEnvelope.message}"
                    );
                }
                break;

            case CampaignOnlineBattleEnvelopeDto battleEnvelope:
                if (!battleEnvelope.success || battleEnvelope.battleResult == null)
                {
                    Debug.LogWarning(
                        $"[CampaignOnlineService] {operationName}: success={battleEnvelope.success}, " +
                        $"battleResultNull={battleEnvelope.battleResult == null}, error={battleEnvelope.error}, message={battleEnvelope.message}, runStatus={battleEnvelope.runStatus}"
                    );
                }
                break;

            case CampaignOnlineProgressEnvelopeDto progressEnvelope:
                if (!progressEnvelope.success || progressEnvelope.progress == null)
                {
                    Debug.LogWarning(
                        $"[CampaignOnlineService] {operationName}: success={progressEnvelope.success}, " +
                        $"progressNull={progressEnvelope.progress == null}, error={progressEnvelope.error}, message={progressEnvelope.message}"
                    );
                }
                break;
        }
    }

    private static void LogEnvelopeSuccess<TDto>(string operationName, TDto dto)
        where TDto : class
    {
        if (!VerboseCampaignServiceLogs)
        {
            return;
        }

        switch (dto)
        {
            case CampaignOnlineSessionEnvelopeDto sessionEnvelope when sessionEnvelope.session != null:
                Debug.LogWarning(
                    $"[CampaignOnlineService] {operationName} OK: session={sessionEnvelope.session.sessionId}, status={sessionEnvelope.session.status}, " +
                    $"campaign={sessionEnvelope.session.modeConfig?.campaignId}, mission={sessionEnvelope.session.modeConfig?.missionId ?? 0}, " +
                    $"playerActive={sessionEnvelope.session.active?.playerCardId}, enemyActive={sessionEnvelope.session.active?.enemyCardId}, " +
                    $"turn={sessionEnvelope.session.progress?.turnNumber ?? 0}, battle={sessionEnvelope.session.progress?.battleCount ?? 0}, " +
                    $"playerDeck={sessionEnvelope.session.playerDeck?.cards?.Count ?? 0}, enemyDeck={sessionEnvelope.session.enemyDeck?.cards?.Count ?? 0}"
                );
                break;

            case CampaignOnlineBattleEnvelopeDto battleEnvelope:
                Debug.LogWarning(
                    $"[CampaignOnlineService] {operationName} OK: success={battleEnvelope.success}, runStatus={battleEnvelope.runStatus}, runEnded={battleEnvelope.runEnded}, " +
                    $"playerNeedsReplacement={battleEnvelope.playerNeedsReplacement}, enemyNeedsReplacement={battleEnvelope.enemyNeedsReplacement}, " +
                    $"botAttack={battleEnvelope.botAttack?.attackSlot}/{battleEnvelope.botAttack?.attackId}, " +
                    $"sessionStatus={battleEnvelope.session?.status}, turn={battleEnvelope.session?.progress?.turnNumber ?? 0}, battle={battleEnvelope.session?.progress?.battleCount ?? 0}"
                );
                break;

            case CampaignOnlineProgressEnvelopeDto progressEnvelope when progressEnvelope.progress != null:
                Debug.LogWarning(
                    $"[CampaignOnlineService] {operationName} OK: campaign={progressEnvelope.progress.campaignId}, " +
                    $"highestUnlockedMission={progressEnvelope.progress.highestUnlockedMissionId}, missionCount={progressEnvelope.progress.missionCount}"
                );
                break;
        }
    }

    private void EnsureServerFunctionsManager()
    {
        if (serverFunctionsManager != null)
        {
            return;
        }

        serverFunctionsManager = GetComponent<ServerFunctionsManager>();
        if (serverFunctionsManager != null)
        {
            return;
        }

        serverFunctionsManager = FindFirstObjectByType<ServerFunctionsManager>();
        if (serverFunctionsManager != null)
        {
            return;
        }

        serverFunctionsManager = gameObject.AddComponent<ServerFunctionsManager>();
        Debug.LogWarning("[CampaignOnlineService] Added missing ServerFunctionsManager to the current GameObject.");
    }
}
