using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    // internal static class ExpeditionRuntimeUiFactory
    // {
    //     private static Font _defaultFont;
    //     private static Sprite _defaultSprite;

    //     private static Font DefaultFont => _defaultFont ??= Resources.GetBuiltinResource<Font>("Arial.ttf");

    //     private static Sprite DefaultSprite
    //     {
    //         get
    //         {
    //             if (_defaultSprite == null)
    //             {
    //                 _defaultSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
    //             }

    //             return _defaultSprite;
    //         }
    //     }

    //     public static RectTransform EnsureWindowRoot(RectTransform parent, string name, Color backgroundColor)
    //     {
    //         var root = EnsureRectTransform(parent, name);
    //         StretchToFill(root);
    //         var image = GetOrAddComponent<Image>(root.gameObject);
    //         image.sprite = DefaultSprite;
    //         image.color = backgroundColor;
    //         return root;
    //     }

    //     public static Text EnsureText(Transform parent, string name, Vector2 anchoredPosition, Vector2 sizeDelta, int fontSize, TextAnchor alignment)
    //     {
    //         var rect = EnsureRectTransform(parent, name);
    //         rect.anchorMin = new Vector2(0.5f, 0.5f);
    //         rect.anchorMax = new Vector2(0.5f, 0.5f);
    //         rect.pivot = new Vector2(0.5f, 0.5f);
    //         rect.anchoredPosition = anchoredPosition;
    //         rect.sizeDelta = sizeDelta;

    //         var text = GetOrAddComponent<Text>(rect.gameObject);
    //         text.font = DefaultFont;
    //         text.fontSize = fontSize;
    //         text.alignment = alignment;
    //         text.horizontalOverflow = HorizontalWrapMode.Wrap;
    //         text.verticalOverflow = VerticalWrapMode.Overflow;
    //         text.color = Color.white;
    //         return text;
    //     }

    //     public static Button EnsureButton(Transform parent, string name, Vector2 anchoredPosition, Vector2 sizeDelta, string label)
    //     {
    //         var rect = EnsureRectTransform(parent, name);
    //         rect.anchorMin = new Vector2(0.5f, 0.5f);
    //         rect.anchorMax = new Vector2(0.5f, 0.5f);
    //         rect.pivot = new Vector2(0.5f, 0.5f);
    //         rect.anchoredPosition = anchoredPosition;
    //         rect.sizeDelta = sizeDelta;

    //         var image = GetOrAddComponent<Image>(rect.gameObject);
    //         image.sprite = DefaultSprite;
    //         image.color = new Color(0.18f, 0.22f, 0.28f, 0.95f);

    //         var button = GetOrAddComponent<Button>(rect.gameObject);
    //         button.targetGraphic = image;
    //         var colors = button.colors;
    //         colors.normalColor = image.color;
    //         colors.highlightedColor = new Color(0.28f, 0.36f, 0.46f, 1f);
    //         colors.pressedColor = new Color(0.14f, 0.18f, 0.24f, 1f);
    //         colors.selectedColor = colors.highlightedColor;
    //         button.colors = colors;

    //         var labelText = EnsureText(rect, "m_textLabel", Vector2.zero, sizeDelta, 28, TextAnchor.MiddleCenter);
    //         labelText.text = label;
    //         return button;
    //     }

    //     public static void SetButtonLabel(Button button, string label)
    //     {
    //         if (button == null)
    //         {
    //             return;
    //         }

    //         var text = button.GetComponentInChildren<Text>(true);
    //         if (text != null)
    //         {
    //             text.text = label;
    //         }
    //     }

    //     private static RectTransform EnsureRectTransform(Transform parent, string name)
    //     {
    //         var child = parent.Find(name) as RectTransform;
    //         if (child != null)
    //         {
    //             return child;
    //         }

    //         var gameObject = new GameObject(name, typeof(RectTransform));
    //         var rectTransform = gameObject.GetComponent<RectTransform>();
    //         rectTransform.SetParent(parent, false);
    //         rectTransform.localScale = Vector3.one;
    //         return rectTransform;
    //     }

    //     private static void StretchToFill(RectTransform rectTransform)
    //     {
    //         rectTransform.anchorMin = Vector2.zero;
    //         rectTransform.anchorMax = Vector2.one;
    //         rectTransform.pivot = new Vector2(0.5f, 0.5f);
    //         rectTransform.offsetMin = Vector2.zero;
    //         rectTransform.offsetMax = Vector2.zero;
    //         rectTransform.anchoredPosition = Vector2.zero;
    //     }

    //     private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    //     {
    //         var component = gameObject.GetComponent<T>();
    //         if (component == null)
    //         {
    //             component = gameObject.AddComponent<T>();
    //         }

    //         return component;
    //     }
    // }
}
