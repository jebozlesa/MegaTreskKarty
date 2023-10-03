using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MoveImage : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite sprite, Transform startPoint, Transform endPoint, Vector2 imageSize, float duration, AudioClip startSound = null, float initialRotation = 0f, float finalRotation = 0f, bool rotateTowardsTarget = true, float moveDurationRatio = 1f)
    {
        // Ak existuje zvuk pre začiatok, prehrať ho
        if (startSound != null)
        {
            AudioSource.PlayClipAtPoint(startSound, Camera.main.transform.position);
        }

        // Vytvorenie GameObjectu pre obrázok
        GameObject imageObject = new GameObject("MoveImageSprite");
        imageObject.transform.SetParent(canvas.transform, false); // Nastavenie ako dieťa Canvasu
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        // Nastavenie veľkosti obrázka
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = imageSize;

        // Nastavenie pozície obrázka na začiatočný bod
        imageObject.transform.position = startPoint.position;

        float originalRotation = imageObject.transform.eulerAngles.z;

        // Otočenie obrázka smerom k cieľu, ak je to požadované
        if (rotateTowardsTarget)
        {
            imageObject.transform.up = endPoint.position - startPoint.position;
        }

        float rotationDifference = imageObject.transform.eulerAngles.z - originalRotation;

        // Pridanie dodatočnej rotácie
        imageObject.transform.rotation *= Quaternion.Euler(0, 0, initialRotation);

        // Spustenie animácie
        yield return StartCoroutine(AnimateMove(imageObject, startPoint, endPoint, duration, finalRotation + rotationDifference, moveDurationRatio));

        // Zničenie obrázka po skončení animácie
        Destroy(imageObject, 0.1f);
    }

    private IEnumerator AnimateMove(GameObject image, Transform startPoint, Transform endPoint, float duration, float finalRotation, float moveDurationRatio)
    {
        float elapsedTime = 0;
        Vector3 startingPos = startPoint.position;
        float startingRotationAngle = image.transform.eulerAngles.z;

        float moveDuration = duration * moveDurationRatio; // Čas pohybu obrázka
        float stayDuration = duration - moveDuration; // Čas, počas ktorého obrázok zostane na mieste

        // Pohyb obrázka
        while (elapsedTime < moveDuration)
        {
            image.transform.position = Vector3.Lerp(startingPos, endPoint.position, (elapsedTime / moveDuration));
            float currentRotationAngle = Mathf.Lerp(startingRotationAngle, finalRotation, (elapsedTime / moveDuration));
            image.transform.eulerAngles = new Vector3(0, 0, currentRotationAngle);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Obrázok zostane na mieste
        image.transform.position = endPoint.position;
        image.transform.eulerAngles = new Vector3(0, 0, finalRotation);
        yield return new WaitForSeconds(stayDuration);
    }
}
