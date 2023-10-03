using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WirelessChargerAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite sprite1, Sprite sprite2, Transform position, Vector2 imageSize, float duration, AudioClip startSound, float switchRatio = 1f, float fadeDurationRatio = 0.2f)
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
        img2.color = new Color(1, 1, 1, 0); // Nastavenie neviditeľnosti

        // Nastavenie veľkosti obrázkov
        RectTransform rectTransform1 = imageObject1.GetComponent<RectTransform>();
        rectTransform1.sizeDelta = imageSize;
        RectTransform rectTransform2 = imageObject2.GetComponent<RectTransform>();
        rectTransform2.sizeDelta = imageSize;

        // Nastavenie pozície obrázkov
        imageObject1.transform.position = position.position;
        imageObject2.transform.position = position.position;

        // Čakanie na zmenu obrázka
        yield return new WaitForSeconds(duration * switchRatio);

        // Zpriehladnenie prvého obrázka a zviditeľnenie druhého obrázka
        float fadeDuration = duration * fadeDurationRatio;
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            float alpha = 1 - (elapsedTime / fadeDuration);
            img1.color = new Color(1, 1, 1, alpha);
            img2.color = new Color(1, 1, 1, 1 - alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Nastavenie plnej neviditeľnosti prvého obrázka a plnej viditeľnosti druhého obrázka
        img1.color = new Color(1, 1, 1, 0);
        img2.color = new Color(1, 1, 1, 1);

        // Čakanie na koniec animácie
        yield return new WaitForSeconds(duration * (1 - switchRatio) - fadeDuration);

        // Zničenie obidvoch obrázkov po skončení animácie
        Destroy(imageObject1);
        Destroy(imageObject2);
    }
}
