using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayFab.CloudScriptModels;
using UnityEngine;

public class LibraryDeckService : MonoBehaviour
{
    [Header("Server Manager")]
    public ServerFunctionsManager serverFunctionsManager;

    public Task<LibraryDeckStateResponse> GetLibraryDeckStateAsync(string playerId)
    {
        return ExecuteAsync<LibraryDeckStateResponse>(
            callback => serverFunctionsManager.GetLibraryDeckState(playerId, callback),
            "GetLibraryDeckStateAsync"
        );
    }

    public Task<LibraryDeckMutationResponse> CreateDeckAsync(string playerId, string contextId)
    {
        return ExecuteAsync<LibraryDeckMutationResponse>(
            callback => serverFunctionsManager.CreateLibraryDeck(playerId, contextId, callback),
            "CreateDeckAsync"
        );
    }

    public Task<LibraryDeckMutationResponse> SetActiveDeckAsync(
        string playerId,
        string contextId,
        string deckId
    )
    {
        return ExecuteAsync<LibraryDeckMutationResponse>(
            callback => serverFunctionsManager.SetActiveLibraryDeck(playerId, contextId, deckId, callback),
            "SetActiveDeckAsync"
        );
    }

    public Task<LibraryDeckMutationResponse> SwapDeckCardAsync(
        string playerId,
        string contextId,
        string deckId,
        string oldCardId,
        string newCardId
    )
    {
        return ExecuteAsync<LibraryDeckMutationResponse>(
            callback => serverFunctionsManager.SwapLibraryDeckCard(
                playerId,
                contextId,
                deckId,
                oldCardId,
                newCardId,
                callback
            ),
            "SwapDeckCardAsync"
        );
    }

    private async Task<TDto> ExecuteAsync<TDto>(
        Action<Action<ExecuteFunctionResult>> request,
        string operationName
    )
        where TDto : class
    {
        var tcs = new TaskCompletionSource<TDto>();

        if (serverFunctionsManager == null)
        {
            Debug.LogError($"[LibraryDeckService] {operationName}: serverFunctionsManager is not assigned");
            tcs.TrySetResult(null);
            return await tcs.Task;
        }

        request(result =>
        {
            if (result?.FunctionResult == null)
            {
                Debug.LogWarning($"[LibraryDeckService] {operationName}: No result returned from server");
                tcs.TrySetResult(null);
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(result.FunctionResult);
                TDto dto = JsonConvert.DeserializeObject<TDto>(json);
                tcs.TrySetResult(dto);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LibraryDeckService] {operationName}: Failed to parse response - {ex.Message}");
                tcs.TrySetResult(null);
            }
        });

        return await tcs.Task;
    }
}
