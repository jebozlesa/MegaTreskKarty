using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AutoportraitAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite sprite, Transform targetCard, Vector2 imageSize, float duration, float moveWidth, float speed, float tiltAngle, AudioClip soundEffect = null)
    {
        // Ak existuje zvukový efekt, prehrať ho
        if (soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        // Vytvorenie GameObjectu pre obrázok
        GameObject imageObject = new GameObject("AutoportraitImage");
        imageObject.transform.SetParent(canvas.transform, false);
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        // Nastavenie veľkosti obrázka
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = imageSize;

        // Nastavenie pozície obrázka na cieľovú kartu
        imageObject.transform.position = targetCard.position;

        // Spustenie animácie
        yield return StartCoroutine(AnimateBrushStroke(imageObject, duration, moveWidth, speed, tiltAngle));

        // Zničenie obrázka po skončení animácie
        Destroy(imageObject, 0.1f);
    }

    private IEnumerator AnimateBrushStroke(GameObject image, float duration, float moveWidth, float speed, float tiltAngle)
    {
        float elapsedTime = 0;
        Vector3 originalPosition = image.transform.position;
        bool movingRight = true;

        while (elapsedTime < duration)
        {
            // Pohyb obrázka
            float moveAmount = speed * Time.deltaTime;
            if (movingRight)
            {
                image.transform.position += new Vector3(moveAmount, 0, 0);
                image.transform.eulerAngles = new Vector3(0, 0, -tiltAngle);
            }
            else
            {
                image.transform.position -= new Vector3(moveAmount, 0, 0);
                image.transform.eulerAngles = new Vector3(0, 0, tiltAngle);
            }

            // Zmena smeru pohybu, ak obrázok dosiahne hranice
            if (Mathf.Abs(image.transform.position.x - originalPosition.x) >= moveWidth)
            {
                movingRight = !movingRight;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Resetovanie pozície a rotácie obrázka na konci animácie
        image.transform.position = originalPosition;
        image.transform.eulerAngles = Vector3.zero;
    }
}
