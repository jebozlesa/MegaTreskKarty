using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vizualny indikator pre kill counter - meni farbu Image komponentu
/// </summary>
public class KillCounterUI : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Zelena farba pre zivy stav")]
    public Color aliveColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Zelena
    
    [Tooltip("Cervena farba pre dead stav")]
    public Color deadColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Cervena
    
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
        // Nastav na zelenu na zaciatku
        SetAlive();
    }
    
    /// <summary>
    /// Nastav stvorcek na zivy stav (zelena)
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
    /// Nastav stvorcek na dead stav (cervena)
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
    /// Skontroluj ci je tento indikator dead
    /// </summary>
    public bool IsDead => isDead;
}
