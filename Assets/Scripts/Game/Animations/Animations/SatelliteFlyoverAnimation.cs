using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SatelliteFlyoverAnimation : MonoBehaviour
{
    public Canvas canvas;

    public IEnumerator StartAnimation(Sprite satelliteSprite, Transform playerCard, Vector2 imageSize, float duration, AudioClip flyoverSound, float distanceAboveCard, float travelDistance)
    {
        GameObject satelliteImageObject = new GameObject("SatelliteImage");
        satelliteImageObject.transform.SetParent(canvas.transform, false);
        Image satelliteImg = satelliteImageObject.AddComponent<Image>();
        satelliteImg.sprite = satelliteSprite;

        RectTransform satelliteRectTransform = satelliteImageObject.GetComponent<RectTransform>();
        satelliteRectTransform.sizeDelta = imageSize;

        Vector3 startPosition = playerCard.position + new Vector3(travelDistance, distanceAboveCard, 0);
        Vector3 endPosition = playerCard.position + new Vector3(-travelDistance, distanceAboveCard, 0);

        satelliteImageObject.transform.position = startPosition;

        AudioSource.PlayClipAtPoint(flyoverSound, Camera.main.transform.position);

        yield return StartCoroutine(MoveSatellite(satelliteImageObject, startPosition, endPosition, duration));

        Destroy(satelliteImageObject);
    }

    private IEnumerator MoveSatellite(GameObject satellite, Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            satellite.transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
