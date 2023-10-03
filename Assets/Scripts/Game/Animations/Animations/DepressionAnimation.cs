using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DepressionAnimation : MonoBehaviour
{
    public Canvas canvas; // Referencia na váš Canvas

    public IEnumerator StartAnimation(Sprite sprite1, Transform position, Vector2 imageSize, float duration, AudioClip startSound)
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

        // Nastavenie veľkosti obrázka
        RectTransform rectTransform1 = imageObject1.GetComponent<RectTransform>();
        rectTransform1.sizeDelta = imageSize;

        // Nastavenie pozície obrázka
        imageObject1.transform.position = position.position;

        // Zpriehladnenie prvého obrázka
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            float alpha = 1 - (elapsedTime / duration);
            img1.color = new Color(1, 1, 1, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Nastavenie plnej neviditeľnosti prvého obrázka
        img1.color = new Color(1, 1, 1, 0);

        // Zničenie obrázka po skončení animácie
        Destroy(imageObject1);
    }
}
