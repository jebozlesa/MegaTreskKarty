using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CorruptionAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite[] sprites, Transform targetCard, Vector2 cardSize, float duration, int numberOfImages, Vector2 imageSize, AudioClip soundEffect = null, float scatterSpeed = 1.0f, float maxRotationAngle = 30f)
    {
        // Ak existuje zvukový efekt, prehrať ho
        if (soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        List<GameObject> spawnedImages = new List<GameObject>();

        // Vytvorenie náhodných obrázkov
        for (int i = 0; i < numberOfImages; i++)
        {
            Sprite randomSprite = sprites[Random.Range(0, sprites.Length)];
            GameObject imageObject = new GameObject("ScatteredImage");
            imageObject.transform.SetParent(canvas.transform, false);
            Image img = imageObject.AddComponent<Image>();
            img.sprite = randomSprite;

            // Nastavenie veľkosti obrázka
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = imageSize;

            imageObject.transform.position = targetCard.position; // Obrázky vznikajú priamo v strede karty

            // Nastavenie náhodnej rotácie v zadanom rozmedzí
            float randomRotation = Random.Range(-maxRotationAngle, maxRotationAngle);
            imageObject.transform.rotation = Quaternion.Euler(0, 0, randomRotation);

            spawnedImages.Add(imageObject);

            // Rozptýlenie obrázka ihneď po jeho vytvorení
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
            StartCoroutine(ScatterImage(imageObject, randomDirection, scatterSpeed, duration));
        }

        yield return new WaitForSeconds(duration);
    }

    private IEnumerator ScatterImage(GameObject image, Vector3 direction, float speed, float duration)
    {
        float elapsedTime = 0;
        Vector3 startingPos = image.transform.position;

        while (elapsedTime < duration)
        {
            image.transform.position += direction * speed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(image);
    }
}
