using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RocketLaunchAnimation : MonoBehaviour
{
    public Canvas canvas;

    public IEnumerator StartRocketAnimation(Sprite rocketSprite, Sprite explosionSprite, Transform playerCard, Vector2 imageSize, AudioClip successSound, AudioClip failureSound, bool isSuccessfulLaunch = true, float duration = 1f)
    {
        GameObject rocketImageObject = new GameObject("RocketImage");
        rocketImageObject.transform.SetParent(canvas.transform, false);
        Image rocketImg = rocketImageObject.AddComponent<Image>();
        rocketImg.sprite = rocketSprite;

        RectTransform rocketRectTransform = rocketImageObject.GetComponent<RectTransform>();
        rocketRectTransform.sizeDelta = imageSize;
        rocketImageObject.transform.position = playerCard.position;

        // Play sound based on launch success at the start of the animation
        AudioSource.PlayClipAtPoint(isSuccessfulLaunch ? successSound : failureSound, Camera.main.transform.position);

        // Phase 1: Shake
        yield return StartCoroutine(ShakeImage(rocketImageObject, duration * 0.5f));

        if (isSuccessfulLaunch)
        {
            // Phase 2: Successful Launch
            yield return StartCoroutine(LaunchRocket(rocketImageObject, duration * 0.5f));
        }
        else
        {
            // Phase 2: Explosion
            rocketImg.sprite = explosionSprite;
            yield return StartCoroutine(ExpandExplosion(rocketImageObject, duration * 0.5f));
        }

        Destroy(rocketImageObject);
    }

    private IEnumerator ShakeImage(GameObject image, float duration)
    {
        Vector3 originalPosition = image.transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float x = originalPosition.x + Random.Range(-5f, 5f);
            float y = originalPosition.y + Random.Range(-5f, 5f);
            image.transform.position = new Vector3(x, y, originalPosition.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.transform.position = originalPosition;
    }

    private IEnumerator LaunchRocket(GameObject rocket, float duration)
    {
        Vector3 startPoint = rocket.transform.position;
        Vector3 endPoint = new Vector3(startPoint.x, startPoint.y + 1000f, startPoint.z); // 1000 units upward

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            rocket.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Pow((elapsedTime / duration), 0.5f)); // Slower start, faster end
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ExpandExplosion(GameObject explosion, float duration)
    {
        RectTransform rectTransform = explosion.GetComponent<RectTransform>();
        Vector2 startSize = rectTransform.sizeDelta;
        Vector2 endSize = startSize * 3f; // 3 times bigger

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
