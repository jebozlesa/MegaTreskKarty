using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RotatingEnlargeAnimation : MonoBehaviour
{
    public Canvas canvas;

    // Pridanie nového parametra 'initialRotation'
    public IEnumerator StartAnimation(Sprite sprite, Transform targetCard, Vector2 startSize, Vector2 endSize, float duration, float rotationSpeed, float initialRotation = 0f, AudioClip soundEffect = null)
    {
        // Ak existuje zvukový efekt, prehrať ho
        if (soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        // Vytvorenie GameObjectu pre obrázok
        GameObject imageObject = new GameObject("RotatingEnlargeImageSprite");
        imageObject.transform.SetParent(canvas.transform, false);
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        // Nastavenie počiatočnej veľkosti obrázka
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = startSize;

        // Nastavenie pozície obrázka na cieľovú kartu
        imageObject.transform.position = targetCard.position;

        // Nastavenie počiatočnej rotácie obrázka
        imageObject.transform.eulerAngles = new Vector3(0, 0, initialRotation);

        // Spustenie animácie zväčšenia a rotácie
        yield return StartCoroutine(AnimateEnlargeAndRotate(imageObject, startSize, endSize, duration, rotationSpeed));

        // Zničenie obrázka po skončení animácie
        Destroy(imageObject, 0.1f);
    }

    private IEnumerator AnimateEnlargeAndRotate(GameObject image, Vector2 startSize, Vector2 endSize, float duration, float rotationSpeed)
    {
        float elapsedTime = 0;
        RectTransform rectTransform = image.GetComponent<RectTransform>();

        while (elapsedTime < duration)
        {
            // Zväčšovanie obrázka
            rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, (elapsedTime / duration));

            // Rotácia obrázka
            image.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.sizeDelta = endSize;
    }
}
