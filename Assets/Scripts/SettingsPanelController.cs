using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class SettingsPanelController : MonoBehaviour
{
    private MainMenuController _menuController;
    private SettingsManager _settings;
    private MusicManager _musicManager;
    private GameAudioManager _audioManager;
    private GameParticleManager _particleManager;
    private Action _onClose;

    private Slider _musicSlider;
    private Slider _sfxSlider;
    private Toggle _particlesToggle;
    private Text _musicValueText;
    private Text _sfxValueText;
    private Text _particlesValueText;
    private Image _particlesToggleBackground;
    private Image _particlesCheckmarkImage;

    private static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.75f);
    private static readonly Color CardColor = new Color(0.03f, 0.03f, 0.07f, 0.98f);
    private static readonly Color AccentColor = new Color(0.95f, 0.82f, 0.04f, 1f);
    private static readonly Color AccentDarkColor = new Color(0.10f, 0.10f, 0.14f, 1f);
    private static readonly Color TitleColor = new Color(1f, 0.94f, 0.20f, 1f);

    public void Initialize(MainMenuController menuController, SettingsManager settings, Action onClose)
    {
        _menuController = menuController;
        _settings = settings;
        _musicManager = MusicManager.Instance;
        _audioManager = GameAudioManager.Instance;
        _particleManager = GameParticleManager.Instance;
        _onClose = onClose;

        BuildUI();
        RefreshFromSettings();
    }

    private void OnEnable()
    {
        BindSettings();
        RefreshFromSettings();
    }

    private void OnDisable()
    {
        if (_settings != null)
        {
            _settings.OnSettingsChanged -= RefreshFromSettings;
        }
    }

    private void BindSettings()
    {
        if (_settings == null)
        {
            _settings = SettingsManager.Instance;
        }

        _musicManager = MusicManager.Instance;
        _audioManager = GameAudioManager.Instance;
        _particleManager = GameParticleManager.Instance;

        if (_settings != null)
        {
            _settings.OnSettingsChanged -= RefreshFromSettings;
            _settings.OnSettingsChanged += RefreshFromSettings;
        }
    }

    private void BuildUI()
    {
        ClearChildren();

        Image overlay = GetComponent<Image>();
        if (overlay == null)
        {
            overlay = gameObject.AddComponent<Image>();
        }
        overlay.color = OverlayColor;
        overlay.raycastTarget = true;

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        RectTransform root = transform as RectTransform;
        if (root != null)
        {
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
        }

        GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        card.transform.SetParent(transform, false);

        Image cardImage = card.GetComponent<Image>();
        cardImage.color = CardColor;

        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.anchorMin = cardRt.anchorMax = cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(430f, 0f);

        VerticalLayoutGroup cardLayout = card.GetComponent<VerticalLayoutGroup>();
        cardLayout.padding = new RectOffset(28, 28, 24, 24);
        cardLayout.spacing = 10f;
        cardLayout.childAlignment = TextAnchor.UpperCenter;
        cardLayout.childControlWidth = true;
        cardLayout.childControlHeight = true;
        cardLayout.childForceExpandWidth = true;
        cardLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = card.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        LayoutElement cardElement = card.GetComponent<LayoutElement>();
        cardElement.preferredWidth = 430f;

        CreateTitle(card.transform);
        CreateSliderRow(card.transform, "Music Volume", GetMusicVolume(), OnMusicChanged, out _musicSlider, out _musicValueText);
        CreateSliderRow(card.transform, "SFX Volume", GetSfxVolume(), OnSfxChanged, out _sfxSlider, out _sfxValueText);
        CreateParticlesToggleRow(card.transform);
        CreateActionRow(card.transform);
    }

    private void CreateTitle(Transform parent)
    {
        GameObject title = CreateText(parent, "Settings", 28, FontStyle.Bold, TitleColor, TextAnchor.MiddleCenter);
        LayoutElement element = title.GetComponent<LayoutElement>();
        element.preferredHeight = 38f;
    }

    private void CreateSliderRow(Transform parent, string label, float defaultValue, Action<float> onChanged, out Slider slider, out Text valueText)
    {
        GameObject row = new GameObject(label + "Row", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);

        VerticalLayoutGroup layout = row.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        LayoutElement rowElement = row.GetComponent<LayoutElement>();
        rowElement.preferredHeight = 66f;

        CreateText(row.transform, label, 15, FontStyle.Bold, AccentColor, TextAnchor.MiddleLeft);

        GameObject sliderObject = new GameObject(label + "Slider", typeof(RectTransform), typeof(Image), typeof(Slider), typeof(LayoutElement));
        sliderObject.transform.SetParent(row.transform, false);

        LayoutElement sliderElement = sliderObject.GetComponent<LayoutElement>();
        sliderElement.preferredHeight = 22f;

        Image background = sliderObject.GetComponent<Image>();
        background.color = new Color(0.15f, 0.15f, 0.20f, 1f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.offsetMin = new Vector2(10f, 0f);
        fillAreaRt.offsetMax = new Vector2(-10f, 0f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = AccentColor;
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObject.transform, false);
        RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(10f, 0f);
        handleAreaRt.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImage = handle.GetComponent<Image>();
        handleImage.color = Color.white;
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.anchorMin = handleRt.anchorMax = new Vector2(0.5f, 0.5f);
        handleRt.sizeDelta = new Vector2(20f, 20f);

        slider = sliderObject.GetComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImage;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.direction = Slider.Direction.LeftToRight;
        slider.SetValueWithoutNotify(defaultValue);

        Text valueTextLocal = CreateText(row.transform, FormatPercent(defaultValue), 13, FontStyle.Normal, Color.white, TextAnchor.MiddleRight).GetComponent<Text>();
        valueTextLocal.raycastTarget = false;
        valueText = valueTextLocal;

        slider.onValueChanged.AddListener(v =>
        {
            valueTextLocal.text = FormatPercent(v);
            onChanged?.Invoke(v);
        });
    }

    private void CreateParticlesToggleRow(Transform parent)
    {
        GameObject row = new GameObject("ParticlesRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(Button), typeof(Image));
        row.transform.SetParent(parent, false);

        Image rowImage = row.GetComponent<Image>();
        rowImage.color = new Color(1f, 1f, 1f, 0.04f);

        Button rowButton = row.GetComponent<Button>();
        rowButton.transition = Selectable.Transition.ColorTint;
        ColorBlock rowColors = rowButton.colors;
        rowColors.normalColor = new Color(1f, 1f, 1f, 0.06f);
        rowColors.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
        rowColors.pressedColor = new Color(1f, 1f, 1f, 0.18f);
        rowColors.selectedColor = rowColors.highlightedColor;
        rowColors.disabledColor = new Color(1f, 1f, 1f, 0.03f);
        rowButton.colors = rowColors;
        rowButton.onClick.AddListener(() =>
        {
            if (_particlesToggle == null)
            {
                return;
            }

            _particlesToggle.isOn = !_particlesToggle.isOn;
        });

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        LayoutElement rowElement = row.GetComponent<LayoutElement>();
        rowElement.preferredHeight = 42f;

        GameObject labelObject = CreateText(row.transform, "Particles", 15, FontStyle.Bold, AccentColor, TextAnchor.MiddleLeft);
        LayoutElement labelElement = labelObject.GetComponent<LayoutElement>();
        labelElement.preferredWidth = 140f;

        GameObject toggleObject = new GameObject("ParticlesToggle", typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
        toggleObject.transform.SetParent(row.transform, false);

        LayoutElement toggleElement = toggleObject.GetComponent<LayoutElement>();
        toggleElement.preferredWidth = 22f;
        toggleElement.preferredHeight = 22f;

        GameObject backgroundObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObj.transform.SetParent(toggleObject.transform, false);
        RectTransform backgroundRt = backgroundObj.GetComponent<RectTransform>();
        Stretch(backgroundRt);
        Image backgroundImage = backgroundObj.GetComponent<Image>();
        backgroundImage.color = AccentDarkColor;
        _particlesToggleBackground = backgroundImage;

        GameObject checkmarkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkmarkObj.transform.SetParent(backgroundObj.transform, false);
        RectTransform checkmarkRt = checkmarkObj.GetComponent<RectTransform>();
        checkmarkRt.anchorMin = new Vector2(0.2f, 0.2f);
        checkmarkRt.anchorMax = new Vector2(0.8f, 0.8f);
        checkmarkRt.offsetMin = Vector2.zero;
        checkmarkRt.offsetMax = Vector2.zero;
        Image checkmarkImage = checkmarkObj.GetComponent<Image>();
        checkmarkImage.color = AccentColor;
        _particlesCheckmarkImage = checkmarkImage;

        _particlesToggle = toggleObject.GetComponent<Toggle>();
        _particlesToggle.targetGraphic = backgroundImage;
        _particlesToggle.graphic = checkmarkImage;
        _particlesToggle.toggleTransition = Toggle.ToggleTransition.None;
        _particlesToggle.isOn = IsParticlesEnabled();
        _particlesToggle.onValueChanged.AddListener(OnParticlesToggled);

        _particlesValueText = CreateText(row.transform, IsParticlesEnabled() ? "ON" : "OFF", 13, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft).GetComponent<Text>();
        _particlesValueText.raycastTarget = false;

        UpdateParticlesVisual(_particlesToggle.isOn);
    }

    private void CreateActionRow(Transform parent)
    {
        GameObject row = new GameObject("ActionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        LayoutElement rowElement = row.GetComponent<LayoutElement>();
        rowElement.preferredHeight = 40f;

        CreateButton(row.transform, "Reset", OnResetPressed);
        CreateButton(row.transform, "Close", OnClosePressed);
    }

    private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = AccentDarkColor;

        LayoutElement element = buttonObject.GetComponent<LayoutElement>();
        element.preferredHeight = 34f;

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        Text text = CreateText(buttonObject.transform, label, 16, FontStyle.Bold, AccentColor, TextAnchor.MiddleCenter).GetComponent<Text>();
        text.raycastTarget = false;
        Stretch(text.rectTransform);
    }

    private GameObject CreateText(Transform parent, string textValue, int size, FontStyle style, Color color, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(textValue, typeof(RectTransform), typeof(Text), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = textValue;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;

        LayoutElement element = textObject.GetComponent<LayoutElement>();
        element.preferredHeight = size + 8f;

        return textObject;
    }

    private void OnMusicChanged(float value)
    {
        if (_musicManager != null)
        {
            _musicManager.SetVolume(value);
            return;
        }

        _settings?.SetMusicVolume(value);
    }

    private void OnSfxChanged(float value)
    {
        if (_audioManager != null)
        {
            _audioManager.SetVolume(value);
            return;
        }

        _settings?.SetSFXVolume(value);
    }

    private void OnParticlesToggled(bool isOn)
    {
        UpdateParticlesVisual(isOn);

        if (_particleManager != null)
        {
            _particleManager.SetEnabled(isOn);
            RefreshFromSettings();
            return;
        }

        if (_settings != null)
        {
            _settings.SetParticlesEnabled(isOn);
            RefreshFromSettings();
        }
    }

    private void OnResetPressed()
    {
        _settings?.ResetToDefaults();
        RefreshFromSettings();
    }

    private void OnClosePressed()
    {
        _onClose?.Invoke();
    }

    private void RefreshFromSettings()
    {
        if (_settings == null)
        {
            _settings = SettingsManager.Instance;
        }

        if (_settings == null)
        {
            return;
        }

        if (_musicSlider != null)
        {
            _musicSlider.SetValueWithoutNotify(GetMusicVolume());
        }

        if (_musicValueText != null)
        {
            _musicValueText.text = FormatPercent(GetMusicVolume());
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.SetValueWithoutNotify(GetSfxVolume());
        }

        if (_sfxValueText != null)
        {
            _sfxValueText.text = FormatPercent(GetSfxVolume());
        }

        if (_particlesToggle != null)
        {
            bool isOn = IsParticlesEnabled();
            _particlesToggle.SetIsOnWithoutNotify(isOn);
            UpdateParticlesVisual(isOn);
        }
    }

    private float GetMusicVolume()
    {
        if (_musicManager != null)
        {
            return _musicManager.GetVolume();
        }

        return _settings != null ? _settings.MusicVolume : SettingsManager.DefaultMusicVolume;
    }

    private float GetSfxVolume()
    {
        if (_audioManager != null)
        {
            return _audioManager.GetVolume();
        }

        return _settings != null ? _settings.SFXVolume : SettingsManager.DefaultSFXVolume;
    }

    private bool IsParticlesEnabled()
    {
        if (_particleManager != null)
        {
            return _particleManager.IsEnabled();
        }

        return _settings != null ? _settings.ParticlesEnabled : SettingsManager.DefaultParticlesEnabled;
    }

    private string FormatPercent(float value)
    {
        return Mathf.RoundToInt(value * 100f) + "%";
    }

    private void UpdateParticlesVisual(bool isOn)
    {
        if (_particlesValueText != null)
        {
            _particlesValueText.text = isOn ? "ON" : "OFF";
            _particlesValueText.color = isOn ? AccentColor : new Color(0.72f, 0.72f, 0.76f, 1f);
        }

        if (_particlesToggleBackground != null)
        {
            _particlesToggleBackground.color = isOn
                ? new Color(0.18f, 0.42f, 0.20f, 1f)
                : new Color(0.32f, 0.12f, 0.12f, 1f);
        }

        if (_particlesCheckmarkImage != null)
        {
            _particlesCheckmarkImage.color = isOn ? AccentColor : new Color(0.45f, 0.45f, 0.45f, 1f);
        }
    }

    private void ClearChildren()
    {
        for (int index = transform.childCount - 1; index >= 0; index--)
        {
            Destroy(transform.GetChild(index).gameObject);
        }
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
