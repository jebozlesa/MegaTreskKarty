using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RisingEnlargeImage : MonoBehaviour
{
    public Canvas canvas;

    public IEnumerator StartRisingEnlargeAnimation(Sprite sprite, Transform targetCard, Vector2 startSize, Vector2 endSize, float duration, float riseSpeed, float startRotation = 0f, float endRotation = 0f, AudioClip soundEffect = null)
    {
        if (soundEffect)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        GameObject imageObject = new GameObject("RisingEnlargeImageSprite");
        imageObject.transform.SetParent(canvas.transform, false);
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = startSize;

        imageObject.transform.eulerAngles = new Vector3(0, 0, startRotation);
        imageObject.transform.position = targetCard.position;

        yield return StartCoroutine(AnimateRisingEnlarge(imageObject, startSize, endSize, riseSpeed, startRotation, endRotation, duration));

        Destroy(imageObject, 0.1f);
    }

    private IEnumerator AnimateRisingEnlarge(GameObject image, Vector2 startSize, Vector2 endSize, float riseSpeed, float startRotation, float endRotation, float duration)
    {
        float elapsedTime = 0;
        RectTransform rectTransform = image.GetComponent<RectTransform>();

        Vector3 startPosition = image.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, riseSpeed * duration, 0);

        while (elapsedTime < duration)
        {
            rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, (elapsedTime / duration));
            image.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / duration));

            float currentRotation = Mathf.Lerp(startRotation, endRotation, (elapsedTime / duration));
            image.transform.eulerAngles = new Vector3(0, 0, currentRotation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.sizeDelta = endSize;
        image.transform.position = endPosition;
        image.transform.eulerAngles = new Vector3(0, 0, endRotation);
    }
}
