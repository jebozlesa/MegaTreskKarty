using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentRandomizer : MonoBehaviour
{
    public Image backgroundImage; // Referencia na komponent Image pre pozadie
    private AudioSource audioSource; // Referencia na komponent AudioSource pre zvuk pozadia

    void Start()
    {
        // Zabezpečíme, že existuje komponent AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.volume = 0.5f;

        // Načítame a aplikujeme náhodné prostredie
        ApplyRandomEnvironment();

        // Aktualizujeme stav zvuku
        UpdateAudioState();
    }

    void ApplyRandomEnvironment()
    {
        // Načítame všetky obrázky pozadia
        Sprite[] backgrounds = Resources.LoadAll<Sprite>("Backgrounds");
        Dictionary<string, AudioClip> backgroundSounds = new Dictionary<string, AudioClip>();

        // Načítame všetky zvuky pozadia do slovníka
        foreach (var sound in Resources.LoadAll<AudioClip>("BackgroundSound"))
        {
            backgroundSounds[sound.name] = sound;
        }

        // Uistíme sa, že máme aspoň jedno pozadie na výber
        if (backgrounds.Length > 0)
        {
            // Náhodne vyberieme index
            int randomIndex = Random.Range(0, backgrounds.Length);

            // Nastavíme obrázok pozadia
            backgroundImage.sprite = backgrounds[randomIndex];

            // Pokúsime sa nájsť zodpovedajúci zvuk pozadia
            if (backgroundSounds.TryGetValue(backgrounds[randomIndex].name, out AudioClip matchingSound))
            {
                // Nastavíme zvuk pozadia, ak bol nájdený
                audioSource.clip = matchingSound;
                audioSource.loop = true; // Nastavíme audio na opakovanie
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"Nebol nájdený zvuk zodpovedajúci obrázku {backgrounds[randomIndex].name}");
            }
        }
        else
        {
            Debug.LogWarning("Neboli nájdené žiadne obrázky pozadia. Uistite sa, že sú umiestnené v priečinku Resources.");
        }
    }

    void UpdateAudioState()
    {
        // Kontrolujeme, či je hudba zapnutá alebo vypnutá
        bool isMusicMuted = PlayerPrefs.GetInt("isMusicMuted", 0) == 1;
        audioSource.mute = isMusicMuted;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // Aplikácia sa obnovuje z pauzy
            AudioSettings.Reset(AudioSettings.GetConfiguration());
            UpdateAudioState();

            // Ak hudba nie je stlmená a nehrá, spustiť hudbu
            bool isMusicMuted = PlayerPrefs.GetInt("isMusicMuted", 0) == 1;
            if (!isMusicMuted && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
