using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingSelectButton : MonoBehaviour
{
    private static readonly Vector2 DefaultButtonSize = new Vector2(100f, 100f);
    private static readonly Vector2 DefaultIconMaxSize = new Vector2(84f, 64f);

    [SerializeField] private Image iconImage;
    [SerializeField] private Vector2 buttonSize = DefaultButtonSize;
    [SerializeField] private Vector2 iconMaxSize = DefaultIconMaxSize;

    public int id;

    public void Initialization(BuildingSO so)
    {
        if (so == null) return;

        id = so.id;
        ApplyButtonSize();
        SetIcon(so.icon);
    }

    public void OnClick()
    {
        DungeonStoryGridBuildingController.Instance.SetGridModeBuild();
        DungeonStoryGridBuildingController.Instance.SelectBuildingById(id);
    }
    public void ActiveDestroyMode()
    {
        DungeonStoryGridBuildingController.Instance.SetDestroyMode();
    }

    private void ApplyButtonSize()
    {
        if (transform is RectTransform rectTransform)
        {
            rectTransform.sizeDelta = buttonSize;
        }
    }

    private void SetIcon(Sprite sprite)
    {
        Image image = ResolveIconImage();
        if (image == null) return;

        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        if (image.transform is RectTransform iconRect)
        {
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = Vector2.zero;
            iconRect.sizeDelta = GetFittedIconSize(sprite);
        }
    }

    private Image ResolveIconImage()
    {
        if (iconImage != null) return iconImage;
        if (transform.childCount <= 0) return null;

        iconImage = transform.GetChild(0).GetComponent<Image>();
        return iconImage;
    }

    private Vector2 GetFittedIconSize(Sprite sprite)
    {
        if (sprite == null || sprite.rect.width <= 0f || sprite.rect.height <= 0f)
        {
            return iconMaxSize;
        }

        float scale = Mathf.Min(
            iconMaxSize.x / sprite.rect.width,
            iconMaxSize.y / sprite.rect.height);
        scale = Mathf.Min(1f, scale);
        return new Vector2(sprite.rect.width * scale, sprite.rect.height * scale);
    }
}
