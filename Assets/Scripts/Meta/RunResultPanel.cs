using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class RunResultPanel : MonoBehaviour
{
    private TMP_Text detailText;
    private Button nextRunButton;
    private IDungeonRunTransitionService transitionService;
    private bool pauseCaptured;
    private float previousTimeScale = 1f;

    [Inject]
    public void Construct(IDungeonRunTransitionService transitionService)
    {
        this.transitionService = transitionService;
    }

    public void Render(RunResultSnapshot result)
    {
        EnsureView();
        if (!pauseCaptured)
        {
            pauseCaptured = true;
            previousTimeScale = Time.timeScale;
        }

        Time.timeScale = 0f;
        gameObject.SetActive(true);
        if (detailText != null)
        {
            detailText.text = result != null ? result.ToDetailText() : "런 결과가 없습니다.";
            detailText.color = DungeonUiTheme.TextPrimary;
        }

        if (nextRunButton != null)
        {
            nextRunButton.interactable = transitionService != null && !transitionService.IsTransitioning;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (pauseCaptured)
        {
            Time.timeScale = previousTimeScale;
            pauseCaptured = false;
        }
    }

    private void EnsureView()
    {
        if (detailText != null && nextRunButton != null) return;

        detailText = transform.Find("RunResultText")?.GetComponent<TMP_Text>()
            ?? GetComponentInChildren<TMP_Text>(true);
        nextRunButton = transform.Find("NextRunButton")?.GetComponent<Button>();
        if (nextRunButton != null)
        {
            nextRunButton.onClick.RemoveListener(HandleNextRun);
            nextRunButton.onClick.AddListener(HandleNextRun);
        }
    }

    private void HandleNextRun()
    {
        if (transitionService == null || transitionService.IsTransitioning)
        {
            return;
        }

        if (nextRunButton != null)
        {
            nextRunButton.interactable = false;
        }

        transitionService.StartNextRun();
    }

    private void OnDestroy()
    {
        if (nextRunButton != null)
        {
            nextRunButton.onClick.RemoveListener(HandleNextRun);
        }
    }
}
