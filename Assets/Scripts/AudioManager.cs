using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip buttonClickSound;
    // public AudioClip purchaseSound;
    // public AudioClip moneyAddedSound;
    // public AudioClip cardAcquiredSound;
    // ... pridajte ďalšie zvuky podľa potreby

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Pomocné metódy pre špecifické zvuky
    public void PlayButtonClickSound()
    {
        PlaySound(buttonClickSound);
    }

    // public void PlayPurchaseSound()
    // {
    //     PlaySound(purchaseSound);
    // }

    // public void PlayMoneyAddedSound()
    // {
    //     PlaySound(moneyAddedSound);
    // }

    // public void PlayCardAcquiredSound()
    // {
    //     PlaySound(cardAcquiredSound);
    // }

    // ... pridajte ďalšie metódy pre špecifické zvuky
}
