using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BowShootAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartShootAnimation(Sprite bowSprite, Sprite arrowSprite, Transform shooterCard, Transform targetCard, Vector2 imageSize, float duration, AudioClip shootSound = null, bool showHitImage = true, bool rotateToTarget = true, Vector2 arrowImageSize = default)
    {
        if (shootSound)
        {
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position);
        }

        // Vytvorenie GameObjectu pre luk
        GameObject bowImageObject = new GameObject("BowImage");
        bowImageObject.transform.SetParent(canvas.transform, false);
        Image bowImg = bowImageObject.AddComponent<Image>();
        bowImg.sprite = bowSprite;

        // Nastavenie veľkosti a pozície luku
        RectTransform bowRectTransform = bowImageObject.GetComponent<RectTransform>();
        bowRectTransform.sizeDelta = imageSize;
        bowImageObject.transform.position = shooterCard.position;

        // Otočenie luku smerom k cieľu
        if (rotateToTarget)
        {
            bowImageObject.transform.up = targetCard.position - shooterCard.position;
        }

        yield return new WaitForSeconds(duration * 0.5f); // Čakanie na výstrel

        // Vytvorenie GameObjectu pre šíp
    GameObject arrowImageObject = new GameObject("ArrowImage");
    arrowImageObject.transform.SetParent(canvas.transform, false);
    Image arrowImg = arrowImageObject.AddComponent<Image>();
    arrowImg.sprite = arrowSprite;

    // Nastavenie veľkosti šípu
    RectTransform arrowRectTransform = arrowImageObject.GetComponent<RectTransform>();
    arrowRectTransform.sizeDelta = arrowImageSize == default ? imageSize : arrowImageSize;
    arrowImageObject.transform.position = shooterCard.position;

    Vector3 endPosition = showHitImage ? GetRandomPosition(targetCard.position, arrowRectTransform.sizeDelta) : GetRandomPositionNearEdge(targetCard.position, arrowRectTransform.sizeDelta, arrowRectTransform.sizeDelta);

    // Otočenie šípu smerom k jeho konečnému cieľu
    if (rotateToTarget)
    {
        arrowImageObject.transform.up = endPosition - shooterCard.position;
    }

    // Spustenie animácie šípu
    yield return StartCoroutine(AnimateMove(arrowImageObject, shooterCard.position, endPosition, duration * 0.5f));


        // Zničenie obrázkov po skončení animácie
        Destroy(bowImageObject);
        Destroy(arrowImageObject);
    }

    private Vector3 GetRandomPosition(Vector3 cardCenter, Vector2 imageSize)
    {
        float randomX = cardCenter.x + Random.Range(-imageSize.x / 2, imageSize.x / 2);
        float randomY = cardCenter.y + Random.Range(-imageSize.y / 2, imageSize.y / 2);

        return new Vector3(randomX, randomY, cardCenter.z);
    }

    private Vector3 GetRandomPositionNearEdge(Vector3 cardCenter, Vector2 cardSize, Vector2 imageSize)
    {
        // Predpokladajme, že veľkosť karty je definovaná ako cardSize

        // Získanie náhodnej pozície mimo karty
        float randomX = cardCenter.x + Random.Range(-cardSize.x / 2 - imageSize.x, cardSize.x / 2 + imageSize.x);
        float randomY = cardCenter.y + Random.Range(-cardSize.y / 2 - imageSize.y, cardSize.y / 2 + imageSize.y);

        // Vytvorenie náhodnej hodnoty pre rozhodnutie, či sa šíp zobrazí mimo hornej/dolnej alebo ľavej/pravej strany karty
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
}
