using UnityEngine;

public enum ParticleEvent
{
    CoinCollect,
    ObstacleHit,
    SpeedBoost,
    GameOver,
}

public class GameParticleManager : MonoBehaviour
{
    public static GameParticleManager Instance { get; private set; }

    [Header("Particle Prefabs — assign in Inspector when ready")]
    [SerializeField] private GameObject coinCollectFX;
    [SerializeField] private GameObject obstacleHitFX;
    [SerializeField] private GameObject speedBoostFX;
    [SerializeField] private GameObject gameOverFX;

    [Header("Enabled (fallback if SettingsManager absent)")]
    [SerializeField] private bool particlesEnabled = true;

    private SettingsManager _settings;
    private bool _particlesEnabled = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        BindSettings();
    }

    private void OnDisable()
    {
        if (_settings != null)
        {
            _settings.OnSettingsChanged -= RefreshSettings;
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
            _settings.OnSettingsChanged -= RefreshSettings;

        _settings = currentSettings;
        if (_settings != null)
            _settings.OnSettingsChanged += RefreshSettings;

        RefreshSettings();
    }

    private void RefreshSettings()
    {
        _particlesEnabled = _settings != null ? _settings.ParticlesEnabled : SettingsManager.DefaultParticlesEnabled;
        particlesEnabled = _particlesEnabled;
    }

    public bool IsEnabled()
    {
        return _settings != null ? _settings.ParticlesEnabled : particlesEnabled;
    }

    public void SetEnabled(bool enabled)
    {
        if (_settings == null)
        {
            BindSettings();
        }

        if (_settings != null)
        {
            _settings.SetParticlesEnabled(enabled);
            return;
        }

        _particlesEnabled = enabled;
        particlesEnabled = enabled;
    }

    public void PlayAt(ParticleEvent evt, Vector3 worldPosition)
    {
        GameObject prefab = evt switch
        {
            ParticleEvent.CoinCollect => coinCollectFX,
            ParticleEvent.ObstacleHit => obstacleHitFX,
            ParticleEvent.SpeedBoost => speedBoostFX,
            ParticleEvent.GameOver => gameOverFX,
            _ => null,
        };

        if (prefab == null) return;
        if (!_particlesEnabled) return;

        Destroy(Instantiate(prefab, worldPosition, Quaternion.identity), 3f);
    }
}
