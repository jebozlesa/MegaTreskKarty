using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vizuálny indikátor pre kill counter - mení farbu Image komponentu
/// </summary>
public class KillCounterUI : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Zelená farba pre živý stav")]
    public Color aliveColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Zelená
    
    [Tooltip("Červená farba pre dead stav")]
    public Color deadColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Červená
    
    [Header("References")]
    private Image imageComponent;
    private bool isDead = false;
    
    void Awake()
    {
        imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {
            Debug.LogError($"[KillCounterUI] Missing Image component on {gameObject.name}!");
        }
    }
    
    void Start()
    {
        // Nastav na zelenú na začiatku
        SetAlive();
    }
    
    /// <summary>
    /// Nastav štvorček na živý stav (zelená)
    /// </summary>
    public void SetAlive()
    {
        if (imageComponent != null)
        {
            imageComponent.color = aliveColor;
            isDead = false;
            Debug.LogWarning($"[KillCounterUI] {gameObject.name} set to ALIVE (green)");
        }
    }
    
    /// <summary>
    /// Nastav štvorček na dead stav (červená)
    /// </summary>
    public void SetDead()
    {
        if (imageComponent != null)
        {
            imageComponent.color = deadColor;
            isDead = true;
            Debug.LogWarning($"[KillCounterUI] {gameObject.name} set to DEAD (red)");
        }
    }
    
    /// <summary>
    /// Skontroluj či je tento indikátor dead
    /// </summary>
    public bool IsDead => isDead;
}
