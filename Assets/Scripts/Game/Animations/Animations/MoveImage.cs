using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MoveImage : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite sprite, Transform startPoint, Transform endPoint, Vector2 imageSize, float duration, AudioClip startSound, float initialRotation = 0f, float finalRotation = 0f)
{
    // Ak existuje zvuk pre začiatok, prehrať ho
    AudioSource.PlayClipAtPoint(startSound, Camera.main.transform.position);

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

    // Otočenie obrázka smerom k cieľu
    imageObject.transform.up = endPoint.position - startPoint.position;

    // Pridanie dodatočnej rotácie
    imageObject.transform.rotation *= Quaternion.Euler(0, 0, initialRotation);

    // Spustenie animácie
    yield return StartCoroutine(AnimateMove(imageObject, startPoint, endPoint, duration, finalRotation));

    // Zničenie obrázka po skončení animácie
    Destroy(imageObject, 0.1f);
}

private IEnumerator AnimateMove(GameObject image, Transform startPoint, Transform endPoint, float duration, float finalRotation)
{
    float elapsedTime = 0;
    Vector3 startingPos = startPoint.position;
    float startingRotationAngle = image.transform.eulerAngles.z;

    while (elapsedTime < duration)
    {
        image.transform.position = Vector3.Lerp(startingPos, endPoint.position, (elapsedTime / duration));
        
        // Lineárna interpolácia medzi začiatočným a konečným uhlom rotácie
        float currentRotationAngle = Mathf.Lerp(startingRotationAngle, finalRotation, (elapsedTime / duration));
        image.transform.eulerAngles = new Vector3(0, 0, currentRotationAngle);

        elapsedTime += Time.deltaTime;
        yield return null;
    }

    image.transform.position = endPoint.position;
    image.transform.eulerAngles = new Vector3(0, 0, finalRotation);
}

}
