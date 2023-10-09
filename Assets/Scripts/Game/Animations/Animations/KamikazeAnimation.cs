using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class KamikazeAnimation : MonoBehaviour
{
    public Canvas canvas;

    public IEnumerator StartAnimation(Sprite planeSprite, Sprite explosionSprite, AudioClip sound, Transform attacker, Transform defender, Vector2 imageSize, Vector2 cardSize, float duration, bool isSuccessful, float moveDurationRatio = 0.5f, float explosionGrowDurationRatio = 0.2f, bool rotateTowardsTarget = true, float initialRotation = 0f, float finalRotation = 0f)
    {
        if (sound != null)
        {
            AudioSource.PlayClipAtPoint(sound, Camera.main.transform.position);
        }

        GameObject planeObject = new GameObject("PlaneSprite");
        planeObject.transform.SetParent(canvas.transform, false);
        Image planeImg = planeObject.AddComponent<Image>();
        planeImg.sprite = planeSprite;

        RectTransform planeRect = planeObject.GetComponent<RectTransform>();
        planeRect.sizeDelta = imageSize;
        planeObject.transform.position = attacker.position;

        Vector3 targetPosition;
        if (isSuccessful)
        {
            targetPosition = GetRandomPosition(defender.position, cardSize);
        }
        else
        {
            targetPosition = GetRandomPositionNearEdge(defender.position, cardSize, imageSize);
        }

        yield return StartCoroutine(AnimateMove(planeObject, attacker.position, targetPosition, duration * moveDurationRatio, rotateTowardsTarget, initialRotation, finalRotation));

        GameObject explosionObject = new GameObject("ExplosionSprite");
        explosionObject.transform.SetParent(canvas.transform, false);
        Image explosionImg = explosionObject.AddComponent<Image>();
        explosionImg.sprite = explosionSprite;
        explosionImg.color = new Color(1, 1, 1, 0);

        RectTransform explosionRect = explosionObject.GetComponent<RectTransform>();
        explosionRect.sizeDelta = imageSize;
        explosionObject.transform.position = planeObject.transform.position;

        Destroy(planeObject);

        yield return StartCoroutine(AnimateExplosion(explosionObject, imageSize, duration * explosionGrowDurationRatio));

        yield return new WaitForSeconds(duration * (1f - moveDurationRatio - explosionGrowDurationRatio));

        Destroy(explosionObject);
    }

    private Vector3 GetRandomPosition(Vector3 cardCenter, Vector2 cardSize)
    {
        float randomX = cardCenter.x + Random.Range(-cardSize.x / 2, cardSize.x / 2);
        float randomY = cardCenter.y + Random.Range(-cardSize.y / 2, cardSize.y / 2);

        return new Vector3(randomX, randomY, cardCenter.z);
    }

    private Vector3 GetRandomPositionNearEdge(Vector3 cardCenter, Vector2 cardSize, Vector2 imageSize)
    {
        float randomX = cardCenter.x + Random.Range(-cardSize.x / 2 - imageSize.x, cardSize.x / 2 + imageSize.x);
        float randomY = cardCenter.y + Random.Range(-cardSize.y / 2 - imageSize.y, cardSize.y / 2 + imageSize.y);

        float randomSide = Random.value;

        if (randomSide < 0.25f)
        {
            randomX = cardCenter.x + Random.Range(-cardSize.x / 2 - imageSize.x, -cardSize.x / 2);
        }
        else if (randomSide < 0.5f)
        {
            randomX = cardCenter.x + Random.Range(cardSize.x / 2, cardSize.x / 2 + imageSize.x);
        }
        else if (randomSide < 0.75f)
        {
            randomY = cardCenter.y + Random.Range(-cardSize.y / 2 - imageSize.y, -cardSize.y / 2);
        }
        else
        {
            randomY = cardCenter.y + Random.Range(cardSize.y / 2, cardSize.y / 2 + imageSize.y);
        }

        return new Vector3(randomX, randomY, cardCenter.z);
    }

    private IEnumerator AnimateMove(GameObject image, Vector3 startPoint, Vector3 endPoint, float duration, bool rotateTowardsTarget, float initialRotation, float finalRotation)
    {
        float elapsedTime = 0;
        Vector3 startingPos = startPoint;
        float startingRotationAngle = image.transform.eulerAngles.z;

        if (rotateTowardsTarget)
        {
            image.transform.up = endPoint - startPoint;
        }

        float rotationDifference = image.transform.eulerAngles.z - startingRotationAngle;
        image.transform.rotation *= Quaternion.Euler(0, 0, initialRotation);

        while (elapsedTime < duration)
        {
            image.transform.position = Vector3.Lerp(startingPos, endPoint, (elapsedTime / duration));
            float currentRotationAngle = Mathf.Lerp(startingRotationAngle + initialRotation, finalRotation + rotationDifference, (elapsedTime / duration));
            image.transform.eulerAngles = new Vector3(0, 0, currentRotationAngle);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.transform.position = endPoint;
        image.transform.eulerAngles = new Vector3(0, 0, finalRotation + rotationDifference);
    }

    private IEnumerator AnimateExplosion(GameObject explosion, Vector2 imageSize, float duration)
    {
        float elapsedTime = 0;
        RectTransform rectTransform = explosion.GetComponent<RectTransform>();
        Image explosionImg = explosion.GetComponent<Image>();

        while (elapsedTime < duration)
        {
            float scale = elapsedTime / duration;
            rectTransform.sizeDelta = imageSize * scale;
            explosionImg.color = new Color(1, 1, 1, scale);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.sizeDelta = imageSize;
        explosionImg.color = new Color(1, 1, 1, 1);
    }
}
