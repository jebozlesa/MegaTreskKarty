using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class AmbushAnimation : MonoBehaviour
{
    public Canvas canvas;

    public IEnumerator StartAnimation(Sprite[] sprites, Transform targetCard, Vector2 cardSize, float duration, int numberOfImages, Vector2 imageSize, AudioClip soundEffect = null, float gatherSpeed = 1.0f, float maxRotationAngle = 30f)
    {
        if (soundEffect != null)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        List<GameObject> spawnedImages = new List<GameObject>();

        for (int i = 0; i < numberOfImages; i++)
        {
            Sprite randomSprite = sprites[i % sprites.Length]; // Choose sprite
            GameObject imageObject = new GameObject("GatheringImage");
            imageObject.transform.SetParent(canvas.transform, false);
            Image img = imageObject.AddComponent<Image>();
            img.sprite = randomSprite;

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = imageSize;

            // Calculate the spawn position based on the angle
            float angle = i * 360f / numberOfImages;
            Vector3 spawnPosition = GetPositionAroundCircle(targetCard.position, cardSize, angle);

            imageObject.transform.position = spawnPosition;

            float randomRotation = Random.Range(-maxRotationAngle, maxRotationAngle);
            imageObject.transform.rotation = Quaternion.Euler(0, 0, randomRotation);

            spawnedImages.Add(imageObject);

            StartCoroutine(GatherImage(imageObject, targetCard.position, gatherSpeed, duration));
        }

        yield return new WaitForSeconds(duration);

        foreach (var image in spawnedImages)
        {
            Destroy(image);
        }
    }

    private Vector3 GetPositionAroundCircle(Vector3 center, Vector2 cardSize, float angle)
    {
        float radius = Mathf.Max(cardSize.x, cardSize.y) * 0.5f;
        float adjustedAngle = angle + 180f;  // Pridanie 180 stupňov k uhlu
        float radian = adjustedAngle * Mathf.Deg2Rad;
        float x = center.x + radius * Mathf.Sin(radian);
        float y = center.y + radius * Mathf.Cos(radian);

        return new Vector3(x, y, center.z);
    }

    private IEnumerator GatherImage(GameObject image, Vector3 targetPosition, float speed, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            image.transform.position = Vector3.MoveTowards(image.transform.position, targetPosition, speed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
