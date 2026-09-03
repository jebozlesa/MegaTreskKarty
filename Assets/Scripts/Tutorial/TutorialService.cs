using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayFab.CloudScriptModels;
using UnityEngine;

public class TutorialService : MonoBehaviour
{
    [Header("Server Manager")]
    public ServerFunctionsManager serverFunctionsManager;

    public TutorialStateResponse CurrentState { get; private set; }

    public Task<TutorialStateResponse> GetTutorialStateAsync(string playerId)
    {
        return ExecuteAsync(
            callback => ResolveServerFunctionsManager()?.GetTutorialState(playerId, callback),
            "GetTutorialStateAsync"
        );
    }

    public async Task<TutorialStateResponse> CompleteStepAsync(
        string playerId,
        string tutorialId,
        string stepId
    )
    {
        string requestId = Guid.NewGuid().ToString();
        TutorialStateResponse state = await ExecuteAsync(
            callback => ResolveServerFunctionsManager()?.CompleteTutorialStep(
                playerId,
                tutorialId,
                stepId,
                requestId,
                callback
            ),
            "CompleteStepAsync"
        );

        return state;
    }

    public async Task<TutorialStateResponse> RefreshCurrentPlayerStateAsync()
    {
        string playerId = ResolvePlayerId();
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogWarning("[TutorialService] Cannot refresh tutorial state: playerId is empty");
            return null;
        }

        return await GetTutorialStateAsync(playerId);
    }

    public async Task<TutorialStateResponse> CompleteCurrentPlayerStepAsync(
        string tutorialId,
        string stepId
    )
    {
        string playerId = ResolvePlayerId();
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogWarning(
                $"[TutorialService] Cannot complete tutorial step: playerId is empty, tutorial={tutorialId}, step={stepId}"
            );
            return null;
        }

        return await CompleteStepAsync(playerId, tutorialId, stepId);
    }

    public bool ShouldShowFlow(string tutorialId)
    {
        return CurrentState != null
            && CurrentState.success
            && !CurrentState.IsFlowCompleted(tutorialId);
    }

    public bool HasCompletedStep(string tutorialId, string stepId)
    {
        return CurrentState != null
            && CurrentState.success
            && CurrentState.HasCompletedStep(tutorialId, stepId);
    }

    private async Task<TutorialStateResponse> ExecuteAsync(
        Action<Action<ExecuteFunctionResult>> request,
        string operationName
    )
    {
        var tcs = new TaskCompletionSource<TutorialStateResponse>();

        if (ResolveServerFunctionsManager() == null)
        {
            Debug.LogError($"[TutorialService] {operationName}: serverFunctionsManager is not assigned");
            tcs.TrySetResult(null);
            return await tcs.Task;
        }

        request(result =>
        {
            if (result?.FunctionResult == null)
            {
                Debug.LogWarning($"[TutorialService] {operationName}: no result returned from server");
                tcs.TrySetResult(null);
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(result.FunctionResult);
                TutorialStateResponse dto = JsonConvert.DeserializeObject<TutorialStateResponse>(json);
                CurrentState = dto;
                Debug.LogWarning(
                    $"[TutorialService] {operationName}: success={dto?.success}, route={dto?.recommendedRoute}, "
                        + $"safe={dto?.gates?.safeCardDeckState}, needsFirstPack={dto?.gates?.needsFirstPack}, "
                        + $"needsLibrarySwap={dto?.gates?.needsLibraryDeckSwap}, blockNavigation={dto?.blockNavigation}, "
                        + $"stage={dto?.stage}, error={dto?.error}"
                );
                tcs.TrySetResult(dto);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TutorialService] {operationName}: failed to parse response - {ex.Message}");
                tcs.TrySetResult(null);
            }
        });

        return await tcs.Task;
    }

    private ServerFunctionsManager ResolveServerFunctionsManager()
    {
        if (serverFunctionsManager != null)
        {
            return serverFunctionsManager;
        }

        serverFunctionsManager = GetComponent<ServerFunctionsManager>();
        if (serverFunctionsManager != null)
        {
            return serverFunctionsManager;
        }

        serverFunctionsManager = FindFirstObjectByType<ServerFunctionsManager>(FindObjectsInactive.Include);
        return serverFunctionsManager;
    }

    private static string ResolvePlayerId()
    {
        if (
            PlayFabManagerLogin.Instance != null
            && !string.IsNullOrWhiteSpace(PlayFabManagerLogin.Instance.LoggedInPlayerId)
        )
        {
            return PlayFabManagerLogin.Instance.LoggedInPlayerId;
        }

        return PlayerPrefs.GetString("LoggedInPlayerId", string.Empty);
    }
}
