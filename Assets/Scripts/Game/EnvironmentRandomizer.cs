using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentRandomizer : MonoBehaviour
{
    public Image backgroundImage; // A reference to the Image component for the background
    private AudioSource audioSource; // A reference to the AudioSource component for the background sound

    void Start()
    {
        // Ensure that there's an AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();

        // Load and apply a random environment
        ApplyRandomEnvironment();
    }

    void ApplyRandomEnvironment()
    {
        // Load all background images
        Sprite[] backgrounds = Resources.LoadAll<Sprite>("Backgrounds");
        Dictionary<string, AudioClip> backgroundSounds = new Dictionary<string, AudioClip>();

        // Load all background sounds into a dictionary
        foreach (var sound in Resources.LoadAll<AudioClip>("BackgroundSound"))
        {
            backgroundSounds[sound.name] = sound;
        }

        // Make sure we have at least one background to choose from
        if (backgrounds.Length > 0)
        {
            // Randomly select an index
            int randomIndex = Random.Range(0, backgrounds.Length);

            // Set the background image
            backgroundImage.sprite = backgrounds[randomIndex];

            // Try to find a matching background sound
            if (backgroundSounds.TryGetValue(backgrounds[randomIndex].name, out AudioClip matchingSound))
            {
                // Set the background sound if found
                audioSource.clip = matchingSound;
                audioSource.loop = true; // Set the audio to loop
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"No matching sound found for {backgrounds[randomIndex].name}");
            }
        }
        else
        {
            Debug.LogWarning("No backgrounds found. Please make sure they are placed in the Resources folder.");
        }
    }
}
