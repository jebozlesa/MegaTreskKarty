using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manager pre sledovanie kill countov v multiplayer battle
/// Tracks koľko kariet zabil player vs enemy
/// Win condition: Prvý kto zabije 3 karty súpera VYHRÁVA
/// </summary>
public class MultiplayerKillCounterManager : MonoBehaviour
{
    [Header("Player Kill Indicators (3 štvorčeky)")]
    [Tooltip("Štvorčeky pre player kills (koľko enemy kariet player zabil)")]
    public KillCounterUI[] playerKillIndicators = new KillCounterUI[3];
    
    [Header("Enemy Kill Indicators (3 štvorčeky)")]
    [Tooltip("Štvorčeky pre enemy kills (koľko player kariet enemy zabil)")]
    public KillCounterUI[] enemyKillIndicators = new KillCounterUI[3];
    
    [Header("Game Systems")]
    public FightSystemMultiplayer fightSystem;
    
    // Kill counts
    private int playerKillCount = 0; // Koľko enemy kariet player zabil
    private int enemyKillCount = 0;  // Koľko player kariet enemy zabil
    
    private const int KILLS_TO_WIN = 3;
    
    void Awake()
    {
        Debug.LogWarning("[KillCounterManager] 🔧🔧🔧 AWAKE CALLED - SCRIPT IS ALIVE! 🔧🔧🔧");
        
        if (fightSystem == null)
        {
            Debug.LogWarning("[KillCounterManager] Fight system not set, searching...");
            fightSystem = FindFirstObjectByType<FightSystemMultiplayer>();
            if (fightSystem == null)
            {
                Debug.LogError("[KillCounterManager] ❌ FightSystemMultiplayer not found in scene!");
            }
            else
            {
                Debug.LogWarning($"[KillCounterManager] ✅ Found FightSystemMultiplayer: {fightSystem.name}");
            }
        }
    }
    
    void Start()
    {
        Debug.LogWarning("[KillCounterManager] ⚡ START() CALLED - Initializing...");
        
        // Validácia
        ValidateIndicators();
        
        // Reset na začiatku
        ResetKillCounts();
        
        Debug.LogWarning("[KillCounterManager] ✅ Initialized. Win condition: First to 3 kills wins!");
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
    /// Reset kill counts na začiatku hry
    /// </summary>
    public void ResetKillCounts()
    {
        playerKillCount = 0;
        enemyKillCount = 0;
        
        // Nastav všetky štvorčeky na zelenú
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
    /// Volané keď player zabije enemy kartu
    /// </summary>
    public void OnPlayerKilledEnemyCard()
    {
        if (playerKillCount >= KILLS_TO_WIN)
        {
            Debug.LogWarning("[KillCounterManager] Player already won, ignoring kill");
            return;
        }
        
        playerKillCount++;
        Debug.LogWarning($"[KillCounterManager] 💀 Player killed enemy card! Count: {playerKillCount}/{KILLS_TO_WIN}");
        
        // Nastav príslušný štvorček na červenú
        int indicatorIndex = playerKillCount - 1; // 0-based index
        if (indicatorIndex < playerKillIndicators.Length && playerKillIndicators[indicatorIndex] != null)
        {
            playerKillIndicators[indicatorIndex].SetDead();
        }
        
        // Skontroluj win condition
        CheckWinCondition();
    }
    
    /// <summary>
    /// Volané keď enemy zabije player kartu
    /// </summary>
    public void OnEnemyKilledPlayerCard()
    {
        if (enemyKillCount >= KILLS_TO_WIN)
        {
            Debug.LogWarning("[KillCounterManager] Enemy already won, ignoring kill");
            return;
        }
        
        enemyKillCount++;
        Debug.LogWarning($"[KillCounterManager] 💀 Enemy killed player card! Count: {enemyKillCount}/{KILLS_TO_WIN}");
        
        // Nastav príslušný štvorček na červenú
        int indicatorIndex = enemyKillCount - 1; // 0-based index
        if (indicatorIndex < enemyKillIndicators.Length && enemyKillIndicators[indicatorIndex] != null)
        {
            enemyKillIndicators[indicatorIndex].SetDead();
        }
        
        // Skontroluj win condition
        CheckWinCondition();
    }
    
    /// <summary>
    /// Skontroluj win condition (prvý na 3 kills vyhráva)
    /// </summary>
    private void CheckWinCondition()
    {
        if (playerKillCount >= KILLS_TO_WIN)
        {
            // ✅ PLAYER WINS!
            Debug.LogWarning($"[KillCounterManager] 🎉 PLAYER WINS! Killed {playerKillCount} enemy cards!");
            
            if (fightSystem != null)
            {
                fightSystem.state = FightStateMultiplayer.WON;
                Debug.LogWarning($"[FightSystemMultiplayer] Fight state changed to WON (player killed {playerKillCount} cards)");
                
                // Zobraz victory message
                if (fightSystem.dialogText != null)
                {
                    fightSystem.dialogText.text = "You won";
                }
                
                // Deaktivuj attack buttony - hra skončila
                DisableAttackButtons();
                
                // Po 2 sekundách prejdi na menu
                StartCoroutine(ReturnToMenuAfterDelay(2f));
            }
            else
            {
                Debug.LogError("[KillCounterManager] ⚠️ fightSystem reference is NULL! Cannot set WON state!");
            }
        }
        else if (enemyKillCount >= KILLS_TO_WIN)
        {
            // ❌ ENEMY WINS!
            Debug.LogWarning($"[KillCounterManager] 💀 ENEMY WINS! Killed {enemyKillCount} player cards!");
            
            if (fightSystem != null)
            {
                fightSystem.state = FightStateMultiplayer.LOST;
                Debug.LogWarning($"[FightSystemMultiplayer] Fight state changed to LOST (enemy killed {enemyKillCount} cards)");
                
                // Zobraz defeat message
                if (fightSystem.dialogText != null)
                {
                    fightSystem.dialogText.text = "You lost";
                }
                
                // Deaktivuj attack buttony - hra skončila
                DisableAttackButtons();
                
                // Po 2 sekundách prejdi na menu
                StartCoroutine(ReturnToMenuAfterDelay(2f));
            }
            else
            {
                Debug.LogError("[KillCounterManager] ⚠️ fightSystem reference is NULL! Cannot set LOST state!");
            }
        }
    }
    
    /// <summary>
    /// Po zadanom čase sa vráť do hlavného menu
    /// </summary>
    private IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        Debug.LogWarning($"[KillCounterManager] ⏱️ Returning to menu in {delay} seconds...");
        yield return new WaitForSeconds(delay);
        
        Debug.LogWarning("[KillCounterManager] 🏠 Loading Main Menu scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
    
    /// <summary>
    /// Deaktivuje attack buttony po skončení hry
    /// </summary>
    private void DisableAttackButtons()
    {
        if (fightSystem == null) return;
        
        // Deaktivuj všetky attack buttony
        if (fightSystem.button1 != null) fightSystem.button1.interactable = false;
        if (fightSystem.button2 != null) fightSystem.button2.interactable = false;
        if (fightSystem.button3 != null) fightSystem.button3.interactable = false;
        if (fightSystem.button4 != null) fightSystem.button4.interactable = false;
        
        Debug.LogWarning("[KillCounterManager] ✅ Attack buttons disabled - game over!");
    }
    
    /// <summary>
    /// Public getters pre debugging
    /// </summary>
    public int PlayerKillCount => playerKillCount;
    public int EnemyKillCount => enemyKillCount;
}
