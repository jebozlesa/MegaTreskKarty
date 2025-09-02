using System;
using PlayFab;
using PlayFab.CloudScriptModels;
using UnityEngine;

public class ServerFunctionsManager : MonoBehaviour
{
    // Univerzálne volanie PlayFab funkcie
    public void CallFunction(string functionName, object parameters, Action<ExecuteFunctionResult> callback)
    {
        var request = new ExecuteFunctionRequest
        {
            FunctionName = functionName,
            FunctionParameter = parameters,
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request, result => {
            callback?.Invoke(result);
        }, error => {
            Debug.LogError(error.GenerateErrorReport());
            callback?.Invoke(null);
        });
    }

    // Príklad: pripojenie do miestnosti
    public void JoinOrCreateRoom(string roomCode, string playerId, Action<ExecuteFunctionResult> callback)
    {
        var parameters = new {
            roomCode = roomCode,
            playerId = playerId
        };
        CallFunction("joinOrCreateRoom", parameters, callback);
    }

    // Tu môžeš pridať ďalšie metódy pre iné serverové funkcie
}
