using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public event System.Action OnSettingsChanged;

    const string KeyMusic      = "MusicVolume";
    const string KeySFX        = "SFXVolume";
    const string KeyParticles  = "ParticlesEnabled";

    public float MusicVolume      { get; private set; }
    public float SFXVolume        { get; private set; }
    public bool  ParticlesEnabled { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        MusicVolume      = PlayerPrefs.GetFloat(KeyMusic,     0.6f);
        SFXVolume        = PlayerPrefs.GetFloat(KeySFX,       1.0f);
        ParticlesEnabled = PlayerPrefs.GetInt(KeyParticles,   1) == 1;
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeyMusic, MusicVolume);
        OnSettingsChanged?.Invoke();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeySFX, SFXVolume);
        OnSettingsChanged?.Invoke();
    }

    public void SetParticlesEnabled(bool value)
    {
        ParticlesEnabled = value;
        PlayerPrefs.SetInt(KeyParticles, value ? 1 : 0);
        OnSettingsChanged?.Invoke();
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
