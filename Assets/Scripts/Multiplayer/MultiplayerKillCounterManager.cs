using UnityEngine;

/// <summary>
/// Renders the server-owned multiplayer score.
/// </summary>
public class MultiplayerKillCounterManager : MonoBehaviour
{
    [Header("Player Kill Indicators (3 stvorceky)")]
    [Tooltip("Stvorceky pre player kills (kolko enemy kariet player zabil)")]
    public KillCounterUI[] playerKillIndicators = new KillCounterUI[3];

    [Header("Enemy Kill Indicators (3 stvorceky)")]
    [Tooltip("Stvorceky pre enemy kills (kolko player kariet enemy zabil)")]
    public KillCounterUI[] enemyKillIndicators = new KillCounterUI[3];

    private int playerKillCount;
    private int enemyKillCount;

    private const int KILLS_TO_WIN = 3;

    void Start()
    {
        ValidateIndicators();
        ResetKillCounts();
    }

    private void ValidateIndicators()
    {
        if (playerKillIndicators.Length != 3)
        {
            Debug.LogWarning(
                $"[KillCounterManager] Player kill indicators should be 3, but found {playerKillIndicators.Length}"
            );
        }
        if (enemyKillIndicators.Length != 3)
        {
            Debug.LogWarning(
                $"[KillCounterManager] Enemy kill indicators should be 3, but found {enemyKillIndicators.Length}"
            );
        }

        for (int i = 0; i < playerKillIndicators.Length; i++)
        {
            if (playerKillIndicators[i] == null)
            {
                Debug.LogError($"[KillCounterManager] Player kill indicator [{i}] is NULL!");
            }
        }
        for (int i = 0; i < enemyKillIndicators.Length; i++)
        {
            if (enemyKillIndicators[i] == null)
            {
                Debug.LogError($"[KillCounterManager] Enemy kill indicator [{i}] is NULL!");
            }
        }
    }

    /// <summary>
    /// Reset presentation while the initial server snapshot is loading.
    /// </summary>
    public void ResetKillCounts()
    {
        playerKillCount = 0;
        enemyKillCount = 0;

        RenderIndicators(playerKillIndicators, 0);
        RenderIndicators(enemyKillIndicators, 0);
    }

    public void RenderServerScore(MatchScoreDto score, string playerId)
    {
        if (score == null || score.version != 1 || score.target != KILLS_TO_WIN
            || score.byPlayerId == null || string.IsNullOrEmpty(playerId)
            || !score.byPlayerId.TryGetValue(playerId, out int playerScore))
        {
            Debug.LogError("[KillCounterManager] Cannot render an invalid server score snapshot");
            return;
        }

        bool opponentFound = false;
        int opponentScore = 0;
        foreach (var entry in score.byPlayerId)
        {
            if (entry.Key == playerId) continue;
            if (opponentFound)
            {
                Debug.LogError("[KillCounterManager] Server score contains more than one opponent");
                return;
            }
            opponentFound = true;
            opponentScore = entry.Value;
        }

        if (!opponentFound || playerScore < 0 || playerScore > KILLS_TO_WIN
            || opponentScore < 0 || opponentScore > KILLS_TO_WIN)
        {
            Debug.LogError("[KillCounterManager] Server score values are outside the supported 0-3 range");
            return;
        }

        playerKillCount = playerScore;
        enemyKillCount = opponentScore;
        RenderIndicators(playerKillIndicators, playerKillCount);
        RenderIndicators(enemyKillIndicators, enemyKillCount);
        Debug.LogWarning(
            $"[KillCounterManager] Server score rendered: player={playerKillCount}, enemy={enemyKillCount}"
        );
    }

    private static void RenderIndicators(KillCounterUI[] indicators, int deadCount)
    {
        if (indicators == null) return;
        for (int index = 0; index < indicators.Length; index++)
        {
            if (indicators[index] == null) continue;
            if (index < deadCount) indicators[index].SetDead();
            else indicators[index].SetAlive();
        }
    }

    /// <summary>
    /// Public getters for diagnostics and tests.
    /// </summary>
    public int PlayerKillCount => playerKillCount;
    public int EnemyKillCount => enemyKillCount;
}
