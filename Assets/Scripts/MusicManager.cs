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
            if (audioSource != null)
            {
                audioSource.loop = true;
            }

            UpdateMusicStateBasedOnScene(SceneManager.GetActiveScene().name);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateMusicStateBasedOnScene(string sceneName)
    {
        if (audioSource == null)
        {
            return;
        }

        GameAudioSettings.ApplyMasterAudioState();
        audioSource.mute = !GameAudioSettings.IsMusicEnabled;

        if (GameAudioSettings.ShouldPlayMusicInScene(sceneName))
        {
            PlayMusic();
        }
        else
        {
            StopMusic();
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
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
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
            AudioSettings.Reset(AudioSettings.GetConfiguration());
            RefreshMusicState();

            string currentSceneName = SceneManager.GetActiveScene().name;
            if (
                audioSource != null
                && GameAudioSettings.ShouldPlayMusicInScene(currentSceneName)
                && !audioSource.isPlaying
            )
            {
                PlayMusic();
            }
        }
    }
}
