using UnityEngine;

public static class MultiplayerMissingRoomNoticeState
{
    public const string PlayerIdKey = "PendingMultiplayerMissingRoomNoticePlayerId";

    public static void Queue(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("[MissingRoomNotice] Cannot queue a notice without a player ID.");
            return;
        }

        PlayerPrefs.SetString(PlayerIdKey, playerId);
        PlayerPrefs.Save();
    }

    public static bool Consume(string playerId)
    {
        string queuedPlayerId = PlayerPrefs.GetString(PlayerIdKey, string.Empty);
        if (string.IsNullOrEmpty(queuedPlayerId))
        {
            return false;
        }

        PlayerPrefs.DeleteKey(PlayerIdKey);
        PlayerPrefs.Save();

        if (queuedPlayerId == playerId)
        {
            return true;
        }

        Debug.LogWarning(
            $"[MissingRoomNotice] Discarded stale notice for another account: queued={queuedPlayerId}, current={playerId}"
        );
        return false;
    }
}
