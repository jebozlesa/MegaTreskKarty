using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;

            UpdateMusicStateBasedOnScene(SceneManager.GetActiveScene().name);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateMusicStateBasedOnScene(string sceneName)
    {
        bool isMusicMuted = PlayerPrefs.GetInt("isMusicMuted", 0) == 1;
        audioSource.mute = isMusicMuted;

        if (isMusicMuted)
        {
            StopMusic();
        }
        else
        {
            if (sceneName == "Login" || sceneName == "Game")
            {
                StopMusic();
            }
            else if (sceneName == "Main" || sceneName == "Marketplace")
            {
                PlayMusic();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusicStateBasedOnScene(scene.name);
    }

    public void RefreshMusicState()
    {
        UpdateMusicStateBasedOnScene(SceneManager.GetActiveScene().name);
    }

    public void PlayMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            // Aplikácia sa obnovuje z pauzy
            AudioSettings.Reset(AudioSettings.GetConfiguration());
            RefreshMusicState();

            // Ak sme na scéne "Main" a hudba nie je stlmená, spustiť hudbu
            string currentSceneName = SceneManager.GetActiveScene().name;
            bool isMusicMuted = PlayerPrefs.GetInt("isMusicMuted", 0) == 1;
            if (currentSceneName == "Main" && !isMusicMuted && !audioSource.isPlaying)
            {
                PlayMusic();
            }
        }
    }

}
