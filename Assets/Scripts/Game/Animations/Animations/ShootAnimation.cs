using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ShootAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartShootAnimation(Sprite shooterSprite, Sprite hitSprite, Transform shooterCard, Transform targetCard, Vector2 imageSize, float duration, float shootRatio = 0.5f, AudioClip shootSound = null, float recoilAngle = 10f, bool showHitImage = true, bool rotateToTarget = true, Vector2 hitImageSize = default)
    {
        if (shootSound)
        {
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position);
        }

        // Vytvorenie GameObjectu pre strelec
        GameObject shooterImageObject = new GameObject("ShooterImage");
        shooterImageObject.transform.SetParent(canvas.transform, false);
        Image shooterImg = shooterImageObject.AddComponent<Image>();
        shooterImg.sprite = shooterSprite;

        // Nastavenie veľkosti a pozície strelec
        RectTransform shooterRectTransform = shooterImageObject.GetComponent<RectTransform>();
        shooterRectTransform.sizeDelta = imageSize;
        shooterImageObject.transform.position = shooterCard.position;

        // Otočenie strelec smerom k cieľu
        if (rotateToTarget)
        {
            shooterImageObject.transform.up = targetCard.position - shooterCard.position;
        }

        // Čakanie na výstrel
        yield return new WaitForSeconds(duration * shootRatio);

        // Miknutie (recoil) zbrane
        shooterImageObject.transform.rotation *= Quaternion.Euler(0, 0, recoilAngle);
        yield return new WaitForSeconds(0.05f);
        shooterImageObject.transform.rotation *= Quaternion.Euler(0, 0, -recoilAngle);

        if (showHitImage)
        {
            // Vytvorenie GameObjectu pre zásah
            GameObject hitImageObject = new GameObject("HitImage");
            hitImageObject.transform.SetParent(canvas.transform, false);
            Image hitImg = hitImageObject.AddComponent<Image>();
            hitImg.sprite = hitSprite;

            // Nastavenie veľkosti a náhodnej pozície zásahu na cieľovej karte
            RectTransform hitRectTransform = hitImageObject.GetComponent<RectTransform>();
            hitRectTransform.sizeDelta = hitImageSize == default ? imageSize : hitImageSize;
            hitImageObject.transform.position = GetRandomPosition(targetCard.position, hitRectTransform.sizeDelta);

            // Čakanie na koniec animácie
            yield return new WaitForSeconds(duration * (1 - shootRatio));

            // Zničenie obrázku zásahu
            Destroy(hitImageObject);
        }

        // Zničenie obrázku strelec po skončení animácie
        Destroy(shooterImageObject);
    }

    private Vector3 GetRandomPosition(Vector3 cardCenter, Vector2 imageSize)
    {
        float randomX = cardCenter.x + Random.Range(-imageSize.x / 2, imageSize.x / 2);
        float randomY = cardCenter.y + Random.Range(-imageSize.y / 2, imageSize.y / 2);

        return new Vector3(randomX, randomY, cardCenter.z);
    }
}
