using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnlargeImage : MonoBehaviour
{
    public Canvas canvas; // Reference na váš Canvas

    public IEnumerator StartEnlargeAnimation(Sprite sprite, Transform targetCard, Vector2 startSize, Vector2 endSize, float duration, float startRotation = 0f, float endRotation = 0f, AudioClip soundEffect = null)
    {
        // Pokud existuje zvukový efekt, přehrajte ho
        if (soundEffect)
        {
            AudioSource.PlayClipAtPoint(soundEffect, Camera.main.transform.position);
        }

        // Vytvoření GameObjectu pro obrázek
        GameObject imageObject = new GameObject("EnlargeImageSprite");
        imageObject.transform.SetParent(canvas.transform, false); // Nastavení jako dítě Canvasu
        Image img = imageObject.AddComponent<Image>();
        img.sprite = sprite;

        // Nastavení počáteční velikosti obrázka
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = startSize;

        // Nastavení počáteční rotace
        imageObject.transform.eulerAngles = new Vector3(0, 0, startRotation);

        // Nastavení pozice obrázka na cílovou kartu
        imageObject.transform.position = targetCard.position;

        // Spuštění animace zvětšení
        yield return StartCoroutine(AnimateEnlarge(imageObject, startSize, endSize, startRotation, endRotation, duration));

        // Zničení obrázka po dokončení animace
        Destroy(imageObject, 0.1f);
    }

    private IEnumerator AnimateEnlarge(GameObject image, Vector2 startSize, Vector2 endSize, float startRotation, float endRotation, float duration)
    {
        float elapsedTime = 0;
        RectTransform rectTransform = image.GetComponent<RectTransform>();

        while (elapsedTime < duration)
        {
            rectTransform.sizeDelta = Vector2.Lerp(startSize, endSize, (elapsedTime / duration));
            
            // Lineárna interpolácia medzi počiatočnou a konečnou rotáciou
            float currentRotation = Mathf.Lerp(startRotation, endRotation, (elapsedTime / duration));
            image.transform.eulerAngles = new Vector3(0, 0, currentRotation);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.sizeDelta = endSize;
        image.transform.eulerAngles = new Vector3(0, 0, endRotation);
    }
}
