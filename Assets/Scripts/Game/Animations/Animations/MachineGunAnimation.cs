using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineGunAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartShootAnimation(Sprite shooterSprite, Sprite hitSprite, Transform shooterCard, Transform targetCard, Vector2 imageSize, float duration, float shootRatio = 0.5f, float shootRatioEnd = 0.5f, AudioClip shootSound = null, float recoilAngle = 10f, int showHitImageCount = 0, bool rotateToTarget = true, Vector2 hitImageSize = default)
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

        // Začiatok triasenia samopalom
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(ShakeGun(shooterImageObject, duration - 0.4f, recoilAngle));

        // Čakanie na výstrel
        yield return new WaitForSeconds(duration * shootRatio);

        float timeBetweenHits = (duration * shootRatioEnd - duration * shootRatio) / showHitImageCount;

        for (int i = 0; i < showHitImageCount; i++)
        {
            // Vytvorenie GameObjectu pre zásah
            GameObject hitImageObject = new GameObject("HitImage" + i);
            hitImageObject.transform.SetParent(canvas.transform, false);
            Image hitImg = hitImageObject.AddComponent<Image>();
            hitImg.sprite = hitSprite;

            // Nastavenie veľkosti a náhodnej pozície zásahu na cieľovej karte
            RectTransform hitRectTransform = hitImageObject.GetComponent<RectTransform>();
            hitRectTransform.sizeDelta = hitImageSize == default ? imageSize : hitImageSize;
            hitImageObject.transform.position = GetRandomPosition(targetCard.position, hitRectTransform.sizeDelta);

            // Čakanie na náhodný čas medzi shootRatio a shootRatioEnd pred zobrazením ďalšieho zásahu
            yield return new WaitForSeconds(timeBetweenHits);
        }

        // Čakanie na koniec animácie
        yield return new WaitForSeconds(duration - duration * shootRatioEnd);

        // Zničenie všetkých obrázkov zásahu
        for (int i = 0; i < showHitImageCount; i++)
        {
            Destroy(GameObject.Find("HitImage" + i));
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

    private IEnumerator ShakeGun(GameObject gun, float duration, float angle)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            gun.transform.rotation *= Quaternion.Euler(0, 0, angle);
            yield return new WaitForSeconds(0.05f);
            gun.transform.rotation *= Quaternion.Euler(0, 0, -angle);
            yield return new WaitForSeconds(0.05f);
            elapsedTime += 0.1f;
        }
    }
}
