public static class TutorialSessionState
{
    private static string cachedPlayerId;
    private static TutorialStateResponse cachedState;

    public static bool TryGet(string playerId, out TutorialStateResponse state)
    {
        bool matches = !string.IsNullOrWhiteSpace(playerId)
            && playerId == cachedPlayerId
            && cachedState != null
            && cachedState.success;

        state = matches ? cachedState : null;
        return matches;
    }

    public static void Store(string playerId, TutorialStateResponse state)
    {
        if (string.IsNullOrWhiteSpace(playerId) || state == null || !state.success)
        {
            return;
        }

        cachedPlayerId = playerId;
        cachedState = state;
    }

    public static void Clear()
    {
        cachedPlayerId = null;
        cachedState = null;
    }
}
