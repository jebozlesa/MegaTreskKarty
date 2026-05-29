using System;
using System.Threading.Tasks;
using PlayFab.CloudScriptModels;
using UnityEngine;

public class RoyalRumbleService : MonoBehaviour
{
    private static bool VerboseRoyalRumbleServiceLogs => true;

    [Header("Server Manager")]
    public ServerFunctionsManager serverFunctionsManager;

    private void Awake()
    {
        EnsureServerFunctionsManager();
    }

    public async Task<RoyalRumbleSessionEnvelopeDto> CreateSessionAsync(string playerId, string username = null)
    {
        string resolvedUsername = string.IsNullOrWhiteSpace(username)
            ? PlayerPrefs.GetString("username", playerId)
            : username;

        return await ExecuteAsync(
            callback => serverFunctionsManager.CreateRoyalRumbleSession(playerId, resolvedUsername, callback),
            RoyalRumbleSessionParser.ParseSessionEnvelope,
            "CreateSessionAsync"
        );
    }

    public async Task<RoyalRumbleSessionEnvelopeDto> GetSessionAsync(string sessionId, string playerId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.GetRoyalRumbleSession(sessionId, playerId, callback),
            RoyalRumbleSessionParser.ParseSessionEnvelope,
            "GetSessionAsync"
        );
    }

    public async Task<RoyalRumbleSessionEnvelopeDto> LoadPlayerDeckAsync(string sessionId, string playerId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.LoadRoyalRumblePlayerDeck(sessionId, playerId, callback),
            RoyalRumbleSessionParser.ParseSessionEnvelope,
            "LoadPlayerDeckAsync"
        );
    }

    public async Task<RoyalRumbleSessionEnvelopeDto> LoadEnemyDeckAsync(string sessionId, string playerId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.LoadRoyalRumbleEnemyDeck(sessionId, playerId, callback),
            RoyalRumbleSessionParser.ParseSessionEnvelope,
            "LoadEnemyDeckAsync"
        );
    }

    public async Task<RoyalRumbleSessionEnvelopeDto> SelectCardAsync(string sessionId, string playerId, string cardId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.SelectRoyalRumbleCard(sessionId, playerId, cardId, callback),
            RoyalRumbleSessionParser.ParseSessionEnvelope,
            "SelectCardAsync"
        );
    }

    public async Task<RoyalRumbleBattleEnvelopeDto> SubmitAttackAsync(string sessionId, string playerId, int attackSlot)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.SubmitRoyalRumbleAttack(sessionId, playerId, attackSlot, callback),
            RoyalRumbleSessionParser.ParseBattleEnvelope,
            "SubmitAttackAsync"
        );
    }

    public async Task<RoyalRumbleSessionEnvelopeDto> AbandonSessionAsync(string sessionId, string playerId)
    {
        return await ExecuteAsync(
            callback => serverFunctionsManager.AbandonRoyalRumbleSession(sessionId, playerId, callback),
            RoyalRumbleSessionParser.ParseSessionEnvelope,
            "AbandonSessionAsync"
        );
    }

    public void AbandonSessionFireAndForget(string sessionId, string playerId)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        EnsureServerFunctionsManager();
        if (serverFunctionsManager == null)
        {
            return;
        }

        serverFunctionsManager.AbandonRoyalRumbleSession(sessionId, playerId, result =>
        {
            if (result?.FunctionResult == null)
            {
                Debug.LogWarning("[RoyalRumbleService] AbandonSessionFireAndForget: No result returned from server.");
                return;
            }

            try
            {
                RoyalRumbleSessionEnvelopeDto dto = RoyalRumbleSessionParser.ParseSessionEnvelope(result.FunctionResult);
                LogEnvelopeWarnings("AbandonSessionFireAndForget", dto);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RoyalRumbleService] AbandonSessionFireAndForget: Failed to parse response - {ex.Message}");
            }
        });
    }

    private async Task<TDto> ExecuteAsync<TDto>(Action<Action<ExecuteFunctionResult>> request, Func<object, TDto> parser, string operationName)
        where TDto : class
    {
        var tcs = new TaskCompletionSource<TDto>();

        EnsureServerFunctionsManager();

        if (serverFunctionsManager == null)
        {
            Debug.LogError($"[RoyalRumbleService] {operationName}: serverFunctionsManager is null");
            tcs.TrySetResult(null);
            return await tcs.Task;
        }

        request(result =>
        {
            if (result?.FunctionResult == null)
            {
                Debug.LogWarning($"[RoyalRumbleService] {operationName}: No result returned from server");
                tcs.TrySetResult(null);
                return;
            }

            try
            {
                TDto dto = parser(result.FunctionResult);
                if (dto == null)
                {
                    Debug.LogWarning($"[RoyalRumbleService] {operationName}: Failed to parse server response");
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
                Debug.LogError($"[RoyalRumbleService] {operationName}: Error parsing response - {ex.Message}");
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
            case RoyalRumbleSessionEnvelopeDto sessionEnvelope:
                if (!sessionEnvelope.success || sessionEnvelope.session == null)
                {
                    Debug.LogWarning(
                        $"[RoyalRumbleService] {operationName}: success={sessionEnvelope.success}, " +
                        $"sessionNull={sessionEnvelope.session == null}, error={sessionEnvelope.error}, message={sessionEnvelope.message}"
                    );
                }
                break;

            case RoyalRumbleBattleEnvelopeDto battleEnvelope:
                if (!battleEnvelope.success || battleEnvelope.battleResult == null)
                {
                    Debug.LogWarning(
                        $"[RoyalRumbleService] {operationName}: success={battleEnvelope.success}, " +
                        $"battleResultNull={battleEnvelope.battleResult == null}, error={battleEnvelope.error}, message={battleEnvelope.message}, runStatus={battleEnvelope.runStatus}"
                    );
                }
                break;
        }
    }

    private static void LogEnvelopeSuccess<TDto>(string operationName, TDto dto)
        where TDto : class
    {
        if (!VerboseRoyalRumbleServiceLogs)
        {
            return;
        }

        switch (dto)
        {
            case RoyalRumbleSessionEnvelopeDto sessionEnvelope when sessionEnvelope.session != null:
                Debug.LogWarning(
                    $"[RoyalRumbleService] {operationName} OK: session={sessionEnvelope.session.sessionId}, status={sessionEnvelope.session.status}, " +
                    $"playerActive={sessionEnvelope.session.active?.playerCardId}, enemyActive={sessionEnvelope.session.active?.enemyCardId}, " +
                    $"turn={sessionEnvelope.session.progress?.turnNumber ?? 0}, battle={sessionEnvelope.session.progress?.battleCount ?? 0}, " +
                    $"playerDeck={sessionEnvelope.session.playerDeck?.cards?.Count ?? 0}, enemyDeck={sessionEnvelope.session.enemyDeck?.cards?.Count ?? 0}"
                );
                break;

            case RoyalRumbleBattleEnvelopeDto battleEnvelope:
                Debug.LogWarning(
                    $"[RoyalRumbleService] {operationName} OK: success={battleEnvelope.success}, runStatus={battleEnvelope.runStatus}, runEnded={battleEnvelope.runEnded}, " +
                    $"playerNeedsReplacement={battleEnvelope.playerNeedsReplacement}, enemyNeedsReplacement={battleEnvelope.enemyNeedsReplacement}, " +
                    $"botAttack={battleEnvelope.botAttack?.attackSlot}/{battleEnvelope.botAttack?.attackId}, " +
                    $"sessionStatus={battleEnvelope.session?.status}, turn={battleEnvelope.session?.progress?.turnNumber ?? 0}, battle={battleEnvelope.session?.progress?.battleCount ?? 0}, " +
                    $"recordSync={battleEnvelope.recordSync?.score ?? 0}/pending={battleEnvelope.recordSync?.pending ?? false}"
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
        Debug.LogWarning("[RoyalRumbleService] Added missing ServerFunctionsManager to the current GameObject.");
    }
}
