using TMPro;
using UnityEngine;

public class RunResultPanel : MonoBehaviour
{
    private TMP_Text detailText;

    public void Render(RunResultSnapshot result)
    {
        EnsureView();
        gameObject.SetActive(true);
        if (detailText != null)
        {
            detailText.text = result != null ? result.ToDetailText() : "런 결과 없음";
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void EnsureView()
    {
        if (detailText != null) return;

        detailText = GetComponentInChildren<TMP_Text>(true);
    }
}
