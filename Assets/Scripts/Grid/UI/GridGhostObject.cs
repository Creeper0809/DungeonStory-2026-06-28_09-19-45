using UnityEngine;

public class GridGhostObject : MonoBehaviour
{
    [SerializeField] private GameObject ghostObject;
    [SerializeField] private Color buildableColor = Color.green;
    [SerializeField] private Color blockedColor = Color.red;

    private SpriteRenderer ghostSpriteRenderer;

    public bool IsHidden { get; private set; } = true;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize(GameObject target = null)
    {
        if (target != null)
        {
            ghostObject = target;
        }

        if (ghostObject == null && transform.childCount > 0)
        {
            ghostObject = transform.GetChild(0).gameObject;
        }

        ghostSpriteRenderer = ghostObject != null
            ? ghostObject.GetComponent<SpriteRenderer>()
            : null;
    }

    public void Show(Sprite sprite)
    {
        EnsureInitialized();
        IsHidden = false;
        if (ghostSpriteRenderer != null)
        {
            ghostSpriteRenderer.sprite = sprite;
        }
    }

    public void Hide()
    {
        EnsureInitialized();
        IsHidden = true;
        if (ghostSpriteRenderer != null)
        {
            ghostSpriteRenderer.sprite = null;
        }
    }

    public void SetBuildable(bool buildable)
    {
        EnsureInitialized();
        if (ghostSpriteRenderer != null)
        {
            ghostSpriteRenderer.color = buildable ? buildableColor : blockedColor;
        }
    }

    public void SetWorldPosition(Vector3 worldPosition, float lerpSpeed = 0f)
    {
        EnsureInitialized();
        if (ghostObject == null) return;

        ghostObject.transform.position = lerpSpeed > 0f
            ? Vector3.Lerp(ghostObject.transform.position, worldPosition, Time.unscaledDeltaTime * lerpSpeed)
            : worldPosition;
    }

    public void SetSize(Vector2 size)
    {
        EnsureInitialized();
        if (ghostSpriteRenderer != null)
        {
            ghostSpriteRenderer.size = size;
        }
    }

    private void EnsureInitialized()
    {
        if (ghostObject != null && ghostSpriteRenderer != null) return;

        Initialize();
    }
}
