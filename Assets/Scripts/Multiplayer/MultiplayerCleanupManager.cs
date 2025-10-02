using UnityEngine;
using System.Collections;

/// <summary>
/// Globálny manager pre cleanup starých multiplayer miestností
/// Pridajte tento script do hlavného menu alebo ako singleton
/// </summary>
public class MultiplayerCleanupManager : MonoBehaviour
{
    [Header("Cleanup Settings")]
    [SerializeField] private float cleanupInterval = 300f; // 5 minút
    [SerializeField] private bool autoCleanupOnStart = true;
    [SerializeField] private bool periodicCleanup = true;
    
    [Header("References")]
    public ServerFunctionsManager serverFunctionsManager;
    
    private Coroutine cleanupCoroutine;
    private static MultiplayerCleanupManager instance;
    
    public static MultiplayerCleanupManager Instance => instance;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (autoCleanupOnStart)
        {
            // Okamžitý cleanup pri štarte
            PerformCleanup();
        }
        
        if (periodicCleanup)
        {
            // Spustenie periodického cleanup
            StartPeriodicCleanup();
        }
    }

    void OnDestroy()
    {
        if (cleanupCoroutine != null)
        {
            StopCoroutine(cleanupCoroutine);
        }
    }

    public void PerformCleanup()
    {
        if (serverFunctionsManager != null)
        {
            Debug.Log("Performing multiplayer rooms cleanup...");
            serverFunctionsManager.CleanupRooms(result =>
            {
                if (result != null && result.FunctionResult != null)
                {
                    try
                    {
                        var resultObj = Newtonsoft.Json.Linq.JObject.Parse(result.FunctionResult.ToString());
                        if (resultObj["removedPlayers"] != null || resultObj["removedRooms"] != null)
                        {
                            int removedPlayers = resultObj["removedPlayers"]?.ToObject<int>() ?? 0;
                            int removedRooms = resultObj["removedRooms"]?.ToObject<int>() ?? 0;
                            Debug.Log($"Cleanup completed: {removedPlayers} players, {removedRooms} rooms removed");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Could not parse cleanup result: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("Cleanup request failed");
                }
            });
        }
        else
        {
            Debug.LogWarning("ServerFunctionsManager not found for cleanup");
        }
    }

    public void StartPeriodicCleanup()
    {
        if (cleanupCoroutine != null)
        {
            StopCoroutine(cleanupCoroutine);
        }
        cleanupCoroutine = StartCoroutine(PeriodicCleanupLoop());
        Debug.Log($"Started periodic cleanup every {cleanupInterval} seconds");
    }

    public void StopPeriodicCleanup()
    {
        if (cleanupCoroutine != null)
        {
            StopCoroutine(cleanupCoroutine);
            cleanupCoroutine = null;
            Debug.Log("Stopped periodic cleanup");
        }
    }

    IEnumerator PeriodicCleanupLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(cleanupInterval);
            PerformCleanup();
        }
    }

    // === VEREJNÉ METÓDY PRE UI ===
    
    [ContextMenu("Manual Cleanup")]
    public void ManualCleanup()
    {
        PerformCleanup();
    }

    public void SetCleanupInterval(float seconds)
    {
        cleanupInterval = seconds;
        if (periodicCleanup && cleanupCoroutine != null)
        {
            // Reštartuj s novým intervalom
            StartPeriodicCleanup();
        }
    }

    // === INTEGRÁCIA S APLIKAČNÝMI EVENTAMI ===
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // Aplikácia sa obnovuje - urob cleanup
            PerformCleanup();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            // Aplikácia získava focus - urob cleanup
            PerformCleanup();
        }
    }
}