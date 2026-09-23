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

    public async Task<TutorialStateResponse> GetTutorialStateAsync(string playerId)
    {
        TutorialStateResponse state = await ExecuteAsync(
            callback => ResolveServerFunctionsManager()?.GetTutorialState(playerId, callback),
            "GetTutorialStateAsync"
        );
        CacheSuccessfulState(playerId, state);
        return state;
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

        CacheSuccessfulState(playerId, state);
        return state;
    }

    public Task<TutorialStateResponse> GetCurrentPlayerStateAsync()
    {
        string playerId = ResolvePlayerId();
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogWarning("[TutorialService] Cannot get tutorial state: playerId is empty");
            return Task.FromResult<TutorialStateResponse>(null);
        }

        if (TutorialSessionState.TryGet(playerId, out TutorialStateResponse cachedState))
        {
            CurrentState = cachedState;
            return Task.FromResult(cachedState);
        }

        return GetTutorialStateAsync(playerId);
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
                if (dto == null || dto.tutorialContractVersion != TutorialConstants.ContractVersion)
                {
                    int actualVersion = dto?.tutorialContractVersion ?? 0;
                    dto ??= new TutorialStateResponse();
                    dto.success = false;
                    dto.stage = "contract_version";
                    dto.error = $"Expected tutorial contract {TutorialConstants.ContractVersion}, received {actualVersion}";
                    Debug.LogError($"[TutorialService] {operationName}: {dto.error}");
                }
                CurrentState = dto;
                Debug.LogWarning(
                    $"[TutorialService] {operationName}: success={dto?.success}, contract={dto?.tutorialContractVersion}, route={dto?.recommendedRoute}, "
                        + $"safe={dto?.gates?.safeCardDeckState}, needsFirstPack={dto?.gates?.needsFirstPack}, "
                        + $"needsLibrarySwap={dto?.gates?.needsLibraryDeckSwap}, blockNavigation={dto?.blockNavigation}, "
                        + $"libraryStatus={dto?.libraryTutorial?.status}, libraryShouldRun={dto?.libraryTutorial?.shouldRun}, "
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

    private void CacheSuccessfulState(string playerId, TutorialStateResponse state)
    {
        if (state == null || !state.success)
        {
            return;
        }

        CurrentState = state;
        TutorialSessionState.Store(playerId, state);
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
