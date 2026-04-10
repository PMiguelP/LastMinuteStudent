using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip musicClip;

    private AudioSource _source;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _source = gameObject.AddComponent<AudioSource>();
        _source.clip        = musicClip;
        _source.loop        = true;
        _source.playOnAwake = false;

        ApplyVolume();

        if (musicClip != null)
            _source.Play();
    }

    private void OnEnable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged += ApplyVolume;
    }

    private void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged -= ApplyVolume;
    }

    private void ApplyVolume()
    {
        if (_source == null) return;
        _source.volume = SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 0.6f;
    }
}
