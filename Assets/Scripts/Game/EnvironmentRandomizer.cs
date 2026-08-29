using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentRandomizer : MonoBehaviour
{
    public Image backgroundImage;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.5f;

        ApplyRandomEnvironment();
        UpdateAudioState();
    }

    private void ApplyRandomEnvironment()
    {
        Sprite[] backgrounds = Resources.LoadAll<Sprite>("Backgrounds");
        Dictionary<string, AudioClip> backgroundSounds = new Dictionary<string, AudioClip>();

        foreach (AudioClip sound in Resources.LoadAll<AudioClip>("BackgroundSound"))
        {
            backgroundSounds[sound.name] = sound;
        }

        if (backgrounds.Length > 0)
        {
            int randomIndex = Random.Range(0, backgrounds.Length);
            backgroundImage.sprite = backgrounds[randomIndex];

            if (backgroundSounds.TryGetValue(backgrounds[randomIndex].name, out AudioClip matchingSound))
            {
                audioSource.clip = matchingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"No matching background sound found for {backgrounds[randomIndex].name}");
            }
        }
        else
        {
            Debug.LogWarning("No background sprites were found in Resources/Backgrounds.");
        }
    }

    private void UpdateAudioState()
    {
        GameAudioSettings.ApplyMasterAudioState();
        audioSource.mute = !GameAudioSettings.IsMusicEnabled || !GameAudioSettings.IsSoundEnabled;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            AudioSettings.Reset(AudioSettings.GetConfiguration());
            UpdateAudioState();

            if (
                GameAudioSettings.IsSoundEnabled
                && GameAudioSettings.IsMusicEnabled
                && !audioSource.isPlaying
            )
            {
                audioSource.Play();
            }
        }
    }
}
