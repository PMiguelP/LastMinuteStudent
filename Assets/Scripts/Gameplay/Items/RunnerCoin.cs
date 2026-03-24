using UnityEngine;

public class RunnerCoin : RunnerPickup
{
    [SerializeField] private int value = 1;

    [Header("Animation")]
    [SerializeField] private float pulseSpeed  = 3f;
    [SerializeField] private float pulseAmount = 0.15f;
    [SerializeField] private float floatHeight = 0.22f;
    [SerializeField] private float floatSpeed  = 1.5f;
    [SerializeField] private float rotateSpeed = 120f;

    private Vector3 _baseScale;
    private Vector3 _baseLocalPosition;
    private bool _scaleCaptured;

    private void OnEnable()
    {
        _baseLocalPosition = transform.localPosition;
        _scaleCaptured     = false;
    }

    private void Update()
    {
        if (!_scaleCaptured)
        {
            _baseScale     = transform.localScale;
            _scaleCaptured = true;
        }

        // Float up and down
        Vector3 pos = _baseLocalPosition;
        pos.y += Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = pos;

        // Spin on Y axis
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);

        // Scale pulse
        float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = _baseScale * s;
    }

    public override void Collect(PlayerRunnerController player)
    {
        if (player == null)
        {
            return;
        }

        MainMenuController menuController = player.GetMenuController();
        if (menuController != null)
        {
            menuController.AddCoins(value);
        }

        GameAudioManager.Instance?.Play(SoundEvent.CoinCollect);
        GameParticleManager.Instance?.PlayAt(ParticleEvent.CoinCollect, transform.position);
        gameObject.SetActive(false);
    }

    public void SetValue(int newValue)
    {
        value = Mathf.Max(1, newValue);
    }
}