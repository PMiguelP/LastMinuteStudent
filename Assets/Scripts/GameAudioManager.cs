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
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    private AudioSource _source;

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

    public void Play(SoundEvent evt)
    {
        AudioClip clip = evt switch
        {
            SoundEvent.CoinCollect    => coinCollectClip,
            SoundEvent.ObstacleHit   => obstacleHitClip,
            SoundEvent.Stumble       => stumbleClip,
            SoundEvent.SpeedBoost    => speedBoostClip,
            SoundEvent.GameOver      => gameOverClip,
            SoundEvent.CatchSequence => catchSequenceClip,
            SoundEvent.UIClick       => uiClickClip,
            SoundEvent.StartGame     => startGameClip,
            SoundEvent.Jump          => jumpClip,
            SoundEvent.Footstep      => footstepClip,
            _                        => null,
        };

        if (clip != null)
        {
            float vol = SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : sfxVolume;
            _source.PlayOneShot(clip, vol);
        }
    }
}
