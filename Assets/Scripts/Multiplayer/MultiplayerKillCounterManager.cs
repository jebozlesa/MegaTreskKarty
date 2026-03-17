using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manager pre sledovanie kill countov v multiplayer battle
/// Tracks kolko kariet zabil player vs enemy
/// Win condition: Prvy kto zabije 3 karty supera VYHRAVA
/// </summary>
public class MultiplayerKillCounterManager : MonoBehaviour
{
    [Header("Player Kill Indicators (3 stvorceky)")]
    [Tooltip("Stvorceky pre player kills (kolko enemy kariet player zabil)")]
    public KillCounterUI[] playerKillIndicators = new KillCounterUI[3];
    
    [Header("Enemy Kill Indicators (3 stvorceky)")]
    [Tooltip("Stvorceky pre enemy kills (kolko player kariet enemy zabil)")]
    public KillCounterUI[] enemyKillIndicators = new KillCounterUI[3];
    
    [Header("Game Systems")]
    public FightSystemMultiplayer fightSystem;
    
    // Kill counts
    private int playerKillCount = 0; // Kolko enemy kariet player zabil
    private int enemyKillCount = 0;  // Kolko player kariet enemy zabil
    
    private const int KILLS_TO_WIN = 3;
    
    void Awake()
    {
        Debug.LogWarning("[KillCounterManager] [INIT][INIT][INIT] AWAKE CALLED - SCRIPT IS ALIVE! [INIT][INIT][INIT]");
        
        if (fightSystem == null)
        {
            Debug.LogWarning("[KillCounterManager] Fight system not set, searching...");
            fightSystem = FindFirstObjectByType<FightSystemMultiplayer>();
            if (fightSystem == null)
            {
                Debug.LogError("[KillCounterManager] [ERR] FightSystemMultiplayer not found in scene!");
            }
            else
            {
                Debug.LogWarning($"[KillCounterManager] [OK] Found FightSystemMultiplayer: {fightSystem.name}");
            }
        }
    }
    
    void Start()
    {
        Debug.LogWarning("[KillCounterManager] [START] START() CALLED - Initializing...");
        
        // Validacia
        ValidateIndicators();
        
        // Reset na zaciatku
        ResetKillCounts();
        
        Debug.LogWarning("[KillCounterManager] [OK] Initialized. Win condition: First to 3 kills wins!");
    }
    
    private void ValidateIndicators()
    {
        if (playerKillIndicators.Length != 3)
        {
            Debug.LogWarning($"[KillCounterManager] Player kill indicators should be 3, but found {playerKillIndicators.Length}");
        }
        if (enemyKillIndicators.Length != 3)
        {
            Debug.LogWarning($"[KillCounterManager] Enemy kill indicators should be 3, but found {enemyKillIndicators.Length}");
        }
        
        // Skontroluj null references
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
    /// Reset kill counts na zaciatku hry
    /// </summary>
    public void ResetKillCounts()
    {
        playerKillCount = 0;
        enemyKillCount = 0;
        
        // Nastav vsetky stvorceky na zelenu
        foreach (var indicator in playerKillIndicators)
        {
            if (indicator != null) indicator.SetAlive();
        }
        foreach (var indicator in enemyKillIndicators)
        {
            if (indicator != null) indicator.SetAlive();
        }
        
        Debug.Log("[KillCounterManager] Kill counts reset to 0-0");
    }
    
    /// <summary>
    /// Volane ked player zabije enemy kartu
    /// </summary>
    public void OnPlayerKilledEnemyCard()
    {
        if (playerKillCount >= KILLS_TO_WIN)
        {
            Debug.LogWarning("[KillCounterManager] Player already won, ignoring kill");
            return;
        }
        
        playerKillCount++;
        Debug.LogWarning($"[KillCounterManager] [DEAD] Player killed enemy card! Count: {playerKillCount}/{KILLS_TO_WIN}");
        
        // Nastav prislusny stvorcek na cervenu
        int indicatorIndex = playerKillCount - 1; // 0-based index
        if (indicatorIndex < playerKillIndicators.Length && playerKillIndicators[indicatorIndex] != null)
        {
            playerKillIndicators[indicatorIndex].SetDead();
        }
        
        // Skontroluj win condition
        CheckWinCondition();
    }
    
    /// <summary>
    /// Volane ked enemy zabije player kartu
    /// </summary>
    public void OnEnemyKilledPlayerCard()
    {
        if (enemyKillCount >= KILLS_TO_WIN)
        {
            Debug.LogWarning("[KillCounterManager] Enemy already won, ignoring kill");
            return;
        }
        
        enemyKillCount++;
        Debug.LogWarning($"[KillCounterManager] [DEAD] Enemy killed player card! Count: {enemyKillCount}/{KILLS_TO_WIN}");
        
        // Nastav prislusny stvorcek na cervenu
        int indicatorIndex = enemyKillCount - 1; // 0-based index
        if (indicatorIndex < enemyKillIndicators.Length && enemyKillIndicators[indicatorIndex] != null)
        {
            enemyKillIndicators[indicatorIndex].SetDead();
        }
        
        // Skontroluj win condition
        CheckWinCondition();
    }
    
    /// <summary>
    /// Skontroluj win condition (prvy na 3 kills vyhrava)
    /// </summary>
    private void CheckWinCondition()
    {
        if (playerKillCount >= KILLS_TO_WIN)
        {
            // [OK] PLAYER WINS!
            Debug.LogWarning($"[KillCounterManager] [WIN] PLAYER WINS! Killed {playerKillCount} enemy cards!");
            
            if (fightSystem != null)
            {
                fightSystem.state = FightStateMultiplayer.WON;
                Debug.LogWarning($"[FightSystemMultiplayer] Fight state changed to WON (player killed {playerKillCount} cards)");
                
                // Zobraz victory message
                if (fightSystem.dialogText != null)
                {
                    fightSystem.dialogText.text = "You won";
                }
                
                // Deaktivuj attack buttony - hra skoncila
                DisableAttackButtons();
                
                // Po 2 sekundach prejdi na menu
                StartCoroutine(ReturnToMenuAfterDelay(2f));
            }
            else
            {
                Debug.LogError("[KillCounterManager] [WARN] fightSystem reference is NULL! Cannot set WON state!");
            }
        }
        else if (enemyKillCount >= KILLS_TO_WIN)
        {
            // [ERR] ENEMY WINS!
            Debug.LogWarning($"[KillCounterManager] [DEAD] ENEMY WINS! Killed {enemyKillCount} player cards!");
            
            if (fightSystem != null)
            {
                fightSystem.state = FightStateMultiplayer.LOST;
                Debug.LogWarning($"[FightSystemMultiplayer] Fight state changed to LOST (enemy killed {enemyKillCount} cards)");
                
                // Zobraz defeat message
                if (fightSystem.dialogText != null)
                {
                    fightSystem.dialogText.text = "You lost";
                }
                
                // Deaktivuj attack buttony - hra skoncila
                DisableAttackButtons();
                
                // Po 2 sekundach prejdi na menu
                StartCoroutine(ReturnToMenuAfterDelay(2f));
            }
            else
            {
                Debug.LogError("[KillCounterManager] [WARN] fightSystem reference is NULL! Cannot set LOST state!");
            }
        }
    }
    
    /// <summary>
    /// Po zadanom case sa vrat do hlavneho menu
    /// </summary>
    private IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        Debug.LogWarning($"[KillCounterManager]  Returning to menu in {delay} seconds...");
        yield return new WaitForSeconds(delay);
        
        Debug.LogWarning("[KillCounterManager] [HOME] Loading Main Menu scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
    
    /// <summary>
    /// Deaktivuje attack buttony po skonceni hry
    /// </summary>
    private void DisableAttackButtons()
    {
        if (fightSystem == null) return;
        
        // Deaktivuj vsetky attack buttony
        if (fightSystem.button1 != null) fightSystem.button1.interactable = false;
        if (fightSystem.button2 != null) fightSystem.button2.interactable = false;
        if (fightSystem.button3 != null) fightSystem.button3.interactable = false;
        if (fightSystem.button4 != null) fightSystem.button4.interactable = false;
        
        Debug.LogWarning("[KillCounterManager] [OK] Attack buttons disabled - game over!");
    }
    
    /// <summary>
    /// Public getters pre debugging
    /// </summary>
    public int PlayerKillCount => playerKillCount;
    public int EnemyKillCount => enemyKillCount;
}
