using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlagWaveAnimation : MonoBehaviour
{
    public Canvas canvas;

    public IEnumerator StartFlagWaveAnimation(Sprite sprite, Transform targetCard, Vector2 imageSize, float duration, float minRotation, float maxRotation, float waveSpeed, AudioClip soundEffect = null)
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
        rectTransform.sizeDelta = imageSize;
        rectTransform.pivot = new Vector2(0.5f, 0f); // Set pivot to bottom center
        imageObject.transform.position = targetCard.position;

        yield return StartCoroutine(AnimateFlagWave(imageObject, duration, minRotation, maxRotation, waveSpeed));

        Destroy(imageObject, 0.1f);
    }

    private IEnumerator AnimateFlagWave(GameObject image, float duration, float minRotation, float maxRotation, float waveSpeed)
    {
        float elapsedTime = 0;
        bool flip = false;

        while (elapsedTime < duration)
        {
            float wave = Mathf.PingPong(elapsedTime * waveSpeed, maxRotation - minRotation) + minRotation;
            image.transform.eulerAngles = new Vector3(0, flip ? 180 : 0, wave);

            // Flip image when it reaches a peak
            if (wave <= minRotation || wave >= maxRotation)
            {
                flip = !flip;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.transform.eulerAngles = new Vector3(0, flip ? 180 : 0, minRotation);
    }
}
