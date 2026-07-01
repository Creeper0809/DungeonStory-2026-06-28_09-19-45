using TMPro;
using UnityEngine;

public static class TMPKoreanFont
{
    private const string EditorFontPath = "Assets/Font/Maplestory Light SDF.asset";

    private static TMP_FontAsset cachedFont;

    public static TMP_FontAsset Resolve()
    {
        if (cachedFont != null)
        {
            return cachedFont;
        }

#if UNITY_EDITOR
        cachedFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(EditorFontPath);
#endif
        return cachedFont != null ? cachedFont : TMP_Settings.defaultFontAsset;
    }

    public static void Apply(TMP_Text text)
    {
        if (text == null) return;

        TMP_FontAsset font = Resolve();
        if (font != null)
        {
            text.font = font;
        }
    }

    public static void ApplyToChildren(Transform root, bool includeInactive = true)
    {
        if (root == null) return;

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(includeInactive))
        {
            Apply(text);
        }
    }
}
