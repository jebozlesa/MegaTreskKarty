using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlagWaveAnimation : MonoBehaviour
{
    public Canvas canvas;

    public IEnumerator StartFlagWaveAnimation(Sprite sprite, Transform targetCard, Vector2 startSize, Vector2 endSize, float duration, float minRotation, float maxRotation, float waveSpeed, AudioClip soundEffect = null)
    {
        if (soundEffect)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        GameObject imageObject = new GameObject("FlagWaveImageSprite");
        imageObject.transform.SetParent(canvas.transform, false);
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = startSize;
        rectTransform.pivot = new Vector2(0.5f, 0f); // Set pivot to bottom center
        imageObject.transform.position = targetCard.position;

        yield return StartCoroutine(AnimateFlagWave(imageObject, startSize, endSize, duration, minRotation, maxRotation, waveSpeed));

        Destroy(imageObject, 0.1f);
    }

    private IEnumerator AnimateFlagWave(GameObject image, Vector2 startSize, Vector2 endSize, float duration, float minRotation, float maxRotation, float waveSpeed)
    {
        float elapsedTime = 0;
        RectTransform rectTransform = image.GetComponent<RectTransform>();
        bool flip = false;

        while (elapsedTime < duration)
        {
            rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, (elapsedTime / duration));

            float wave = Mathf.Sin(elapsedTime * waveSpeed) * (maxRotation - minRotation) + minRotation;
            image.transform.eulerAngles = new Vector3(0, flip ? 180 : 0, wave);

            if (Mathf.Abs(wave) >= Mathf.Abs(maxRotation) - 1) // Near peak, flip image
            {
                flip = !flip;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.sizeDelta = endSize;
        image.transform.eulerAngles = new Vector3(0, flip ? 180 : 0, 0);
    }
}
