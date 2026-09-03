using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TutorialPanelSequence : MonoBehaviour
{
    public string tutorialId;
    public TutorialService tutorialService;
    public List<TutorialStepPanelBinding> steps = new List<TutorialStepPanelBinding>();
    public GameObject delayPanel;
    public bool beginOnEnable = true;
    public bool hideRootWhenComplete = true;

    private int currentStepIndex = -1;
    private bool isBusy;

    private void Awake()
    {
        HideAllPanels();
    }

    private void OnEnable()
    {
        if (beginOnEnable)
        {
            Begin();
        }
    }

    public async void Begin()
    {
        if (isBusy)
        {
            return;
        }

        TutorialService service = ResolveTutorialService();
        if (service == null)
        {
            Debug.LogWarning($"[TutorialPanelSequence] Cannot start tutorial: tutorialService is not assigned");
            SetRootActive(false);
            return;
        }

        isBusy = true;
        TutorialStateResponse state = await service.RefreshCurrentPlayerStateAsync();
        isBusy = false;

        if (state == null || !state.success)
        {
            Debug.LogWarning(
                $"[TutorialPanelSequence] Cannot start tutorial: tutorial={tutorialId}, stage={state?.stage}, error={state?.error}"
            );
            SetRootActive(false);
            return;
        }

        ShowFirstIncompleteStep(state);
    }

    public void AcknowledgeCurrentStep()
    {
        if (currentStepIndex < 0 || currentStepIndex >= steps.Count)
        {
            CompleteSequence();
            return;
        }

        TutorialStepPanelBinding step = steps[currentStepIndex];
        StartCoroutine(CompleteAndAdvance(step));
    }

    public void CompleteNamedStep(string stepId)
    {
        TutorialStepPanelBinding step = steps.Find(entry => entry != null && entry.stepId == stepId);
        if (step == null)
        {
            Debug.LogWarning(
                $"[TutorialPanelSequence] Cannot complete missing step: tutorial={tutorialId}, step={stepId}"
            );
            return;
        }

        StartCoroutine(CompleteAndAdvance(step));
    }

    private IEnumerator CompleteAndAdvance(TutorialStepPanelBinding step)
    {
        if (isBusy || step == null)
        {
            yield break;
        }

        isBusy = true;
        HideAllPanels();

        Task<TutorialStateResponse> task = ResolveTutorialService()
            ?.CompleteCurrentPlayerStepAsync(tutorialId, step.stepId);
        if (task != null)
        {
            yield return new WaitUntil(() => task.IsCompleted);
        }

        if (step.delaySeconds > 0f && delayPanel != null)
        {
            delayPanel.SetActive(true);
            yield return new WaitForSeconds(step.delaySeconds);
            delayPanel.SetActive(false);
        }

        isBusy = false;
        ShowFirstIncompleteStep(task?.Result);
    }

    private void ShowFirstIncompleteStep(TutorialStateResponse state)
    {
        HideAllPanels();

        if (state == null || state.IsFlowCompleted(tutorialId))
        {
            CompleteSequence();
            return;
        }

        for (int index = 0; index < steps.Count; index++)
        {
            TutorialStepPanelBinding step = steps[index];
            if (step == null || string.IsNullOrWhiteSpace(step.stepId))
            {
                continue;
            }

            if (!state.HasCompletedStep(tutorialId, step.stepId))
            {
                currentStepIndex = index;
                if (step.panel != null)
                {
                    step.panel.SetActive(true);
                }
                return;
            }
        }

        CompleteSequence();
    }

    private void CompleteSequence()
    {
        currentStepIndex = -1;
        HideAllPanels();
        SetRootActive(!hideRootWhenComplete);
    }

    private void HideAllPanels()
    {
        if (delayPanel != null)
        {
            delayPanel.SetActive(false);
        }

        foreach (TutorialStepPanelBinding step in steps)
        {
            if (step?.panel != null)
            {
                step.panel.SetActive(false);
            }
        }
    }

    private void SetRootActive(bool active)
    {
        if (gameObject.activeSelf != active)
        {
            gameObject.SetActive(active);
        }
    }

    private TutorialService ResolveTutorialService()
    {
        if (tutorialService != null)
        {
            return tutorialService;
        }

        tutorialService = FindFirstObjectByType<TutorialService>(FindObjectsInactive.Include);
        return tutorialService;
    }
}

[Serializable]
public class TutorialStepPanelBinding
{
    public string stepId;
    public GameObject panel;
    public float delaySeconds;
}
