using UnityEngine;

public class RunnerSpeedBoost : RunnerPickup
{
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float duration = 5f;

    public override void Collect(PlayerRunnerController player)
    {
        if (player == null)
        {
            return;
        }

        player.ApplySpeedBoost(speedMultiplier, duration);
        GameAudioManager.Instance?.Play(SoundEvent.SpeedBoost);
        GameParticleManager.Instance?.PlayAt(ParticleEvent.SpeedBoost, transform.position);
        gameObject.SetActive(false);
    }
}
