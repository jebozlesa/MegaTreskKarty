using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AirStrikeAnimation : MonoBehaviour
{
    public Canvas canvas;

    public IEnumerator StartAnimation(Sprite planeSprite, Sprite explosionSprite, AudioClip sound, Transform attacker, Transform defender, Vector2 imageSize, Vector2 cardSize, float duration, int showHitImageCount, float moveDurationRatio = 0.5f, float explosionGrowDurationRatio = 0.2f, bool rotateTowardsTarget = true, float initialRotation = 0f, float finalRotation = 0f)
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

        Vector3 targetPosition = defender.position - (defender.position - attacker.position) / 2; // Midpoint

        if (rotateTowardsTarget)
        {
            planeObject.transform.up = targetPosition - attacker.position;
        }

        planeObject.transform.rotation *= Quaternion.Euler(0, 0, initialRotation);

        yield return StartCoroutine(AnimateMove(planeObject, attacker.position, targetPosition, duration * moveDurationRatio));

        float timeBetweenHits = (duration * (1f - moveDurationRatio)) / showHitImageCount;

        for (int i = 0; i < showHitImageCount; i++)
        {
            GameObject explosionObject = new GameObject("ExplosionSprite");
            explosionObject.transform.SetParent(canvas.transform, false);
            Image explosionImg = explosionObject.AddComponent<Image>();
            explosionImg.sprite = explosionSprite;
            explosionImg.color = new Color(1, 1, 1, 0);

            RectTransform explosionRect = explosionObject.GetComponent<RectTransform>();
            explosionRect.sizeDelta = imageSize;
            explosionObject.transform.position = GetRandomPosition(defender.position, cardSize);

            yield return StartCoroutine(AnimateExplosion(explosionObject, imageSize, explosionGrowDurationRatio));

            yield return new WaitForSeconds(timeBetweenHits - explosionGrowDurationRatio);

            Destroy(explosionObject);
        }

        Destroy(planeObject);
    }

    private Vector3 GetRandomPosition(Vector3 cardCenter, Vector2 cardSize)
    {
        float randomX = cardCenter.x + Random.Range(-cardSize.x / 2, cardSize.x / 2);
        float randomY = cardCenter.y + Random.Range(-cardSize.y / 2, cardSize.y / 2);

        return new Vector3(randomX, randomY, cardCenter.z);
    }

    private IEnumerator AnimateMove(GameObject image, Vector3 startPoint, Vector3 endPoint, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            image.transform.position = Vector3.Lerp(startPoint, endPoint, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.transform.position = endPoint;
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
