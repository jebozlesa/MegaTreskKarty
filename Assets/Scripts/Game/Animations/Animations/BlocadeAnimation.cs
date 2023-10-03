using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BlocadeAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite sprite, Transform targetCard, Vector2 startSize, Vector2 endSize, float duration, float shakeDuration, float startRotation = 0f, float endRotation = 0f, AudioClip soundEffect = null)
    {
        // Ak existuje zvukový efekt, prehrať ho
        if (soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        // Vytvorenie GameObjectu pre obrázok
        GameObject imageObject = new GameObject("EnlargeImageSprite");
        imageObject.transform.SetParent(canvas.transform, false); // Nastavenie ako dieťa Canvasu
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        // Nastavenie počiatočnej veľkosti obrázka
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = startSize;

        // Nastavenie počiatočnej rotácie
        imageObject.transform.eulerAngles = new Vector3(0, 0, startRotation);

        // Nastavenie pozície obrázka na cieľovú kartu
        imageObject.transform.position = targetCard.position;

        // Spustenie animácie zväčšenia
        yield return StartCoroutine(AnimateEnlarge(imageObject, startSize, endSize, startRotation, endRotation, duration, shakeDuration));

        // Zničenie obrázka po skončení animácie
        Destroy(imageObject, 0.1f);
    }

    private IEnumerator AnimateEnlarge(GameObject image, Vector2 startSize, Vector2 endSize, float startRotation, float endRotation, float duration, float shakeDuration)
    {
        float elapsedTime = 0;
        RectTransform rectTransform = image.GetComponent<RectTransform>();

        float halfDuration = duration / 2f;
        Vector3 originalPosition = image.transform.position;

        while (elapsedTime < duration)
        {
            // Zväčšovanie a rotácia obrázka
            if (elapsedTime < halfDuration)
            {
                rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, (elapsedTime / halfDuration));
                float currentRotation = Mathf.Lerp(startRotation, endRotation, (elapsedTime / halfDuration));
                image.transform.eulerAngles = new Vector3(0, 0, currentRotation);
            }
            // Zmenšovanie a rotácia obrázka
            else
            {
                rectTransform.sizeDelta = Vector2.Lerp(endSize, startSize, ((elapsedTime - halfDuration) / halfDuration));
                float currentRotation = Mathf.Lerp(endRotation, startRotation, ((elapsedTime - halfDuration) / halfDuration));
                image.transform.eulerAngles = new Vector3(0, 0, currentRotation);
            }

            // Triasenie obrázka
            if (elapsedTime >= halfDuration - shakeDuration / 2f && elapsedTime <= halfDuration + shakeDuration / 2f)
            {
                float shakeAmount = 5f; // Môžete upraviť podľa potreby
                image.transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;
            }
            else
            {
                image.transform.position = originalPosition;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.sizeDelta = startSize;
        image.transform.eulerAngles = new Vector3(0, 0, startRotation);
        image.transform.position = originalPosition;
    }
}
