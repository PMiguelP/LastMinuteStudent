using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip musicClip;
    [Header("Volume (fallback if SettingsManager absent)")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;

    private AudioSource _source;
    private SettingsManager _settings;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _source = gameObject.AddComponent<AudioSource>();
        _source.clip = musicClip;
        _source.loop = true;
        _source.playOnAwake = false;

    }

    private void OnEnable()
    {
        BindSettings();

        ApplyVolume();

        if (musicClip != null)
        {
            if (_source != null && !_source.isPlaying)
                _source.Play();
        }
    }

    private void OnDisable()
    {
        if (_settings != null)
            _settings.OnSettingsChanged -= ApplyVolume;

        _settings = null;
    }

    private void Start()
    {
        BindSettings();
        ApplyVolume();

        if (musicClip != null && _source != null && !_source.isPlaying)
            _source.Play();
    }

    private void BindSettings()
    {
        SettingsManager currentSettings = SettingsManager.Instance;
        if (_settings == currentSettings) return;

        if (_settings != null)
            _settings.OnSettingsChanged -= ApplyVolume;

        _settings = currentSettings;
        if (_settings != null)
            _settings.OnSettingsChanged += ApplyVolume;
    }

    private void ApplyVolume()
    {
        if (_source == null) return;
        _source.volume = _settings != null ? _settings.MusicVolume : musicVolume;
    }

    public float GetVolume()
    {
        return _settings != null ? _settings.MusicVolume : musicVolume;
    }

    public void SetVolume(float value)
    {
        if (_settings == null)
        {
            BindSettings();
        }

        if (_settings != null)
        {
            _settings.SetMusicVolume(value);
            return;
        }

        if (_source != null)
        {
            _source.volume = Mathf.Clamp01(value);
            musicVolume = _source.volume;
        }
    }
}
