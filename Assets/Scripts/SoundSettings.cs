using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    public Sprite soundOnIcon; // Ikona pre zapnutý zvuk
    public Sprite soundOffIcon; // Ikona pre vypnutý zvuk
    public Image soundIconImage; // Image komponenta pre ikonu zvuku na tlačidle

    private bool isMusicMuted;

    private void Awake()
    {
        // Načíta nastavenie hudby
        isMusicMuted = PlayerPrefs.GetInt("isMusicMuted", 0) == 1;
        UpdateMusicState();
        UpdateSoundIcon();
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;
        PlayerPrefs.SetInt("isMusicMuted", isMusicMuted ? 1 : 0);
        PlayerPrefs.Save();

        UpdateSoundIcon();

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.RefreshMusicState();
        }
    }


    private void UpdateMusicState()
    {
        if (MusicManager.Instance != null)
        {
            if (isMusicMuted)
            {
                MusicManager.Instance.StopMusic();
            }
            else
            {
                MusicManager.Instance.PlayMusic();
            }
        }
    }

    private void UpdateSoundIcon()
    {
        if (soundIconImage != null)
        {
            soundIconImage.sprite = isMusicMuted ? soundOffIcon : soundOnIcon;
        }
    }
}

