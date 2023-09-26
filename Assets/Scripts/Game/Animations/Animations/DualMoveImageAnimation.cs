using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DualMoveImageAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite sprite1, Sprite sprite2, Transform startPoint, Transform endPoint, Vector2 imageSize, float duration, AudioClip startSound, float switchRatio = 0.5f, float switchDelay = 0.2f, float initialRotation = 0f, float finalRotation = 0f, bool rotateTowardsTarget = true)
    {
        

        // Ak existuje zvuk pre začiatok, prehrať ho
        if (startSound != null)
        {
            AudioSource.PlayClipAtPoint(startSound, Camera.main.transform.position);
        }

        // Vytvorenie GameObjectu pre prvý obrázok
        GameObject imageObject1 = new GameObject("ImageSprite1");
        imageObject1.transform.SetParent(canvas.transform, false);
        Image img1 = imageObject1.AddComponent<Image>();
        img1.sprite = sprite1;

        // Vytvorenie GameObjectu pre druhý obrázok (ale bude neviditeľný na začiatku)
        GameObject imageObject2 = new GameObject("ImageSprite2");
        imageObject2.transform.SetParent(canvas.transform, false);
        Image img2 = imageObject2.AddComponent<Image>();
        img2.sprite = sprite2;
        imageObject2.transform.localScale = Vector3.zero;

        // Nastavenie veľkosti obrázkov
        RectTransform rectTransform1 = imageObject1.GetComponent<RectTransform>();
        rectTransform1.sizeDelta = imageSize;
        RectTransform rectTransform2 = imageObject2.GetComponent<RectTransform>();
        rectTransform2.sizeDelta = imageSize;

        // Nastavenie pozície obrázkov na začiatočný bod
        imageObject1.transform.position = startPoint.position;
        imageObject2.transform.position = startPoint.position;

        // Otočenie obrázkov smerom k cieľu, ak je to požadované
        if (rotateTowardsTarget)
        {
            imageObject1.transform.up = endPoint.position - startPoint.position;
            imageObject2.transform.up = endPoint.position - startPoint.position;
        }

        // Pridanie dodatočnej rotácie
        imageObject1.transform.rotation *= Quaternion.Euler(0, 0, initialRotation);
        imageObject2.transform.rotation *= Quaternion.Euler(0, 0, initialRotation);

        // Spustenie animácie
        yield return StartCoroutine(AnimateMoveAndSwitch(imageObject1, imageObject2, startPoint, endPoint, duration, switchRatio, switchDelay, finalRotation));

        // Zničenie obrázkov po skončení animácie
        Destroy(imageObject1);
        Destroy(imageObject2);
    }

    private IEnumerator AnimateMoveAndSwitch(GameObject image1, GameObject image2, Transform startPoint, Transform endPoint, float duration, float switchRatio, float switchDelay, float finalRotation)
    {
        float elapsedTime = 0;
        Vector3 startingPos = startPoint.position;
        float startingRotationAngle = image1.transform.eulerAngles.z;

        float switchStart = duration * (switchRatio - switchDelay);
        float switchEnd = duration * (switchRatio + switchDelay);

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;

            // Pohyb obrázkov
            Vector3 currentPos = Vector3.Lerp(startingPos, endPoint.position, progress);
            image1.transform.position = currentPos;
            image2.transform.position = currentPos;

            // Rotácia obrázkov
            float currentRotationAngle = Mathf.Lerp(startingRotationAngle, finalRotation, progress);
            image1.transform.eulerAngles = new Vector3(0, 0, currentRotationAngle);
            image2.transform.eulerAngles = new Vector3(0, 0, currentRotationAngle);

            // Zmena obrázkov na základe switchRatio a switchDelay
            if (progress > switchStart && progress < switchEnd)
            {
                float switchProgress = (progress - switchStart) / (switchEnd - switchStart);
                image1.transform.localScale = Vector3.one * (1 - switchProgress);
                image2.transform.localScale = Vector3.one * switchProgress;
            }
            else if (progress >= switchEnd)
            {
                image1.transform.localScale = Vector3.zero;
                image2.transform.localScale = Vector3.one;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image1.transform.position = endPoint.position;
        image2.transform.position = endPoint.position;
        image1.transform.eulerAngles = new Vector3(0, 0, finalRotation);
        image2.transform.eulerAngles = new Vector3(0, 0, finalRotation);
    }

}
