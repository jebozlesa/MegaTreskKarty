using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum SoundEffectType
    {
        ButtonClick,
        Purchase,
        // MoneyAdded,
        // CardAcquired,
        // ... pridajte ďalšie typy zvukových efektov
    }

    public SoundEffectType soundEffectType = SoundEffectType.ButtonClick;
    public Color pressedColor = Color.yellow;
    public Image innerImage;
    public float delay = 0f;
    public UnityEvent onDelayedClick;

    private Button button;
    private Color originalColor;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (innerImage != null)
        {
            originalColor = innerImage.color; // Uloženie pôvodnej farby
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (innerImage != null)
        {
            innerImage.color = pressedColor;
        }
        PlaySoundEffect();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartCoroutine(ExecuteActionAfterDelay());
    }

    private IEnumerator ExecuteActionAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        onDelayedClick.Invoke();

        if (innerImage != null)
        {
            innerImage.color = originalColor;
        }
    }

    private void PlaySoundEffect()
    {
        switch (soundEffectType)
        {
            case SoundEffectType.ButtonClick:
                AudioManager.Instance.PlayButtonClickSound();
                break;
            case SoundEffectType.Purchase:
                AudioManager.Instance.PlayPurchaseSound();
                break;
                // ... pridajte ďalšie prípady
        }
    }
}
