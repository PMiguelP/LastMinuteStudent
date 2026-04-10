using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(MainMenuController))]
public class MainMenuUIBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuController menuController;

    [Header("Text")]
    [SerializeField] private string gameTitleLine1    = "LAST MINUTE";
    [SerializeField] private string gameTitleLine2    = "STUDENT";
    [SerializeField] private string gameTagline       = "Don\u2019t get caught before class.";
    [SerializeField] private string startButtonText   = "\u25ba  PLAY";
    [SerializeField] private string restartButtonText = "\u25ba  PLAY AGAIN";
    [SerializeField] private string quitButtonText    = "QUIT";
    [SerializeField] private string gameOverTitle     = "CAUGHT!";
    [SerializeField] private string gameOverSubtitle  = "Your professor got you.";
    [SerializeField] private string chaserWarningText = "\u26a0  JULIO IS CLOSE!";

    [Header("Colors")]
    [SerializeField] private Color accentColor   = new Color(0.95f, 0.62f, 0.07f, 1f);
    [SerializeField] private Color dangerColor   = new Color(0.92f, 0.22f, 0.22f, 1f);
    [SerializeField] private Color safeColor     = new Color(0.13f, 0.77f, 0.37f, 1f);
    [SerializeField] private Color coinColor     = new Color(1f,    0.89f, 0.16f, 1f);
    [SerializeField] private Color secondaryText = new Color(0.60f, 0.65f, 0.72f, 1f);

    // Shared palette
    static readonly Color RBg     = new Color(0.02f, 0.02f, 0.06f, 0.97f);
    static readonly Color RBorder = new Color(0.95f, 0.82f, 0.04f, 1f);
    static readonly Color RText   = new Color(1f,    0.94f, 0.20f, 1f);
    static readonly Color RDim    = new Color(0.50f, 0.44f, 0.12f, 1f);
    static readonly Color RRed    = new Color(1f,    0.12f, 0.04f, 1f);
    const float B = 3f;

    // Settings panel reference (toggled at runtime)
    private GameObject _settingsPanel;

    // ── Awake ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (menuController == null) menuController = GetComponent<MainMenuController>();
        if (menuController == null) menuController = FindFirstObjectByType<MainMenuController>();
        if (menuController == null) return;

        Transform existing = transform.Find("UICanvas");
        if (existing != null) Destroy(existing.gameObject);

        menuController.ClearUIRefs();
        EnsureEventSystemExists();
        BuildMenu();
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    private void BuildMenu()
    {
        Canvas canvas = CreateCanvas();

        GameObject mainMenu = BuildMainMenu(canvas.transform);
        GameObject gameOver = BuildGameOver(canvas.transform,
            out Text goScore, out Text goCoins, out Text goHighScore, out Text goLifetimeCoins);

        GameObject hud = BuildHUD(canvas.transform,
            out Text scoreText, out Text coinText, out Text barLabel);

        _settingsPanel = BuildSettingsPanel(canvas.transform);

        // Loading overlay — topmost, starts hidden
        CanvasGroup loadingOverlay = BuildLoadingOverlay(canvas.transform);

        menuController.SetMainMenuPanel(mainMenu);
        menuController.SetGameOverPanel(gameOver);
        menuController.SetScorePanel(hud);
        menuController.SetScoreText(scoreText);
        menuController.SetCoinText(coinText);
        menuController.SetGameOverScoreText(goScore);
        menuController.SetGameOverCoinText(goCoins);
        menuController.SetHighScoreText(goHighScore);
        menuController.SetLifetimeCoinText(goLifetimeCoins);
        menuController.SetProximityBarFill(null);
        menuController.SetProximityLabel(barLabel);
        menuController.SetLoadingScreen(loadingOverlay);
    }

    // ── Canvas ────────────────────────────────────────────────────────────────

    private Canvas CreateCanvas()
    {
        GameObject obj = new GameObject("UICanvas",
            typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        obj.transform.SetParent(transform, false);

        Canvas c = obj.GetComponent<Canvas>();
        c.renderMode   = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;

        CanvasScaler s = obj.GetComponent<CanvasScaler>();
        s.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        s.referenceResolution = new Vector2(1920f, 1080f);
        s.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        s.matchWidthOrHeight  = 0.5f;

        return c;
    }

    // ── HUD ───────────────────────────────────────────────────────────────────

    private GameObject BuildHUD(Transform parent,
        out Text scoreText, out Text coinText, out Text barLabel)
    {
        GameObject hud = MakeRect("HUD", parent);
        Stretch(hud);

        Canvas hudCanvas = hud.AddComponent<Canvas>();
        hudCanvas.overrideSorting = true;
        hudCanvas.sortingOrder    = 200;
        hud.AddComponent<GraphicRaycaster>();

        // ── Score box ────────────────────────────────────────────────────
        GameObject scoreBorder = MakeRect("ScoreBorder", hud.transform);
        scoreBorder.AddComponent<Image>().color = RBorder;
        Anchor(scoreBorder.GetComponent<RectTransform>(),
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(12f, -120f), new Vector2(248f, -8f));

        GameObject scoreInner = MakeRect("ScoreInner", scoreBorder.transform);
        scoreInner.AddComponent<Image>().color = RBg;
        Anchor(scoreInner.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one, new Vector2(B, B), new Vector2(-B, -B));

        Text scoreLbl = MakeText("Label", scoreInner.transform, "SCORE", 14, FontStyle.Bold, RBorder);
        scoreLbl.alignment = TextAnchor.UpperLeft;
        Anchor(scoreLbl.rectTransform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(10f, -22f), new Vector2(-10f, 0f));

        scoreText = MakeText("Value", scoreInner.transform, "000000", 52, FontStyle.Bold, RText);
        scoreText.alignment = TextAnchor.LowerLeft;
        Anchor(scoreText.rectTransform,
            Vector2.zero, new Vector2(1f, 1f),
            new Vector2(8f, 4f), new Vector2(-8f, -24f));
        RetroOutline(scoreText.gameObject, Color.black, 2f);

        // ── Coin box ─────────────────────────────────────────────────────
        GameObject coinBorder = MakeRect("CoinBorder", hud.transform);
        coinBorder.AddComponent<Image>().color = RBorder;
        Anchor(coinBorder.GetComponent<RectTransform>(),
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-200f, -88f), new Vector2(-12f, -8f));

        GameObject coinInner = MakeRect("CoinInner", coinBorder.transform);
        coinInner.AddComponent<Image>().color = RBg;
        Anchor(coinInner.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one, new Vector2(B, B), new Vector2(-B, -B));

        Text coinLbl = MakeText("Label", coinInner.transform, "BOOKS", 12, FontStyle.Bold, RBorder);
        coinLbl.alignment = TextAnchor.UpperLeft;
        Anchor(coinLbl.rectTransform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(8f, -18f), new Vector2(-8f, 0f));

        coinText = MakeText("Value", coinInner.transform, "\u25c6 0", 36, FontStyle.Bold, RText);
        coinText.alignment = TextAnchor.LowerLeft;
        Anchor(coinText.rectTransform,
            Vector2.zero, new Vector2(1f, 1f),
            new Vector2(8f, 4f), new Vector2(-8f, -20f));
        RetroOutline(coinText.gameObject, Color.black, 2f);

        // ── "JULIO IS CLOSE!" floating label (no bar, no panel) ──────────
        barLabel = MakeText("JulioLabel", hud.transform,
            chaserWarningText, 20, FontStyle.Bold, RRed);
        barLabel.alignment = TextAnchor.MiddleCenter;
        Anchor(barLabel.rectTransform,
            new Vector2(0.2f, 0f), new Vector2(0.8f, 0f),
            new Vector2(0f, 10f), new Vector2(0f, 42f));
        RetroOutline(barLabel.gameObject, Color.black, 2f);
        barLabel.enabled = false;

        return hud;
    }

    // ── Main Menu ─────────────────────────────────────────────────────────────

    private GameObject BuildMainMenu(Transform parent)
    {
        GameObject overlay = MakeRect("MainMenuPanel", parent);
        overlay.AddComponent<Image>().color = new Color(0.01f, 0.01f, 0.03f, 0.97f);
        Stretch(overlay);
        overlay.AddComponent<CanvasGroupFadeIn>();

        GameObject card = MakeRect("Card", overlay.transform);
        card.AddComponent<Image>().color = RBorder;
        RectTransform cr = card.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(1f, 0.5f);
        cr.sizeDelta = new Vector2(440f, 0f);
        cr.anchoredPosition = new Vector2(-48f, 0f);
        VLG(card, 44 + B, 44 + B, 40 + B, 40 + B, 12f, TextAnchor.UpperLeft);
        AutoHeight(card);

        GameObject cardBg = MakeRect("CardBg", card.transform);
        cardBg.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.07f, 1f);
        cardBg.AddComponent<LayoutElement>().ignoreLayout = true;
        Anchor(cardBg.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one, new Vector2(B, B), new Vector2(-B, -B));

        Bar(card.transform, RBorder, 4f);
        Spacer(card.transform, 10f);

        Text t1 = Row(card.transform, gameTitleLine1, 46, FontStyle.Bold, RText, 58f);
        RetroOutline(t1.gameObject, Color.black, 3f);
        Text t2 = Row(card.transform, gameTitleLine2, 62, FontStyle.Bold, Color.white, 76f);
        RetroOutline(t2.gameObject, Color.black, 3f);
        Spacer(card.transform, 4f);

        Row(card.transform, gameTagline, 16, FontStyle.Normal, RDim, 24f);
        Spacer(card.transform, 16f);

        Bar(card.transform, new Color(RBorder.r, RBorder.g, RBorder.b, 0.30f), 1f);
        Spacer(card.transform, 16f);

        RetroBtn(card.transform, startButtonText,   RBorder, Color.black, menuController.StartGame, 72f);
        Spacer(card.transform, 8f);
        RetroBtn(card.transform, "\u2699  SETTINGS", new Color(0.12f, 0.12f, 0.16f, 1f), RDim, OpenSettings, 44f);
        Spacer(card.transform, 6f);
        RetroBtn(card.transform, quitButtonText,    new Color(0.10f, 0.10f, 0.12f, 1f), RDim, menuController.QuitGame, 44f);
        Spacer(card.transform, 8f);

        return overlay;
    }

    private void OpenSettings()
    {
        if (_settingsPanel != null)
            _settingsPanel.SetActive(!_settingsPanel.activeSelf);
    }

    // ── Settings Panel ────────────────────────────────────────────────────────

    private GameObject BuildSettingsPanel(Transform parent)
    {
        // Full-screen dark overlay
        GameObject overlay = MakeRect("SettingsPanel", parent);
        overlay.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        Stretch(overlay);

        // Card
        GameObject card = MakeRect("Card", overlay.transform);
        card.AddComponent<Image>().color = RBorder;
        RectTransform cr = card.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0.5f, 0.5f);
        cr.sizeDelta = new Vector2(420f, 0f);
        cr.anchoredPosition = Vector2.zero;
        VLG(card, 40 + B, 40 + B, 36 + B, 36 + B, 14f, TextAnchor.UpperLeft);
        AutoHeight(card);

        GameObject cardBg = MakeRect("CardBg", card.transform);
        cardBg.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.07f, 1f);
        cardBg.AddComponent<LayoutElement>().ignoreLayout = true;
        Anchor(cardBg.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one, new Vector2(B, B), new Vector2(-B, -B));

        Bar(card.transform, RBorder, 3f);
        Spacer(card.transform, 8f);

        Text title = Row(card.transform, "\u2699  SETTINGS", 28, FontStyle.Bold, RText, 40f);
        RetroOutline(title.gameObject, Color.black, 2f);
        Spacer(card.transform, 8f);
        Bar(card.transform, new Color(RBorder.r, RBorder.g, RBorder.b, 0.25f), 1f);
        Spacer(card.transform, 10f);

        // Music Volume
        BuildSliderRow(card.transform, "Music Volume",
            SettingsManager.Instance?.MusicVolume ?? 0.6f,
            v => SettingsManager.Instance?.SetMusicVolume(v));

        Spacer(card.transform, 6f);

        // SFX Volume
        BuildSliderRow(card.transform, "SFX Volume",
            SettingsManager.Instance?.SFXVolume ?? 1f,
            v => SettingsManager.Instance?.SetSFXVolume(v));

        Spacer(card.transform, 6f);

        // Particles toggle
        BuildParticlesToggle(card.transform);

        Spacer(card.transform, 14f);
        Bar(card.transform, new Color(RBorder.r, RBorder.g, RBorder.b, 0.25f), 1f);
        Spacer(card.transform, 10f);

        RetroBtn(card.transform, "BACK", new Color(0.10f, 0.10f, 0.12f, 1f), RDim,
            () => overlay.SetActive(false), 44f);
        Spacer(card.transform, 6f);

        overlay.SetActive(false);
        return overlay;
    }

    private void BuildSliderRow(Transform parent, string label, float initialValue,
        System.Action<float> onChange)
    {
        // Label above, slider below — avoids HLG width-calculation issues
        GameObject container = MakeRect(label + "Row", parent);
        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing              = 4f;
        vlg.childControlWidth    = true;
        vlg.childControlHeight   = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        container.AddComponent<LayoutElement>().preferredHeight = 52f;

        Text lbl = MakeText("Label", container.transform, label, 15, FontStyle.Bold, RBorder);
        lbl.alignment = TextAnchor.MiddleLeft;
        lbl.raycastTarget = false;
        lbl.gameObject.AddComponent<LayoutElement>().preferredHeight = 20f;

        // Slider container
        GameObject sliderObj = MakeRect("Slider", container.transform);
        sliderObj.AddComponent<LayoutElement>().preferredHeight = 24f;

        const float handleW = 20f;

        // Background
        GameObject bg = MakeRect("Background", sliderObj.transform);
        bg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.20f, 1f);
        RectTransform bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.25f);
        bgRt.anchorMax = new Vector2(1f, 0.75f);
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;

        // Fill Area → Fill
        GameObject fillArea = MakeRect("Fill Area", sliderObj.transform);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.offsetMin = new Vector2(handleW * 0.5f, 0f);
        fillAreaRt.offsetMax = new Vector2(-handleW * 0.5f, 0f);

        GameObject fill = MakeRect("Fill", fillArea.transform);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fill.AddComponent<Image>().color = RBorder;
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;

        // Handle Slide Area → Handle
        GameObject handleArea = MakeRect("Handle Slide Area", sliderObj.transform);
        RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(handleW * 0.5f, 0f);
        handleAreaRt.offsetMax = new Vector2(-handleW * 0.5f, 0f);

        GameObject handle = MakeRect("Handle", handleArea.transform);
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;
        handleRt.sizeDelta = new Vector2(handleW, handleW);
        handleRt.anchorMin = handleRt.anchorMax = new Vector2(0.5f, 0.5f);

        // Slider component — add AFTER children exist
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect      = fillRt;
        slider.handleRect    = handleRt;
        slider.targetGraphic = handleImg;
        slider.minValue      = 0f;
        slider.maxValue      = 1f;
        slider.value         = initialValue;
        slider.wholeNumbers  = false;
        slider.direction     = Slider.Direction.LeftToRight;

        slider.onValueChanged.AddListener(v => onChange?.Invoke(v));
    }

    private void BuildParticlesToggle(Transform parent)
    {
        GameObject container = MakeRect("ParticlesRow", parent);
        HorizontalLayoutGroup h = container.AddComponent<HorizontalLayoutGroup>();
        h.spacing              = 12f;
        h.childAlignment       = TextAnchor.MiddleLeft;
        h.childControlWidth    = true;
        h.childControlHeight   = true;
        h.childForceExpandWidth  = false;
        h.childForceExpandHeight = false;
        container.AddComponent<LayoutElement>().preferredHeight = 36f;

        Text lbl = MakeText("Label", container.transform, "Particles", 15, FontStyle.Bold, RBorder);
        lbl.alignment = TextAnchor.MiddleLeft;
        lbl.raycastTarget = false;
        LayoutElement lblEl = lbl.gameObject.AddComponent<LayoutElement>();
        lblEl.minWidth       = 140f;
        lblEl.preferredWidth = 140f;
        lblEl.flexibleWidth  = 0f;

        // Track state locally — SettingsManager may not be ready during Awake
        bool current = SettingsManager.Instance?.ParticlesEnabled ?? true;
        Color onCol  = new Color(0.10f, 0.75f, 0.30f, 1f);
        Color offCol = new Color(0.45f, 0.10f, 0.10f, 1f);

        GameObject btnObj = MakeRect("Toggle", container.transform);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = current ? onCol : offCol;
        LayoutElement btnEl = btnObj.AddComponent<LayoutElement>();
        btnEl.minWidth       = 80f;
        btnEl.preferredWidth = 80f;
        btnEl.flexibleWidth  = 0f;

        Text btnTxt = MakeText("Txt", btnObj.transform, current ? "ON" : "OFF",
            16, FontStyle.Bold, Color.white);
        btnTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(btnTxt.rectTransform);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        ColorBlock cb = btn.colors;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        cb.pressedColor     = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = cb;

        btn.onClick.AddListener(() =>
        {
            current = !current;
            SettingsManager.Instance?.SetParticlesEnabled(current);
            btnImg.color = current ? onCol : offCol;
            btnTxt.text  = current ? "ON" : "OFF";
        });
    }

    // ── Game Over ─────────────────────────────────────────────────────────────

    private GameObject BuildGameOver(Transform parent,
        out Text goScore, out Text goCoins, out Text goHighScore, out Text goLifetimeCoins)
    {
        GameObject overlay = MakeRect("GameOverPanel", parent);
        overlay.AddComponent<Image>().color = new Color(0.01f, 0.01f, 0.03f, 0.97f);
        Stretch(overlay);
        overlay.AddComponent<CanvasGroupFadeIn>();

        GameObject card = MakeRect("Card", overlay.transform);
        card.AddComponent<Image>().color = RRed;
        RectTransform cr = card.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0f, 0.5f);
        cr.sizeDelta = new Vector2(520f, 0f);
        cr.anchoredPosition = new Vector2(48f, 0f);
        VLG(card, 44 + B, 44 + B, 36 + B, 36 + B, 10f, TextAnchor.UpperLeft);
        AutoHeight(card);

        GameObject cardBg = MakeRect("CardBg", card.transform);
        cardBg.AddComponent<Image>().color = new Color(0.03f, 0.03f, 0.07f, 1f);
        cardBg.AddComponent<LayoutElement>().ignoreLayout = true;
        Anchor(cardBg.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one, new Vector2(B, B), new Vector2(-B, -B));

        Bar(card.transform, RRed, 4f);
        Spacer(card.transform, 4f);

        Text titleText = Row(card.transform, gameOverTitle, 64, FontStyle.Bold, RRed, 80f);
        RetroOutline(titleText.gameObject, Color.black, 4f);
        Row(card.transform, gameOverSubtitle, 18, FontStyle.Normal, RDim, 26f);
        Spacer(card.transform, 10f);

        // Stats block
        GameObject statsBlock = MakeRect("Stats", card.transform);
        statsBlock.AddComponent<LayoutElement>().preferredHeight = 100f;

        Color rBorderC = RBorder;
        Text scoreLabel = MakeText("ScoreLabel", statsBlock.transform, "SCORE", 13, FontStyle.Bold, rBorderC);
        scoreLabel.alignment = TextAnchor.UpperLeft;
        Anchor(scoreLabel.rectTransform,
            new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(0f, 0f));

        goScore = MakeText("ScoreValue", statsBlock.transform, "0", 48, FontStyle.Bold, Color.white);
        goScore.alignment = TextAnchor.UpperLeft;
        Anchor(goScore.rectTransform,
            new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f), new Vector2(0f, -18f));
        RetroOutline(goScore.gameObject, Color.black, 3f);

        Text booksLabel = MakeText("BooksLabel", statsBlock.transform, "BOOKS", 13, FontStyle.Bold, rBorderC);
        booksLabel.alignment = TextAnchor.UpperLeft;
        Anchor(booksLabel.rectTransform,
            new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0f, -18f), new Vector2(0f, 0f));

        goCoins = MakeText("BooksValue", statsBlock.transform, "0", 40, FontStyle.Bold, coinColor);
        goCoins.alignment = TextAnchor.UpperLeft;
        Anchor(goCoins.rectTransform,
            new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(0f, -76f), new Vector2(0f, -18f));
        RetroOutline(goCoins.gameObject, Color.black, 2f);

        // Total lifetime books row
        GameObject totalRow = MakeRect("TotalRow", card.transform);
        HorizontalLayoutGroup totalH = totalRow.AddComponent<HorizontalLayoutGroup>();
        totalH.spacing = 6f;
        totalH.childAlignment = TextAnchor.MiddleLeft;
        totalH.childControlHeight = true;
        totalRow.AddComponent<LayoutElement>().preferredHeight = 22f;

        MakeText("TotalLabel", totalRow.transform,
            "TOTAL BOOKS:", 14, FontStyle.Bold, RBorder).raycastTarget = false;
        goLifetimeCoins = MakeText("TotalValue", totalRow.transform,
            "0", 14, FontStyle.Bold, coinColor);
        goLifetimeCoins.raycastTarget = false;

        goHighScore = Row(card.transform, "Best  0", 17, FontStyle.Bold,
            new Color(0.55f, 0.60f, 0.66f), 24f);
        Spacer(card.transform, 4f);
        Row(card.transform, "Press Enter or Space to restart", 14, FontStyle.Normal,
            new Color(0.32f, 0.29f, 0.09f, 0.85f), 20f);
        Spacer(card.transform, 10f);

        GameObject btnRow = MakeRect("Buttons", card.transform);
        HorizontalLayoutGroup h = btnRow.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 10f;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = false;
        btnRow.AddComponent<LayoutElement>().preferredHeight = 60f;

        RetroBtn(btnRow.transform, restartButtonText, RBorder, Color.black, menuController.RestartGame, 60f);
        RetroBtn(btnRow.transform, quitButtonText, new Color(0.10f, 0.10f, 0.12f, 1f), RDim, menuController.QuitGame, 60f);

        Spacer(card.transform, 6f);

        overlay.SetActive(false);
        return overlay;
    }

    // ── Loading overlay ───────────────────────────────────────────────────────

    private CanvasGroup BuildLoadingOverlay(Transform parent)
    {
        GameObject obj = MakeRect("LoadingOverlay", parent);
        obj.AddComponent<Image>().color = Color.black;
        Stretch(obj);

        Canvas c = obj.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder    = 999;

        CanvasGroup cg = obj.AddComponent<CanvasGroup>();
        cg.alpha          = 0f;
        cg.blocksRaycasts = false;
        cg.interactable   = false;
        obj.SetActive(false);
        return cg;
    }

    // ── Layout helpers ────────────────────────────────────────────────────────

    private static void RetroOutline(GameObject obj, Color color, float size)
    {
        Outline o = obj.AddComponent<Outline>();
        o.effectColor    = color;
        o.effectDistance = new Vector2(size, -size);
        o.useGraphicAlpha = false;
    }

    private void VLG(GameObject obj,
        float padLeft, float padRight, float padTop, float padBot,
        float spacing, TextAnchor align)
    {
        VerticalLayoutGroup v = obj.AddComponent<VerticalLayoutGroup>();
        v.padding              = new RectOffset((int)padLeft, (int)padRight, (int)padTop, (int)padBot);
        v.spacing              = spacing;
        v.childAlignment       = align;
        v.childControlWidth    = true;
        v.childControlHeight   = true;
        v.childForceExpandWidth  = true;
        v.childForceExpandHeight = false;
    }

    private void AutoHeight(GameObject obj)
    {
        ContentSizeFitter f = obj.AddComponent<ContentSizeFitter>();
        f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void Bar(Transform parent, Color color, float h)
    {
        GameObject obj = MakeRect("Bar", parent);
        obj.AddComponent<Image>().color = color;
        obj.AddComponent<LayoutElement>().preferredHeight = h;
    }

    private void Spacer(Transform parent, float h)
    {
        MakeRect("Spacer", parent).AddComponent<LayoutElement>().preferredHeight = h;
    }

    private Text Row(Transform parent, string content,
                     int size, FontStyle style, Color color, float h)
    {
        Text t = MakeText(content, parent, content, size, style, color);
        t.alignment = TextAnchor.MiddleLeft;
        t.gameObject.AddComponent<LayoutElement>().preferredHeight = h;
        return t;
    }

    private void RetroBtn(Transform parent, string label, Color bg, Color textColor,
                          UnityEngine.Events.UnityAction onClick, float h)
    {
        GameObject obj = MakeRect(label, parent);
        obj.AddComponent<Image>().color = bg;

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = bg;
        cb.highlightedColor = bg * 1.20f;
        cb.pressedColor     = bg * 0.72f;
        cb.selectedColor    = bg;
        cb.colorMultiplier  = 1f;
        cb.fadeDuration     = 0.08f;
        btn.colors = cb;
        btn.onClick.AddListener(() => GameAudioManager.Instance?.Play(SoundEvent.UIClick));
        btn.onClick.AddListener(onClick);

        obj.AddComponent<LayoutElement>().preferredHeight = h;

        Text t = MakeText("Label", obj.transform, label, 24, FontStyle.Bold, textColor);
        t.alignment = TextAnchor.MiddleCenter;
        Stretch(t.rectTransform);
        RetroOutline(t.gameObject, Color.black, 2f);
    }

    private GameObject MakeRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private Text MakeText(string name, Transform parent,
                          string content, int size, FontStyle style, Color color)
    {
        GameObject obj = MakeRect(name, parent);
        Text t = obj.AddComponent<Text>();
        t.text               = content;
        t.fontSize           = size;
        t.fontStyle          = style;
        t.color              = color;
        t.raycastTarget      = false;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return t;
    }

    private void Stretch(GameObject obj) => Stretch(obj.GetComponent<RectTransform>());
    private void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private void Anchor(RectTransform rt,
                        Vector2 anchorMin, Vector2 anchorMax,
                        Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    // ── Event system ──────────────────────────────────────────────────────────

    private void EnsureEventSystemExists()
    {
        EventSystem existing = FindFirstObjectByType<EventSystem>();
        if (existing != null) { UpgradeEventSystem(existing.gameObject); return; }

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule))
            .transform.SetParent(transform, false);
    }

    private void UpgradeEventSystem(GameObject obj)
    {
        StandaloneInputModule old = obj.GetComponent<StandaloneInputModule>();
        if (old != null) Destroy(old);
        if (obj.GetComponent<InputSystemUIInputModule>() == null)
            obj.AddComponent<InputSystemUIInputModule>();
    }
}
