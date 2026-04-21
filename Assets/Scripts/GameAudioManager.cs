using UnityEngine;

public enum SoundEvent
{
    CoinCollect,
    ObstacleHit,
    Stumble,
    SpeedBoost,
    GameOver,
    CatchSequence,
    UIClick,
    StartGame,
    Jump,
    Footstep,
}

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("Clips — assign in Inspector when ready")]
    [SerializeField] private AudioClip coinCollectClip;
    [SerializeField] private AudioClip obstacleHitClip;
    [SerializeField] private AudioClip stumbleClip;
    [SerializeField] private AudioClip speedBoostClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip catchSequenceClip;
    [SerializeField] private AudioClip uiClickClip;
    [SerializeField] private AudioClip startGameClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip footstepClip;

    [Header("Volume (fallback if SettingsManager absent)")]
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;

    private AudioSource _source;
    private SettingsManager _settings;
    private float _currentVolume = 1f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
    }

    private void OnEnable()
    {
        BindSettings();
    }

    private void OnDisable()
    {
        if (_settings != null)
        {
            _settings.OnSettingsChanged -= RefreshVolume;
        }

        _settings = null;
    }

    private void Start()
    {
        BindSettings();
    }

    private void BindSettings()
    {
        SettingsManager currentSettings = SettingsManager.Instance;
        if (_settings == currentSettings) return;

        if (_settings != null)
            _settings.OnSettingsChanged -= RefreshVolume;

        _settings = currentSettings;
        if (_settings != null)
            _settings.OnSettingsChanged += RefreshVolume;

        RefreshVolume();
    }

    private void RefreshVolume()
    {
        _currentVolume = _settings != null ? _settings.SFXVolume : sfxVolume;
    }

    public float GetVolume()
    {
        return _settings != null ? _settings.SFXVolume : sfxVolume;
    }

    public void SetVolume(float value)
    {
        if (_settings == null)
        {
            BindSettings();
        }

        if (_settings != null)
        {
            _settings.SetSFXVolume(value);
            return;
        }

        sfxVolume = Mathf.Clamp01(value);
    }

    public void Play(SoundEvent evt)
    {
        AudioClip clip = evt switch
        {
            SoundEvent.CoinCollect => coinCollectClip,
            SoundEvent.ObstacleHit => obstacleHitClip,
            SoundEvent.Stumble => stumbleClip,
            SoundEvent.SpeedBoost => speedBoostClip,
            SoundEvent.GameOver => gameOverClip,
            SoundEvent.CatchSequence => catchSequenceClip,
            SoundEvent.UIClick => uiClickClip,
            SoundEvent.StartGame => startGameClip,
            SoundEvent.Jump => jumpClip,
            SoundEvent.Footstep => footstepClip,
            _ => null,
        };

        if (clip != null)
        {
            _source.PlayOneShot(clip, _currentVolume);
        }
    }
}
