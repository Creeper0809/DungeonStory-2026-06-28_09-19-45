using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class NoticeFeed : MonoBehaviour, UtilEventListener<NoticeFeedEvent>
{
    public GameObject textPrefab;
    private INoticeFeedPresenter presenter;

    private void Awake()
    {
        ConfigureLayout();
    }

    [Inject]
    public void ConstructNoticeFeed(INoticeFeedPresenter presenter)
    {
        this.presenter = presenter
            ?? throw new System.ArgumentNullException(nameof(presenter));
    }

    public virtual void OnTriggerEvent(NoticeFeedEvent e)
    {
        RequirePresenter().Present(textPrefab, transform, e);
    }

    private void OnEnable()
    {
        ConfigureLayout();
        this.EventStartListening();
    }
    private void OnDisable()
    {
        this.EventStopListening();
    }

    private INoticeFeedPresenter RequirePresenter()
    {
        return presenter
            ?? throw new System.InvalidOperationException(
                $"{nameof(NoticeFeed)} requires VContainer injection of {nameof(INoticeFeedPresenter)}.");
    }

    private void ConfigureLayout()
    {
        if (transform is RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -24f);
            rect.sizeDelta = new Vector2(520f, 360f);
        }

        VerticalLayoutGroup layout = GetComponent<VerticalLayoutGroup>();
        if (layout != null)
        {
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.color = Color.clear;
            image.raycastTarget = false;
        }

        if (GetComponent<RectMask2D>() == null)
        {
            gameObject.AddComponent<RectMask2D>();
        }
    }
}
public struct NoticeFeedEvent
{
    public string notice;
    public Grade grade;
    public enum Grade
    {
        NONE,
        WARNING,
        DANGER
    }
    public NoticeFeedEvent(string notice, Grade grade)
    {
        this.notice = notice;
        this.grade = grade;
    }
    static NoticeFeedEvent e;
    public static void Trigger(string notice, Grade grade)
    {
        e.notice = notice;
        e.grade = grade;
        EventObserver.TriggerEvent(e);
    }
}
