using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RandomImageSpawner : MonoBehaviour
{
    public Canvas canvas;

    public IEnumerator StartRandomSpawnAnimation(Sprite[] sprites, Transform targetCard, Vector2 cardSize, float duration, int totalImages, int spawnIntensity = 3, Vector2 startSize = default, Vector2 endSize = default, float imageLifetime = 0.2f, AudioClip soundEffect = null)
    {
        if (soundEffect)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        float spawnInterval = duration / (totalImages / (float)spawnIntensity);
        float elapsedTime = 0;

        int spawnedImages = 0;

        while (elapsedTime < duration && spawnedImages < totalImages)
        {
            for (int i = 0; i < spawnIntensity && spawnedImages < totalImages; i++)
            {
                Sprite randomSprite = sprites[Random.Range(0, sprites.Length)];
                Vector3 randomPosition = GetRandomPosition(targetCard.position, cardSize);
                StartCoroutine(SpawnAndEnlargeImage(randomSprite, randomPosition, startSize, endSize, imageLifetime, soundEffect));
                spawnedImages++;
            }

            elapsedTime += spawnInterval;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private Vector3 GetRandomPosition(Vector3 cardCenter, Vector2 cardSize)
    {
        float randomX = cardCenter.x + Random.Range(-cardSize.x / 2, cardSize.x / 2) * 1.1f;
        float randomY = cardCenter.y + Random.Range(-cardSize.y / 2, cardSize.y / 2) * 1.1f;

        return new Vector3(randomX, randomY, cardCenter.z);
    }

    private IEnumerator SpawnAndEnlargeImage(Sprite sprite, Vector3 position, Vector2 startSize, Vector2 endSize, float imageLifetime, AudioClip soundEffect = null)
    {
        GameObject imageObject = new GameObject("RandomSpawnImage");
        imageObject.transform.SetParent(canvas.transform, false);
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = startSize;

        imageObject.transform.position = position;

        float elapsedTime = 0;
        while (elapsedTime < imageLifetime)
        {
            rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, elapsedTime / imageLifetime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(imageObject);
    }
}
