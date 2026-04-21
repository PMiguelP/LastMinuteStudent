using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public event System.Action OnSettingsChanged;

    public const float DefaultMusicVolume = 0.6f;
    public const float DefaultSFXVolume = 1.0f;
    public const bool DefaultParticlesEnabled = true;

    const string KeyMusic = "MusicVolume";
    const string KeySFX = "SFXVolume";
    const string KeyParticles = "ParticlesEnabled";

    public float MusicVolume { get; private set; }
    public float SFXVolume { get; private set; }
    public bool ParticlesEnabled { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    private void LoadSettings()
    {
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KeyMusic, DefaultMusicVolume));
        SFXVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KeySFX, DefaultSFXVolume));
        ParticlesEnabled = PlayerPrefs.GetInt(KeyParticles, DefaultParticlesEnabled ? 1 : 0) == 1;
    }

    private void SaveSettings()
    {
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (Mathf.Approximately(MusicVolume, clamped)) return;

        MusicVolume = clamped;
        PlayerPrefs.SetFloat(KeyMusic, MusicVolume);
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetSFXVolume(float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (Mathf.Approximately(SFXVolume, clamped)) return;

        SFXVolume = clamped;
        PlayerPrefs.SetFloat(KeySFX, SFXVolume);
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetParticlesEnabled(bool value)
    {
        if (ParticlesEnabled == value) return;

        ParticlesEnabled = value;
        PlayerPrefs.SetInt(KeyParticles, value ? 1 : 0);
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void ResetToDefaults()
    {
        MusicVolume = DefaultMusicVolume;
        SFXVolume = DefaultSFXVolume;
        ParticlesEnabled = DefaultParticlesEnabled;

        PlayerPrefs.SetFloat(KeyMusic, MusicVolume);
        PlayerPrefs.SetFloat(KeySFX, SFXVolume);
        PlayerPrefs.SetInt(KeyParticles, ParticlesEnabled ? 1 : 0);
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveSettings();
        }
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SaveSettings();
            Instance = null;
        }
    }
}
