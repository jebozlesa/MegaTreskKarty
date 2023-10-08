using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class BleedAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite[] sprites, Transform targetCard, Vector2 cardSize, float duration, int numberOfImages, Vector2 imageSize, AudioClip soundEffect = null, float scatterSpeed = 1.0f)
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
            // Aktualizácia pozície obrázka
            image.transform.position += direction * speed * Time.deltaTime;

            // Otočenie obrázka v smere pohybu
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            image.transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90°, pretože výchozí smer obrázka je hore

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(image);
    }
}
