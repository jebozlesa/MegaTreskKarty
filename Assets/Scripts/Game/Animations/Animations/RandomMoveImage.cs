using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RandomMoveImage : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartRandomMoveAnimation(Sprite sprite, Transform sourceCard, Transform targetCard, Vector2 cardSize, int numberOfAttacks, Vector2 imageSize, float duration, AudioClip startSound, float initialRotation = 0f, float finalRotation = 0f)
    {
        AudioSource.PlayClipAtPoint(startSound, Camera.main.transform.position);
        for (int i = 0; i < numberOfAttacks; i++)
        {
            Vector3 randomStartPosition = GetRandomPosition(sourceCard.position, cardSize);
            Vector3 randomEndPosition = GetRandomPosition(targetCard.position, cardSize);

            // Calculate the final rotation as a difference from the initial rotation
            float finalRotationDifference = finalRotation - initialRotation;

            StartCoroutine(StartAnimation(sprite, randomStartPosition, randomEndPosition, imageSize, duration, startSound, initialRotation, finalRotationDifference));
        }

        yield return new WaitForSeconds(duration + 0.1f); // Wait for all animations to finish
    }

    private Vector3 GetRandomPosition(Vector3 cardCenter, Vector2 cardSize)
    {
        float randomX = cardCenter.x + Random.Range(-cardSize.x / 2, cardSize.x / 2);
        float randomY = cardCenter.y + Random.Range(-cardSize.y / 2, cardSize.y / 2);

        return new Vector3(randomX, randomY, cardCenter.z);
    }

    private IEnumerator StartAnimation(Sprite sprite, Vector3 startPoint, Vector3 endPoint, Vector2 imageSize, float duration, AudioClip startSound, float initialRotation, float finalRotationDifference)
    {

        GameObject imageObject = new GameObject("RandomMoveImageSprite");
        imageObject.transform.SetParent(canvas.transform, false);
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = imageSize;

        imageObject.transform.position = startPoint;
        imageObject.transform.up = endPoint - startPoint;
        imageObject.transform.rotation = Quaternion.Euler(0, 0, initialRotation);

        yield return StartCoroutine(AnimateMove(imageObject, startPoint, endPoint, duration, initialRotation, finalRotationDifference));

        Destroy(imageObject, 0.1f);
    }

    private IEnumerator AnimateMove(GameObject image, Vector3 startPoint, Vector3 endPoint, float duration, float initialRotation, float finalRotationDifference)
    {
        float elapsedTime = 0;
        Vector3 startingPos = startPoint;
        float finalRotation = initialRotation + finalRotationDifference;

        while (elapsedTime < duration)
        {
            image.transform.position = Vector3.Lerp(startingPos, endPoint, (elapsedTime / duration));

            // Lineárna interpolácia medzi začiatočnou a konečnou rotáciou
            float currentRotation = Mathf.Lerp(initialRotation, finalRotation, (elapsedTime / duration));
            image.transform.rotation = Quaternion.Euler(0, 0, currentRotation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.transform.position = endPoint;
        image.transform.rotation = Quaternion.Euler(0, 0, finalRotation);
    }
}
