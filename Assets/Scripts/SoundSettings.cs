using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    public Sprite soundOnIcon;
    public Sprite soundOffIcon;
    public Image soundIconImage;

    private bool isMusicMuted;

    private void Awake()
    {
        isMusicMuted = !GameAudioSettings.IsMusicEnabled;
        GameAudioSettings.ApplyStoredSettings();
        UpdateSoundIcon();
    }

    public void ToggleMusic()
    {
        GameAudioSettings.SetMusicEnabled(!GameAudioSettings.IsMusicEnabled);
        isMusicMuted = !GameAudioSettings.IsMusicEnabled;
        UpdateSoundIcon();
    }

    private void UpdateSoundIcon()
    {
        if (soundIconImage != null)
        {
            soundIconImage.sprite = isMusicMuted ? soundOffIcon : soundOnIcon;
        }
    }
}
