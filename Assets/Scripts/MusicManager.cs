using UnityEngine;
using UnityEngine.SceneManagement; // Potrebné pre prácu so scénami

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Udrží tento objekt pri prechode medzi scénami
            SetupMusic();

            // Prihlásenie na udalosť sceneLoaded
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Zničí duplicitné inštancie
        }
    }

    private void SetupMusic()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true; // Nastaví hudbu na prehrávanie v slučke
        }
    }

    // Táto metóda sa zavolá vždy, keď sa načíta nová scéna
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Login")
        {
            StopMusic();
        }
        else if (scene.name == "Main")
        {
            PlayMusic();
        }
        else if (scene.name == "Game")
        {
            StopMusic();
        }
        // Tento kód môžete rozšíriť pre ďalšie scény podľa potreby
    }

    public void PlayMusic()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void OnDestroy()
    {
        // Odhlásenie z udalosti sceneLoaded pri zničení objektu
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
