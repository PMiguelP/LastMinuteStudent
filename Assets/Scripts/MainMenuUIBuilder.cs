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
    [SerializeField] private string gameTitleLine1 = "LAST MINUTE";
    [SerializeField] private string gameTitleLine2 = "STUDENT";
    [SerializeField] private string gameTagline = "Don\u2019t get caught before class.";
    [SerializeField] private string startButtonText = "\u25ba  PLAY";
    [SerializeField] private string restartButtonText = "\u25ba  PLAY AGAIN";
    [SerializeField] private string quitButtonText = "QUIT";
    [SerializeField] private string gameOverTitle = "CAUGHT!";
    [SerializeField] private string gameOverSubtitle = "Your professor got you.";
    [SerializeField] private string chaserWarningText = "\u26a0  PROF IS CLOSE!";

    [Header("Colors")]
    [SerializeField] private Color accentColor   = new Color(0.95f, 0.62f, 0.07f, 1f);
    [SerializeField] private Color dangerColor   = new Color(0.92f, 0.22f, 0.22f, 1f);
    [SerializeField] private Color safeColor     = new Color(0.13f, 0.77f, 0.37f, 1f);
    [SerializeField] private Color coinColor     = new Color(1f, 0.89f, 0.16f, 1f);
    [SerializeField] private Color secondaryText = new Color(0.60f, 0.65f, 0.72f, 1f);

    

    private void Awake()
    {
        if (menuController == null)
            menuController = GetComponent<MainMenuController>();
        if (menuController == null)
            menuController = FindFirstObjectByType<MainMenuController>();
        if (menuController == null) return;

        
        Transform existing = transform.Find("UICanvas");
        if (existing != null) Destroy(existing.gameObject);

        
        menuController.ClearUIRefs();

        EnsureEventSystemExists();
        BuildMenu();
    }

    

    private void BuildMenu()
    {
        Canvas canvas = CreateCanvas();

        
        GameObject mainMenu = BuildMainMenu(canvas.transform);

        GameObject gameOver = BuildGameOver(canvas.transform,
            out Text goScore, out Text goCoins, out Text goHighScore);

        
        GameObject hud = BuildHUD(canvas.transform,
            out Text scoreText, out Text coinText,
            out Image barFill,  out Text barLabel);

        menuController.SetMainMenuPanel(mainMenu);
        menuController.SetGameOverPanel(gameOver);
        menuController.SetScorePanel(hud);
        menuController.SetScoreText(scoreText);
        menuController.SetCoinText(coinText);
        menuController.SetGameOverScoreText(goScore);
        menuController.SetGameOverCoinText(goCoins);
        menuController.SetHighScoreText(goHighScore);
        menuController.SetProximityBarFill(barFill);
        menuController.SetProximityLabel(barLabel);
    }

    

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

    

    private GameObject BuildHUD(Transform parent,
        out Text scoreText, out Text coinText,
        out Image barFill,  out Text barLabel)
    {
        GameObject hud = MakeRect("HUD", parent);
        Stretch(hud);

        Canvas hudCanvas = hud.AddComponent<Canvas>();
        hudCanvas.overrideSorting = true;
        hudCanvas.sortingOrder    = 200;
        hud.AddComponent<GraphicRaycaster>();

        
        Color rBg     = new Color(0.02f, 0.02f, 0.05f, 0.94f);   
        Color rBorder = new Color(0.95f, 0.82f, 0.04f, 1f);       
        Color rText   = new Color(1f,    0.94f, 0.20f, 1f);       
        Color rDim    = new Color(0.30f, 0.24f, 0.02f, 1f);       
        Color rGreen  = new Color(0.08f, 1f,    0.30f, 1f);       
        Color rRed    = new Color(1f,    0.12f, 0.04f, 1f);       
        const float B = 3f;                                         

        
        
        
        
        
        GameObject scoreBorder = MakeRect("ScoreBorder", hud.transform);
        scoreBorder.AddComponent<Image>().color = rBorder;
        Anchor(scoreBorder.GetComponent<RectTransform>(),
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(10f, -134f), new Vector2(260f, -6f));

        GameObject scoreInner = MakeRect("ScoreInner", scoreBorder.transform);
        scoreInner.AddComponent<Image>().color = rBg;
        Stretch(scoreInner);
        Anchor(scoreInner.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one,
            new Vector2(B, B), new Vector2(-B, -B));

        Text scoreLbl = MakeText("Label", scoreInner.transform,
            "SCORE", 16, FontStyle.Bold, rBorder);
        scoreLbl.alignment = TextAnchor.UpperLeft;
        Anchor(scoreLbl.rectTransform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(10f, -24f), new Vector2(-10f, 0f));

        scoreText = MakeText("Value", scoreInner.transform,
            "000000", 58, FontStyle.Bold, rText);
        scoreText.alignment = TextAnchor.LowerLeft;
        Anchor(scoreText.rectTransform,
            Vector2.zero, new Vector2(1f, 1f),
            new Vector2(8f, 4f), new Vector2(-8f, -26f));
        RetroOutline(scoreText.gameObject, Color.black, 3f);

        
        
        
        
        
        GameObject coinBorder = MakeRect("CoinBorder", hud.transform);
        coinBorder.AddComponent<Image>().color = rBorder;
        Anchor(coinBorder.GetComponent<RectTransform>(),
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-210f, -96f), new Vector2(-10f, -6f));

        GameObject coinInner = MakeRect("CoinInner", coinBorder.transform);
        coinInner.AddComponent<Image>().color = rBg;
        Anchor(coinInner.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one,
            new Vector2(B, B), new Vector2(-B, -B));

        Text coinLbl = MakeText("Label", coinInner.transform,
            "BOOKS", 13, FontStyle.Bold, rBorder);
        coinLbl.alignment = TextAnchor.UpperLeft;
        Anchor(coinLbl.rectTransform,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(8f, -20f), new Vector2(-8f, 0f));

        coinText = MakeText("Value", coinInner.transform,
            "\u25c6 0", 40, FontStyle.Bold, rText);
        coinText.alignment = TextAnchor.LowerLeft;
        Anchor(coinText.rectTransform,
            Vector2.zero, new Vector2(1f, 1f),
            new Vector2(8f, 4f), new Vector2(-8f, -22f));
        RetroOutline(coinText.gameObject, Color.black, 2f);

        
        
        
        
        GameObject threatPanel = MakeRect("ThreatPanel", hud.transform);
        threatPanel.AddComponent<Image>().color = rBg;
        Anchor(threatPanel.GetComponent<RectTransform>(),
            Vector2.zero, new Vector2(1f, 0f),
            new Vector2(0f, 0f), new Vector2(0f, 34f));

        
        GameObject topLine = MakeRect("TopLine", threatPanel.transform);
        topLine.AddComponent<Image>().color = rBorder;
        Anchor(topLine.GetComponent<RectTransform>(),
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -2f), new Vector2(0f, 0f));

        
        Text profLbl = MakeText("ProfLabel", threatPanel.transform,
            "PROF", 13, FontStyle.Bold, rBorder);
        profLbl.alignment = TextAnchor.MiddleCenter;
        Anchor(profLbl.rectTransform,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(4f, 0f), new Vector2(68f, 0f));

        
        Text safeLbl = MakeText("SafeLabel", threatPanel.transform,
            "SAFE", 13, FontStyle.Bold, rGreen);
        safeLbl.alignment = TextAnchor.MiddleCenter;
        Anchor(safeLbl.rectTransform,
            new Vector2(1f, 0f), new Vector2(1f, 1f),
            new Vector2(-68f, 0f), new Vector2(-4f, 0f));

        
        GameObject fillBg = MakeRect("FillBg", threatPanel.transform);
        fillBg.AddComponent<Image>().color = rDim;
        Anchor(fillBg.GetComponent<RectTransform>(),
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(72f, 4f), new Vector2(-72f, -4f));

        
        GameObject fillObj = MakeRect("Fill", fillBg.transform);
        barFill            = fillObj.AddComponent<Image>();
        barFill.color      = rGreen;
        barFill.type       = Image.Type.Filled;
        barFill.fillMethod = Image.FillMethod.Horizontal;
        barFill.fillOrigin = 0;
        barFill.fillAmount = 1f;
        Stretch(fillObj);

        
        barLabel = MakeText("BarLabel", hud.transform,
            chaserWarningText, 18, FontStyle.Bold, rRed);
        barLabel.alignment = TextAnchor.MiddleCenter;
        Anchor(barLabel.rectTransform,
            Vector2.zero, new Vector2(1f, 0f),
            new Vector2(0f, 34f), new Vector2(0f, 62f));
        RetroOutline(barLabel.gameObject, Color.black, 2f);
        barLabel.enabled = false;

        return hud;
    }

    
    private static void RetroOutline(GameObject obj, Color color, float size)
    {
        Outline o = obj.AddComponent<Outline>();
        o.effectColor    = color;
        o.effectDistance = new Vector2(size, -size);
        o.useGraphicAlpha = false;
    }

    

    private GameObject BuildMainMenu(Transform parent)
    {
        const int  B      = 3;
        Color rBg     = new Color(0.01f, 0.01f, 0.02f, 0.98f);
        Color rBorder = new Color(0.95f, 0.82f, 0.04f, 1f);
        Color rText   = new Color(1f,    0.94f, 0.20f, 1f);
        Color rDim    = new Color(0.55f, 0.48f, 0.14f, 1f);

        
        GameObject overlay = MakeRect("MainMenuPanel", parent);
        overlay.AddComponent<Image>().color = rBg;
        Stretch(overlay);
        overlay.AddComponent<CanvasGroupFadeIn>();

        
        GameObject card = MakeRect("Card", overlay.transform);
        card.AddComponent<Image>().color = rBorder;
        RectTransform cr = card.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(1f, 0.5f);
        cr.sizeDelta = new Vector2(480f, 0f);
        cr.anchoredPosition = new Vector2(-40f, 0f);
        VLG(card, 48 + B, 48 + B, 44 + B, 44 + B, 14f, TextAnchor.UpperLeft);
        AutoHeight(card);

        
        GameObject cardBg = MakeRect("CardBg", card.transform);
        cardBg.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 1f);
        cardBg.AddComponent<LayoutElement>().ignoreLayout = true;
        Anchor(cardBg.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one,
            new Vector2(B, B), new Vector2(-B, -B));

        
        Bar(card.transform, rBorder, 4f);
        Spacer(card.transform, 8f);

        
        Text t1 = Row(card.transform, gameTitleLine1, 52, FontStyle.Bold, rText, 64f);
        RetroOutline(t1.gameObject, Color.black, 3f);
        Text t2 = Row(card.transform, gameTitleLine2, 68, FontStyle.Bold, Color.white, 84f);
        RetroOutline(t2.gameObject, Color.black, 3f);
        Spacer(card.transform, 6f);

        
        Row(card.transform, gameTagline, 18, FontStyle.Normal, rDim, 28f);
        Spacer(card.transform, 14f);

        
        Bar(card.transform, new Color(rBorder.r, rBorder.g, rBorder.b, 0.35f), 1f);
        Spacer(card.transform, 18f);

        
        RetroBtn(card.transform, startButtonText,
            rBorder, Color.black, menuController.StartGame, 80f);
        Spacer(card.transform, 8f);
        RetroBtn(card.transform, quitButtonText,
            new Color(0.10f, 0.10f, 0.12f, 1f), rDim, menuController.QuitGame, 48f);
        Spacer(card.transform, 8f);

        return overlay;
    }

    

    private GameObject BuildGameOver(Transform parent,
        out Text goScore, out Text goCoins, out Text goHighScore)
    {
        const int  B      = 3;
        Color rBg     = new Color(0.01f, 0.01f, 0.02f, 0.98f);
        Color rBorder = new Color(0.95f, 0.82f, 0.04f, 1f);
        Color rRed    = new Color(1f,    0.12f, 0.04f, 1f);
        Color rDim    = new Color(0.55f, 0.48f, 0.14f, 1f);

        
        GameObject overlay = MakeRect("GameOverPanel", parent);
        overlay.AddComponent<Image>().color = rBg;
        Stretch(overlay);
        overlay.AddComponent<CanvasGroupFadeIn>();

        
        GameObject card = MakeRect("Card", overlay.transform);
        card.AddComponent<Image>().color = rRed;
        RectTransform cr = card.GetComponent<RectTransform>();
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0f, 0.5f);
        cr.sizeDelta = new Vector2(560f, 0f);
        cr.anchoredPosition = new Vector2(40f, 0f);
        VLG(card, 48 + B, 48 + B, 40 + B, 40 + B, 12f, TextAnchor.UpperLeft);
        AutoHeight(card);

        
        GameObject cardBg = MakeRect("CardBg", card.transform);
        cardBg.AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.05f, 1f);
        cardBg.AddComponent<LayoutElement>().ignoreLayout = true;
        Anchor(cardBg.GetComponent<RectTransform>(),
            Vector2.zero, Vector2.one,
            new Vector2(B, B), new Vector2(-B, -B));

        
        Bar(card.transform, rRed, 5f);
        Spacer(card.transform, 6f);

        
        Text titleText = Row(card.transform, gameOverTitle, 72, FontStyle.Bold, rRed, 88f);
        RetroOutline(titleText.gameObject, Color.black, 4f);
        Row(card.transform, gameOverSubtitle, 20, FontStyle.Normal, rDim, 28f);
        Spacer(card.transform, 12f);

        
        GameObject statsBlock = MakeRect("Stats", card.transform);
        statsBlock.AddComponent<LayoutElement>().preferredHeight = 106f;

        Text scoreLabel = MakeText("ScoreLabel", statsBlock.transform,
            "SCORE", 13, FontStyle.Bold, rBorder);
        scoreLabel.alignment = TextAnchor.UpperLeft;
        Anchor(scoreLabel.rectTransform,
            new Vector2(0f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -20f), new Vector2(0f, 0f));

        goScore = MakeText("ScoreValue", statsBlock.transform,
            "0", 54, FontStyle.Bold, Color.white);
        goScore.alignment = TextAnchor.UpperLeft;
        Anchor(goScore.rectTransform,
            new Vector2(0f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -96f), new Vector2(0f, -20f));
        RetroOutline(goScore.gameObject, Color.black, 3f);

        Text booksLabel = MakeText("BooksLabel", statsBlock.transform,
            "BOOKS", 13, FontStyle.Bold, rBorder);
        booksLabel.alignment = TextAnchor.UpperLeft;
        Anchor(booksLabel.rectTransform,
            new Vector2(0.5f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -20f), new Vector2(0f, 0f));

        goCoins = MakeText("BooksValue", statsBlock.transform,
            "0", 44, FontStyle.Bold, coinColor);
        goCoins.alignment = TextAnchor.UpperLeft;
        Anchor(goCoins.rectTransform,
            new Vector2(0.5f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -82f), new Vector2(0f, -20f));
        RetroOutline(goCoins.gameObject, Color.black, 2f);

        
        goHighScore = Row(card.transform, "Best  0", 18, FontStyle.Bold,
            new Color(0.55f, 0.60f, 0.66f), 26f);
        Spacer(card.transform, 6f);
        Row(card.transform, "Press Enter or Space to restart", 15, FontStyle.Normal,
            new Color(0.35f, 0.32f, 0.10f, 0.85f), 22f);
        Spacer(card.transform, 12f);

        
        GameObject btnRow = MakeRect("Buttons", card.transform);
        HorizontalLayoutGroup h = btnRow.AddComponent<HorizontalLayoutGroup>();
        h.spacing              = 12f;
        h.childAlignment       = TextAnchor.MiddleCenter;
        h.childControlWidth    = true;
        h.childControlHeight   = true;
        h.childForceExpandWidth  = true;
        h.childForceExpandHeight = false;
        btnRow.AddComponent<LayoutElement>().preferredHeight = 68f;

        RetroBtn(btnRow.transform, restartButtonText,
            new Color(0.95f, 0.82f, 0.04f, 1f), Color.black,
            menuController.RestartGame, 68f);
        RetroBtn(btnRow.transform, quitButtonText,
            new Color(0.10f, 0.10f, 0.12f, 1f), rDim,
            menuController.QuitGame, 68f);

        Spacer(card.transform, 8f);

        overlay.SetActive(false);
        return overlay;
    }

    private void BackgroundPill(Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject obj = MakeRect("BgPill", parent);
        obj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        Anchor(obj.GetComponent<RectTransform>(), anchorMin, anchorMax, offsetMin, offsetMax);
    }

    

    private void VLG(GameObject obj,
        int padLeft, int padRight, int padTop, int padBot,
        float spacing, TextAnchor align)
    {
        VerticalLayoutGroup v = obj.AddComponent<VerticalLayoutGroup>();
        v.padding              = new RectOffset(padLeft, padRight, padTop, padBot);
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

    private void Btn(Transform parent, string label, Color bg,
                     UnityEngine.Events.UnityAction onClick, float h)
    {
        RetroBtn(parent, label, bg, Color.white, onClick, h);
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
        cb.pressedColor     = bg * 0.75f;
        cb.selectedColor    = bg;
        cb.colorMultiplier  = 1f;
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        obj.AddComponent<LayoutElement>().preferredHeight = h;

        Text t = MakeText("Label", obj.transform, label, 27, FontStyle.Bold, textColor);
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
        t.text             = content;
        t.fontSize         = size;
        t.fontStyle        = style;
        t.color            = color;
        t.raycastTarget    = false;
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
