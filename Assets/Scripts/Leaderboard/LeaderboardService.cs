using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayFab.CloudScriptModels;
using UnityEngine;

public class LeaderboardService : MonoBehaviour
{
    public ServerFunctionsManager serverFunctionsManager;

    public Task<LeaderboardStateResponse> GetLeaderboardStateAsync(string playerId, string mode)
    {
        return ExecuteAsync(
            callback => serverFunctionsManager.GetLeaderboardState(playerId, mode, callback),
            $"GetLeaderboardStateAsync({mode})"
        );
    }

    private async Task<LeaderboardStateResponse> ExecuteAsync(
        Action<Action<ExecuteFunctionResult>> request,
        string operationName
    )
    {
        var completion = new TaskCompletionSource<LeaderboardStateResponse>();
        if (serverFunctionsManager == null)
        {
            Debug.LogError($"[LeaderboardService] {operationName}: serverFunctionsManager is not assigned");
            completion.TrySetResult(null);
            return await completion.Task;
        }

        request(result =>
        {
            if (result?.FunctionResult == null)
            {
                Debug.LogWarning($"[LeaderboardService] {operationName}: No result returned from server");
                completion.TrySetResult(null);
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(result.FunctionResult);
                completion.TrySetResult(JsonConvert.DeserializeObject<LeaderboardStateResponse>(json));
            }
            catch (Exception error)
            {
                Debug.LogError(
                    $"[LeaderboardService] {operationName}: Failed to parse response - {error.Message}"
                );
                completion.TrySetResult(null);
            }
        });
        return await completion.Task;
    }
}
