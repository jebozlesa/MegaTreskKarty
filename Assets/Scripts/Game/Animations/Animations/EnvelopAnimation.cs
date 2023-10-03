using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnvelopAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite[] sprites, Transform targetCard, Vector2 imageSize, float spawnDistance, float convergeDistance, float duration, int numberOfImages, AudioClip soundEffect = null)
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
            GameObject imageObject = new GameObject("ConvergeImage");
            imageObject.transform.SetParent(canvas.transform, false);
            Image img = imageObject.AddComponent<Image>();
            img.sprite = randomSprite;

            // Nastavenie veľkosti obrázka
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = imageSize;

            // Nastavenie pozície obrázka na náhodnú pozíciu v určenej vzdialenosti od karty
            Vector3 randomPosition = GetRandomPosition(targetCard.position, spawnDistance);
            imageObject.transform.position = randomPosition;

            spawnedImages.Add(imageObject);

            // Presun obrázka k stredu karty
            StartCoroutine(ConvergeImage(imageObject, targetCard.position, convergeDistance, duration));
        }

        yield return new WaitForSeconds(duration);

        foreach (var img in spawnedImages)
        {
            Destroy(img);
        }
    }

    private Vector3 GetRandomPosition(Vector3 cardCenter, float distance)
    {
        float randomAngle = Random.Range(0, 2 * Mathf.PI);
        float offsetX = distance * Mathf.Cos(randomAngle);
        float offsetY = distance * Mathf.Sin(randomAngle);

        return new Vector3(cardCenter.x + offsetX, cardCenter.y + offsetY, cardCenter.z);
    }

    private IEnumerator ConvergeImage(GameObject image, Vector3 targetPosition, float finalDistance, float duration)
    {
        Vector3 startingPos = image.transform.position;
        Vector3 finalPos = targetPosition + (startingPos - targetPosition).normalized * finalDistance;

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            // Kontrola, či objekt stále existuje
            if (image != null)
            {
                image.transform.position = Vector3.Lerp(startingPos, finalPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
            }
            else
            {
                // Ak objekt neexistuje, ukončíme Coroutine
                yield break;
            }
            yield return null;
        }

        // Kontrola, či objekt stále existuje predtým, ako ho zničíme
        if (image != null)
        {
            Destroy(image);
        }
    }

}
