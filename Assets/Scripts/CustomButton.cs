using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class CustomButton : MonoBehaviour
{
    public Color pressedColor = Color.yellow;
    public Image innerImage;
    public float delay = 0.7f;
    public UnityEvent onDelayedClick;

    private Button button;
    private Color originalColor = Color.white;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        StartCoroutine(ButtonClickEffect());
    }

    private IEnumerator ButtonClickEffect()
    {
        if (innerImage != null)
        {
            innerImage.color = pressedColor;
        }

        AudioManager.Instance.PlayButtonClickSound();

        yield return new WaitForSeconds(delay);

        onDelayedClick.Invoke(); // Vykoná akciu priradenú v Unity Editori

        if (innerImage != null)
        {
            innerImage.color = originalColor;
        }
    }
}
