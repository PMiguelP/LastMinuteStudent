using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class MainMenuUIBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuController menuController;

    [Header("Text")]
    [SerializeField] private string gameTitle = "Last Minute Student";
    [SerializeField] private string startButtonText = "Start";
    [SerializeField] private string restartButtonText = "Restart";
    [SerializeField] private string quitButtonText = "Quit";
    [SerializeField] private string subtitleText = "Run, dodge, and stay ahead.";
    [SerializeField] private string pressStartText = "Press Enter or Space to start";
    [SerializeField] private string gameOverTitle = "Game Over";
    [SerializeField] private string gameOverSubtitle = "Caught before making it to class.";
    [SerializeField] private string controlsText = "Controls\nA / D or Left / Right  Change lane\nSpace  Jump\n\nObjective\nAvoid obstacles and keep moving.";

    [Header("Look")]
    [SerializeField] private Vector2 panelSize = new Vector2(620f, 520f);
    [SerializeField] private Vector2 gameOverPanelSize = new Vector2(560f, 420f);
    [SerializeField] private Color backgroundColor = new Color(0.05f, 0.08f, 0.12f, 1f);
    [SerializeField] private Color backgroundGlowColor = new Color(0.72f, 0.5f, 0.16f, 0.12f);
    [SerializeField] private Color overlayColor = new Color(0.03f, 0.05f, 0.08f, 0.72f);
    [SerializeField] private Color panelColor = new Color(0.08f, 0.12f, 0.17f, 0.94f);
    [SerializeField] private Color infoPanelColor = new Color(0.12f, 0.16f, 0.22f, 0.95f);
    [SerializeField] private Color accentColor = new Color(0.89f, 0.61f, 0.18f, 1f);
    [SerializeField] private Color buttonColor = new Color(0.89f, 0.61f, 0.18f, 1f);
    [SerializeField] private Color secondaryButtonColor = new Color(0.24f, 0.3f, 0.37f, 1f);

    private void Awake()
    {
        if (menuController == null)
        {
            menuController = GetComponent<MainMenuController>();
        }

        if (menuController == null)
        {
            return;
        }

        if (menuController.GetMainMenuPanel() != null)
        {
            return;
        }

        EnsureEventSystemExists();
        BuildMenu();
    }

    private void BuildMenu()
    {
        Canvas canvas = CreateCanvas();
        GameObject scorePanel = CreateScorePanel(canvas.transform);
        Text scoreValue = scorePanel.transform.Find("ScoreValue").GetComponent<Text>();

        GameObject overlay = CreateUIObject("MainMenuPanel", canvas.transform);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = overlayColor;
        overlay.AddComponent<CanvasGroupFadeIn>();

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        StretchToFullScreen(overlayRect);

        CreateBackground(overlay.transform);

        GameObject panel = CreateUIObject("Card", overlay.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = panelColor;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;
        panelRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 30, 30);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        CreateAccentBar(panel.transform);
        CreateTitle(panel.transform);
        CreateSubtitle(panel.transform);
        CreatePressStartHint(panel.transform);
        CreateInfoPanel(panel.transform);
        CreateButtonRow(panel.transform);

        GameObject gameOverOverlay = CreateGameOverOverlay(canvas.transform);

        menuController.SetMainMenuPanel(overlay);
        menuController.SetGameOverPanel(gameOverOverlay);
        menuController.SetScorePanel(scorePanel);
        menuController.SetScoreText(scoreValue);
        scorePanel.SetActive(false);
    }

    private Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private void CreateAccentBar(Transform parent)
    {
        GameObject accent = CreateUIObject("Accent", parent);
        Image image = accent.AddComponent<Image>();
        image.color = accentColor;

        LayoutElement layoutElement = accent.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 8f;
    }

    private void CreateTitle(Transform parent)
    {
        Text title = CreateText(parent, "Title", gameTitle, 54, FontStyle.Bold);
        title.alignment = TextAnchor.MiddleLeft;
        title.color = Color.white;

        LayoutElement layoutElement = title.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 90f;
    }

    private void CreateSubtitle(Transform parent)
    {
        Text subtitle = CreateText(parent, "Subtitle", subtitleText, 24, FontStyle.Normal);
        subtitle.alignment = TextAnchor.UpperLeft;
        subtitle.color = new Color(0.84f, 0.88f, 0.92f, 0.92f);

        LayoutElement layoutElement = subtitle.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 62f;
    }

    private void CreatePressStartHint(Transform parent)
    {
        Text hint = CreateText(parent, "PressStartHint", pressStartText, 21, FontStyle.Bold);
        hint.alignment = TextAnchor.MiddleLeft;
        hint.color = new Color(0.98f, 0.84f, 0.48f, 0.96f);

        LayoutElement layoutElement = hint.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 40f;
    }

    private void CreateInfoPanel(Transform parent)
    {
        GameObject infoPanel = CreateUIObject("InfoPanel", parent);
        Image image = infoPanel.AddComponent<Image>();
        image.color = infoPanelColor;

        LayoutElement layoutElement = infoPanel.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 190f;

        VerticalLayoutGroup layout = infoPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 20, 20);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        Text sectionTitle = CreateText(infoPanel.transform, "InfoTitle", "How To Play", 22, FontStyle.Bold);
        sectionTitle.alignment = TextAnchor.MiddleLeft;
        sectionTitle.color = Color.white;
        LayoutElement titleLayout = sectionTitle.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 30f;

        Text body = CreateText(infoPanel.transform, "InfoBody", controlsText, 20, FontStyle.Normal);
        body.alignment = TextAnchor.UpperLeft;
        body.color = new Color(0.87f, 0.9f, 0.95f, 0.96f);
        LayoutElement bodyLayout = body.gameObject.AddComponent<LayoutElement>();
        bodyLayout.preferredHeight = 120f;
    }

    private void CreateButtonRow(Transform parent)
    {
        GameObject row = CreateUIObject("ButtonRow", parent);
        HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 78f;

        CreateButton(row.transform, "StartButton", startButtonText, buttonColor, menuController.StartGame);
        CreateButton(row.transform, "QuitButton", quitButtonText, secondaryButtonColor, menuController.QuitGame);
    }

    private GameObject CreateGameOverOverlay(Transform parent)
    {
        GameObject overlay = CreateUIObject("GameOverPanel", parent);
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = overlayColor;

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        StretchToFullScreen(overlayRect);

        overlay.AddComponent<CanvasGroupFadeIn>();
        CreateBackground(overlay.transform);

        GameObject panel = CreateUIObject("GameOverCard", overlay.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = panelColor;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = gameOverPanelSize;
        panelRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 30, 30);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        CreateAccentBar(panel.transform);

        Text title = CreateText(panel.transform, "GameOverTitle", gameOverTitle, 50, FontStyle.Bold);
        title.alignment = TextAnchor.MiddleLeft;
        title.color = Color.white;
        LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 84f;

        Text subtitle = CreateText(panel.transform, "GameOverSubtitle", gameOverSubtitle, 24, FontStyle.Normal);
        subtitle.alignment = TextAnchor.UpperLeft;
        subtitle.color = new Color(0.84f, 0.88f, 0.92f, 0.92f);
        LayoutElement subtitleLayout = subtitle.gameObject.AddComponent<LayoutElement>();
        subtitleLayout.preferredHeight = 52f;

        Text scoreLabel = CreateText(panel.transform, "FinalScoreLabel", "Final Score", 22, FontStyle.Bold);
        scoreLabel.alignment = TextAnchor.MiddleLeft;
        scoreLabel.color = new Color(0.98f, 0.84f, 0.48f, 0.96f);
        LayoutElement labelLayout = scoreLabel.gameObject.AddComponent<LayoutElement>();
        labelLayout.preferredHeight = 30f;

        Text scoreValue = CreateText(panel.transform, "FinalScoreValue", "0000", 52, FontStyle.Bold);
        scoreValue.alignment = TextAnchor.MiddleLeft;
        scoreValue.color = Color.white;
        LayoutElement valueLayout = scoreValue.gameObject.AddComponent<LayoutElement>();
        valueLayout.preferredHeight = 72f;

        Text hint = CreateText(panel.transform, "RestartHint", "Press Enter or Space to restart", 20, FontStyle.Normal);
        hint.alignment = TextAnchor.MiddleLeft;
        hint.color = new Color(0.87f, 0.9f, 0.95f, 0.88f);
        LayoutElement hintLayout = hint.gameObject.AddComponent<LayoutElement>();
        hintLayout.preferredHeight = 34f;

        GameObject row = CreateUIObject("GameOverButtons", panel.transform);
        HorizontalLayoutGroup rowLayoutGroup = row.AddComponent<HorizontalLayoutGroup>();
        rowLayoutGroup.spacing = 14f;
        rowLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        rowLayoutGroup.childControlHeight = false;
        rowLayoutGroup.childControlWidth = true;
        rowLayoutGroup.childForceExpandHeight = false;
        rowLayoutGroup.childForceExpandWidth = true;

        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 78f;

        CreateButton(row.transform, "RestartButton", restartButtonText, buttonColor, menuController.RestartGame);
        CreateButton(row.transform, "GameOverQuitButton", quitButtonText, secondaryButtonColor, menuController.QuitGame);

        overlay.SetActive(false);
        menuController.SetGameOverScoreText(scoreValue);
        return overlay;
    }

    private void CreateBackground(Transform parent)
    {
        GameObject background = CreateUIObject("Background", parent);
        Image image = background.AddComponent<Image>();
        image.color = backgroundColor;

        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        StretchToFullScreen(backgroundRect);

        GameObject glow = CreateUIObject("BackgroundGlow", background.transform);
        Image glowImage = glow.AddComponent<Image>();
        glowImage.color = backgroundGlowColor;

        RectTransform glowRect = glow.GetComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.62f, 0.2f);
        glowRect.anchorMax = new Vector2(1.02f, 1.05f);
        glowRect.offsetMin = Vector2.zero;
        glowRect.offsetMax = Vector2.zero;

        GameObject stripe = CreateUIObject("AccentStripe", background.transform);
        Image stripeImage = stripe.AddComponent<Image>();
        stripeImage.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.16f);

        RectTransform stripeRect = stripe.GetComponent<RectTransform>();
        stripeRect.anchorMin = new Vector2(0f, 0f);
        stripeRect.anchorMax = new Vector2(0f, 1f);
        stripeRect.pivot = new Vector2(0f, 0.5f);
        stripeRect.sizeDelta = new Vector2(18f, 0f);
        stripeRect.anchoredPosition = new Vector2(42f, 0f);
    }

    private GameObject CreateScorePanel(Transform parent)
    {
        GameObject scorePanel = CreateUIObject("ScorePanel", parent);
        Image image = scorePanel.AddComponent<Image>();
        image.color = new Color(0.08f, 0.12f, 0.17f, 0.86f);

        RectTransform scoreRect = scorePanel.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(1f, 1f);
        scoreRect.anchorMax = new Vector2(1f, 1f);
        scoreRect.pivot = new Vector2(1f, 1f);
        scoreRect.sizeDelta = new Vector2(240f, 118f);
        scoreRect.anchoredPosition = new Vector2(-42f, -38f);

        VerticalLayoutGroup layout = scorePanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 18, 18);
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        Text label = CreateText(scorePanel.transform, "ScoreLabel", "Score", 22, FontStyle.Bold);
        label.alignment = TextAnchor.MiddleLeft;
        label.color = new Color(0.98f, 0.84f, 0.48f, 0.96f);
        LayoutElement labelLayout = label.gameObject.AddComponent<LayoutElement>();
        labelLayout.preferredHeight = 28f;

        Text value = CreateText(scorePanel.transform, "ScoreValue", "0000", 48, FontStyle.Bold);
        value.alignment = TextAnchor.MiddleLeft;
        value.color = Color.white;
        LayoutElement valueLayout = value.gameObject.AddComponent<LayoutElement>();
        valueLayout.preferredHeight = 56f;

        return scorePanel;
    }

    private void CreateButton(Transform parent, string objectName, string label, Color backgroundColor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = backgroundColor;

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = backgroundColor;
        colors.highlightedColor = backgroundColor * 1.08f;
        colors.pressedColor = backgroundColor * 0.92f;
        colors.selectedColor = backgroundColor;
        colors.disabledColor = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.5f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0f, 72f);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 72f;
        layoutElement.preferredWidth = 0f;

        Text text = CreateText(buttonObject.transform, "Label", label, 30, FontStyle.Bold);
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        RectTransform textRect = text.GetComponent<RectTransform>();
        StretchToFullScreen(textRect);
    }

    private Text CreateText(Transform parent, string objectName, string content, int fontSize, FontStyle fontStyle)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return text;
    }

    private void EnsureEventSystemExists()
    {
        EventSystem existingEventSystem = FindFirstObjectByType<EventSystem>();

        if (existingEventSystem != null)
        {
            UpgradeEventSystem(existingEventSystem.gameObject);
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.transform.SetParent(transform, false);
    }

    private void UpgradeEventSystem(GameObject eventSystemObject)
    {
        StandaloneInputModule standaloneInputModule = eventSystemObject.GetComponent<StandaloneInputModule>();
        if (standaloneInputModule != null)
        {
            Destroy(standaloneInputModule);
        }

        if (eventSystemObject.GetComponent<InputSystemUIInputModule>() == null)
        {
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private void StretchToFullScreen(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
