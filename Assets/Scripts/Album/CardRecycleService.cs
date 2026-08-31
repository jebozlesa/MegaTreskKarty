using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayFab.CloudScriptModels;
using UnityEngine;

public class CardRecycleService : MonoBehaviour
{
    [Header("Server Manager")]
    public ServerFunctionsManager serverFunctionsManager;

    public Task<CardRecycleResponse> RecycleCardAsync(
        string playerId,
        string cardId,
        string requestId
    )
    {
        return ExecuteAsync(
            callback => serverFunctionsManager.RecyclePlayerCard(playerId, cardId, requestId, callback),
            "RecycleCardAsync"
        );
    }

    private async Task<CardRecycleResponse> ExecuteAsync(
        Action<Action<ExecuteFunctionResult>> request,
        string operationName
    )
    {
        var tcs = new TaskCompletionSource<CardRecycleResponse>();

        if (serverFunctionsManager == null)
        {
            Debug.LogError($"[CardRecycleService] {operationName}: serverFunctionsManager is not assigned");
            tcs.TrySetResult(null);
            return await tcs.Task;
        }

        request(result =>
        {
            if (result?.FunctionResult == null)
            {
                Debug.LogWarning($"[CardRecycleService] {operationName}: No result returned from server");
                tcs.TrySetResult(null);
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(result.FunctionResult);
                CardRecycleResponse dto = JsonConvert.DeserializeObject<CardRecycleResponse>(json);
                tcs.TrySetResult(dto);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CardRecycleService] {operationName}: Failed to parse response - {ex.Message}");
                tcs.TrySetResult(null);
            }
        });

        return await tcs.Task;
    }
}
