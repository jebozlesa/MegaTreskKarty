using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextBubble : MonoBehaviour
{
    public TMP_Text bubbleText;
    public Image image;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    public IEnumerator ShowForSeconds(string text, Sprite sprite, float seconds)
    {
        bubbleText.text = text;
        image.sprite = sprite;
        gameObject.SetActive(true);

        yield return new WaitForSeconds(seconds);

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetText(string line)
    {
        bubbleText.text = line;
    }
}
