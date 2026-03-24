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

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayAt(ParticleEvent evt, Vector3 worldPosition)
    {
        GameObject prefab = evt switch
        {
            ParticleEvent.CoinCollect => coinCollectFX,
            ParticleEvent.ObstacleHit => obstacleHitFX,
            ParticleEvent.SpeedBoost  => speedBoostFX,
            ParticleEvent.GameOver    => gameOverFX,
            _                         => null,
        };

        if (prefab == null) return;

        Destroy(Instantiate(prefab, worldPosition, Quaternion.identity), 3f);
    }
}
