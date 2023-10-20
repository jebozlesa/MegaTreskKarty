using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MidpointRotatingEnlargeAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    // Metóda pre spustenie animácie, teraz prijíma dva parametre pre útočníka a obrancu
    public IEnumerator StartAnimation(Sprite sprite, Transform attacker, Transform defender, Vector2 startSize, Vector2 endSize, float duration, float rotationSpeed, float initialRotation = 0f, AudioClip soundEffect = null)
    {
        // Ak existuje zvukový efekt, prehrať ho
        if (soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        // Vytvorenie GameObjectu pre obrázok
        GameObject imageObject = new GameObject("MidpointRotatingEnlargeImageSprite");
        imageObject.transform.SetParent(canvas.transform, false); // Nastavenie ako dieťa Canvasu
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        // Nastavenie počiatočnej veľkosti obrázka
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = startSize;

        // Výpočet stredovej pozície medzi útočníkom a obrancom
        Vector3 midpoint = (attacker.position + defender.position) / 2.0f;
        imageObject.transform.position = midpoint;

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
