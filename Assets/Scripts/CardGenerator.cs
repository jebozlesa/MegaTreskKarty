using System.Collections;
using UnityEngine;

public class CardGenerator : MonoBehaviour
{
    public Canvas canvas;
    public GameObject cardPrefab;

    public IEnumerator ShowCardOnScreen(
        int id,
        string cardName,
        string image,
        Color32 color,
        int level
    )
    {
        Debug.Log($"ShowCardOnScreen({id},{cardName},{level})");

        if (canvas == null || cardPrefab == null)
        {
            Debug.LogError("[CardGenerator] Cannot show card: canvas or cardPrefab is not assigned.");
            yield break;
        }

        GameObject cardInstance = Instantiate(cardPrefab);
        cardInstance.transform.SetParent(canvas.transform, false);

        ShowCard showCard = cardInstance.GetComponent<ShowCard>();
        if (showCard == null)
        {
            Debug.LogError("[CardGenerator] Cannot show card: cardPrefab is missing ShowCard.");
            Destroy(cardInstance);
            yield break;
        }

        showCard.cardId = id;
        showCard.cardName = cardName;
        showCard.image = image;
        showCard.color = color;
        showCard.level = level;

        cardInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        RectTransform cardRectTransform = cardInstance.GetComponent<RectTransform>();
        if (cardRectTransform != null)
        {
            cardRectTransform.anchoredPosition = Vector2.zero;
        }

        cardInstance.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        cardInstance.SetActive(false);
        Destroy(cardInstance);
    }
}
