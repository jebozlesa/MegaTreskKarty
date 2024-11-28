using System.Collections;
using UnityEngine;

public class RecycleCardAnimation : MonoBehaviour
{
    public static RecycleCardAnimation Instance { get; private set; }

    public Sprite coinSprite;
    public GameObject startPoint;
    public GameObject endPoint;
    public float duration = 1f; // Predvolená doba trvania animácie

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ak chcete, aby sa tento objekt nezničil pri načítavaní novej scény
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Metóda na spustenie animácie priletenia mince k cieľu
    public void AnimateCoin()
    {
        if (startPoint != null && endPoint != null)
        {
            StartCoroutine(AnimateCoinCoroutine(startPoint.transform.position, endPoint.transform.position));
        }
        else
        {
            Debug.LogError("StartPoint or EndPoint is not set.");
        }
    }

    private IEnumerator AnimateCoinCoroutine(Vector2 startPoint, Vector2 endPoint)
    {
        GameObject coin = new GameObject("Coin");
        SpriteRenderer spriteRenderer = coin.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = coinSprite;

        coin.transform.position = startPoint;
        Vector3 targetPosition = endPoint;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            coin.transform.position = Vector3.Lerp(startPoint, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        coin.transform.position = targetPosition;
        Destroy(coin);
    }
}
