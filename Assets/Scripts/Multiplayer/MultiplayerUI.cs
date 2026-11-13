using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerUI : MonoBehaviour
{
    // [OK] Konstanty pre dialog messages - centralne miesto pre vsetky UI texty
    public const string MSG_CHOOSE_ATTACK = "Choose your attack!";
    public const string MSG_WAITING_OPPONENT = "Waiting for opponent...";
    public const string MSG_LOADING = "Loading...";
    public const string MSG_CHOOSE_FIGHTER = "Choose fighter!";
    
    [Header("UI References")]
    public TMP_Text playerNameText;
    public TMP_Text enemyNameText;
    public TMP_Text statusText;
    public Button exitButton;
    public MultiplayerService multiplayerService;
    public TMP_Text inputCountdownText;
    public Image inputCountdownFrame;
    public Image inputCountdownFace;
    public AudioClip inputCountdownRevealSound;
    public AudioClip inputCountdownUrgencySound;
    public Color inputCountdownUrgentColor = Color.red;
    public Color inputCountdownPulseColor = Color.blue;
    public float inputCountdownShakeDuration = 1f;
    public float inputCountdownShakeRotation = 8f;

    private const int InputCountdownRevealThresholdMs = 15000;
    private const int InputCountdownUrgencyThresholdMs = 3000;
    private const int InputCountdownPulseIntervalMs = 500;
    private const float InputCountdownShakeOscillations = 6f;
    private Color inputCountdownOriginalFaceColor;
    private bool hasInputCountdownOriginalFaceColor;
    private bool inputCountdownWasVisible;
    private int lastInputCountdownUrgencySecond;
    private Coroutine inputCountdownShakeCoroutine;
    private Quaternion inputCountdownShakeOriginRotation;
    private bool hasInputCountdownShakeOrigin;

    private void Awake()
    {
        if (inputCountdownFace != null)
        {
            inputCountdownOriginalFaceColor = inputCountdownFace.color;
            hasInputCountdownOriginalFaceColor = true;
        }
    }

    private void OnDisable()
    {
        ResetInputCountdownPresentation();
    }

    private void Update()
    {
        if (inputCountdownText == null) return;
        int remainingMs = multiplayerService != null && multiplayerService.HasInputCountdown
            ? multiplayerService.InputDisplayRemainingMilliseconds : 0;
        bool visible = ShouldShowInputCountdown(remainingMs);
        GameObject root = inputCountdownText.transform.parent != null
            ? inputCountdownText.transform.parent.gameObject : inputCountdownText.gameObject;
        if (root.activeSelf != visible) root.SetActive(visible);

        int soundCue = AdvanceInputCountdownSoundState(visible, remainingMs);
        if (!visible)
        {
            ApplyInputCountdownColorPhase(0);
            StopInputCountdownShake();
            return;
        }

        inputCountdownText.text = Mathf.CeilToInt(remainingMs / 1000f).ToString();
        ApplyInputCountdownColorPhase(GetInputCountdownColorPhase(remainingMs));
        if (soundCue == 1)
        {
            StartInputCountdownShake();
            AudioManager.Instance?.PlaySound(inputCountdownRevealSound);
        }
        else if (soundCue == 2)
            AudioManager.Instance?.PlaySound(inputCountdownUrgencySound);
    }

    private static bool ShouldShowInputCountdown(int remainingMs)
    {
        return remainingMs > 0 && remainingMs <= InputCountdownRevealThresholdMs;
    }

    private static int GetInputCountdownColorPhase(int remainingMs)
    {
        if (remainingMs <= 0 || remainingMs > InputCountdownUrgencyThresholdMs) return 0;
        int halfSecond = Mathf.CeilToInt(remainingMs / (float)InputCountdownPulseIntervalMs);
        return halfSecond % 2 == 0 ? 1 : 2;
    }

    private int AdvanceInputCountdownSoundState(bool visible, int remainingMs)
    {
        if (!visible)
        {
            inputCountdownWasVisible = false;
            lastInputCountdownUrgencySecond = 0;
            return 0;
        }

        int urgencySecond = remainingMs <= InputCountdownUrgencyThresholdMs
            ? Mathf.CeilToInt(remainingMs / 1000f) : 0;
        if (!inputCountdownWasVisible)
        {
            inputCountdownWasVisible = true;
            lastInputCountdownUrgencySecond = urgencySecond;
            return urgencySecond > 0 ? 2 : 1;
        }

        if (urgencySecond <= 0 || urgencySecond == lastInputCountdownUrgencySecond) return 0;
        lastInputCountdownUrgencySecond = urgencySecond;
        return 2;
    }

    private void ApplyInputCountdownColorPhase(int phase)
    {
        Color phaseColor;
        if (phase == 1)
            phaseColor = inputCountdownUrgentColor;
        else if (phase == 2)
            phaseColor = inputCountdownPulseColor;
        else
        {
            RestoreInputCountdownFaceColor();
            return;
        }

        if (inputCountdownFace != null)
            inputCountdownFace.color = phaseColor;
    }

    private void RestoreInputCountdownFaceColor()
    {
        if (inputCountdownFace != null && hasInputCountdownOriginalFaceColor)
            inputCountdownFace.color = inputCountdownOriginalFaceColor;
    }

    private void StartInputCountdownShake()
    {
        StopInputCountdownShake();
        RectTransform target = inputCountdownFrame != null
            ? inputCountdownFrame.rectTransform : null;
        if (target == null || inputCountdownShakeDuration <= 0f) return;

        inputCountdownShakeOriginRotation = target.localRotation;
        hasInputCountdownShakeOrigin = true;
        inputCountdownShakeCoroutine = StartCoroutine(ShakeInputCountdown(target));
    }

    private IEnumerator ShakeInputCountdown(RectTransform target)
    {
        float elapsed = 0f;
        while (elapsed < inputCountdownShakeDuration)
        {
            float normalizedTime = elapsed / inputCountdownShakeDuration;
            float wave = EvaluateInputCountdownShake(normalizedTime);
            target.localRotation = inputCountdownShakeOriginRotation
                * Quaternion.Euler(0f, 0f, wave * inputCountdownShakeRotation);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        RestoreInputCountdownRotation(target);
        inputCountdownShakeCoroutine = null;
    }

    private static float EvaluateInputCountdownShake(float normalizedTime)
    {
        float time = Mathf.Clamp01(normalizedTime);
        if (time <= 0f || time >= 1f) return 0f;
        return Mathf.Sin(time * Mathf.PI * 2f * InputCountdownShakeOscillations);
    }

    private void StopInputCountdownShake()
    {
        if (inputCountdownShakeCoroutine != null)
        {
            StopCoroutine(inputCountdownShakeCoroutine);
            inputCountdownShakeCoroutine = null;
        }
        if (inputCountdownFrame != null)
            RestoreInputCountdownRotation(inputCountdownFrame.rectTransform);
    }

    private void RestoreInputCountdownRotation(RectTransform target)
    {
        if (!hasInputCountdownShakeOrigin || target == null) return;
        target.localRotation = inputCountdownShakeOriginRotation;
        hasInputCountdownShakeOrigin = false;
    }

    private void ResetInputCountdownPresentation()
    {
        inputCountdownWasVisible = false;
        lastInputCountdownUrgencySecond = 0;
        ApplyInputCountdownColorPhase(0);
        StopInputCountdownShake();
    }

    // Verejne metody pre volanie z FightSystemMultiplayer
    public void UpdatePlayerInfo(string playerName, string enemyName)
    {
        playerNameText.text = playerName;
        enemyNameText.text = enemyName;
    }

    public void ShowStatus(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }

    public void SetExitButtonListener(UnityEngine.Events.UnityAction action)
    {
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(action);
            Debug.Log("[MultiplayerUI] Exit button listener registered");
        }
        else
        {
            Debug.LogError("[MultiplayerUI] Exit button is not assigned");
        }
    }

    // Pridat dalsie metody podla potreby (ShowCards, ShowVictory, ShowDefeat, ...)
}
