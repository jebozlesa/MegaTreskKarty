using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DualImageAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite sprite1, Sprite sprite2, Transform position, Vector2 imageSize, float duration, AudioClip startSound, float switchRatio = 0.5f)
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

        // Nastavenie pozície obrázkov
        imageObject1.transform.position = position.position;
        imageObject2.transform.position = position.position;

        // Čakanie na zmenu obrázka
        yield return new WaitForSeconds(duration * switchRatio);

        // Zmenšenie prvého obrázka a zväčšenie druhého obrázka
        float switchDuration = duration * (1 - switchRatio);
        float elapsedTime = 0;
        while (elapsedTime < switchDuration)
        {
            float scale = 1 - (elapsedTime / switchDuration);
            imageObject1.transform.localScale = new Vector3(scale, scale, scale);
            imageObject2.transform.localScale = new Vector3(1 - scale, 1 - scale, 1 - scale);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Zničenie prvý obrázok a nechaj druhý obrázok v plnej veľkosti
        Destroy(imageObject1);
        imageObject2.transform.localScale = Vector3.one;

        // Čakanie na koniec animácie
        yield return new WaitForSeconds(duration * (1 - switchRatio));

        // Zničenie druhý obrázok po skončení animácie
        Destroy(imageObject2);
    }
}
