using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCharacterRandomDance : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Image targetImage;

    [Header("Resources")]
    [SerializeField]
    private string resourcesPath = "Loading/Characters";

    [Header("Playback")]
    [SerializeField]
    private float frameDurationSeconds = 0.14f;

    [SerializeField]
    private bool playOnEnable = true;

    private readonly Dictionary<string, List<FrameEntry>> characterSets =
        new Dictionary<string, List<FrameEntry>>();
    private readonly List<string> orderedSetKeys = new List<string>();

    private Coroutine danceRoutine;

    private struct FrameEntry
    {
        public int index;
        public Sprite sprite;
    }

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        RebuildSets();
    }

    private void OnEnable()
    {
        if (!playOnEnable)
        {
            return;
        }

        StartDance();
    }

    private void OnDisable()
    {
        StopDance();
    }

    public void StartDance()
    {
        if (targetImage == null)
        {
            Debug.LogError("[LoadingCharacterRandomDance] Missing target Image reference.");
            return;
        }

        if (orderedSetKeys.Count == 0)
        {
            Debug.LogError(
                "[LoadingCharacterRandomDance] No valid character sprite sets found in Resources/"
                    + resourcesPath
                    + ". Expected naming like einsteindance1..N or tesladance1..N."
            );
            return;
        }

        if (danceRoutine == null)
        {
            danceRoutine = StartCoroutine(DanceLoop());
        }
    }

    public void StopDance()
    {
        if (danceRoutine != null)
        {
            StopCoroutine(danceRoutine);
            danceRoutine = null;
        }
    }

    public void RebuildSets()
    {
        characterSets.Clear();
        orderedSetKeys.Clear();

        HashSet<string> processedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        Sprite[] sprites = Resources.LoadAll<Sprite>(resourcesPath);
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite sprite = sprites[i];
            if (sprite == null)
            {
                continue;
            }

            if (!TryParseName(sprite.name, out string key, out int frameIndex))
            {
                continue;
            }

            AddFrame(key, frameIndex, sprite);
            processedNames.Add(sprite.name);
        }

        // Also support textures imported as Texture2D (not Sprite) by creating runtime sprites.
        Texture2D[] textures = Resources.LoadAll<Texture2D>(resourcesPath);
        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D texture = textures[i];
            if (texture == null || processedNames.Contains(texture.name))
            {
                continue;
            }

            if (!TryParseName(texture.name, out string key, out int frameIndex))
            {
                continue;
            }

            Sprite runtimeSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f
            );

            AddFrame(key, frameIndex, runtimeSprite);
            processedNames.Add(texture.name);
        }

        for (int i = orderedSetKeys.Count - 1; i >= 0; i--)
        {
            string key = orderedSetKeys[i];
            List<FrameEntry> frames = characterSets[key];
            frames.Sort((a, b) => a.index.CompareTo(b.index));

            if (frames.Count < 2)
            {
                characterSets.Remove(key);
                orderedSetKeys.RemoveAt(i);
            }
        }
    }

    private void AddFrame(string key, int frameIndex, Sprite sprite)
    {
        if (!characterSets.TryGetValue(key, out List<FrameEntry> frames))
        {
            frames = new List<FrameEntry>();
            characterSets[key] = frames;
            orderedSetKeys.Add(key);
        }

        frames.Add(new FrameEntry { index = frameIndex, sprite = sprite });
    }

    private IEnumerator DanceLoop()
    {
        int previousSet = -1;

        while (true)
        {
            int setIndex = PickRandomSetIndex(previousSet);
            previousSet = setIndex;

            string setKey = orderedSetKeys[setIndex];
            List<FrameEntry> frames = characterSets[setKey];

            for (int i = 0; i < frames.Count; i++)
            {
                targetImage.sprite = frames[i].sprite;
                yield return new WaitForSecondsRealtime(frameDurationSeconds);
            }
        }
    }

    private int PickRandomSetIndex(int previousSet)
    {
        if (orderedSetKeys.Count == 1)
        {
            return 0;
        }

        int next = UnityEngine.Random.Range(0, orderedSetKeys.Count);
        if (next == previousSet)
        {
            next = (next + 1) % orderedSetKeys.Count;
        }

        return next;
    }

    private static bool TryParseName(string spriteName, out string key, out int frameIndex)
    {
        key = null;
        frameIndex = -1;

        Match match = Regex.Match(spriteName, "^(.*?)(\\d+)$");
        if (!match.Success)
        {
            return false;
        }

        key = match.Groups[1].Value.Trim();
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (!int.TryParse(match.Groups[2].Value, out frameIndex))
        {
            return false;
        }

        return frameIndex >= 0;
    }
}
