using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRunnerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private Animator playerAnimator;

    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 16f;
    [SerializeField] private float laneSpacing = 2.5f;
    [SerializeField] private float laneChangeSpeed = 28f;
    [SerializeField] private float jumpVelocity = 12f;
    [SerializeField] private float gravity = 26f;

    [Header("Bounds")]
    [SerializeField] private float groundY = 0f;
    [SerializeField] private float failY = -3f;
    [SerializeField] private string obstacleTag = "Obstacle";

    [Header("Animation")]
    [SerializeField] private string runParameterName = "IsRunning";
    [SerializeField] private string jumpTriggerName = "Jump";
    [SerializeField] private string stumbleTriggerName = "Stumble";

    [Header("Speed Ramp")]
    [SerializeField] private float maxForwardSpeed = 28f;
    [SerializeField] private float speedRampRate = 1.5f;

    [Header("Stumble")]
    [SerializeField] private float stumbleDuration = 1.0f;
    [SerializeField] private float stumbleKnockbackSpeed = 5f;
    [SerializeField] private float vulnerabilityWindow = 3.0f;
    [SerializeField] private float stumbleAnimatorSpeed = 2f;

    private Vector3 startPosition;
    private float verticalVelocity;
    private int currentLane;
    private bool wasGroundedLastFrame;

    private float _activeSpeedMultiplier = 1f;
    private float _boostTimer;
    private bool _hasDoubleJump;
    private bool _usedDoubleJump;

    private float _rampedSpeed;

    private bool _isStumbling;
    private float _stumbleTimer;
    private bool _isVulnerable;
    private float _vulnerableTimer;

    private float _footstepTimer;

    private ChaserFollower _chaser;

    private void Awake()
    {
        startPosition = transform.position;

        if (menuController == null)
        {
            menuController = FindFirstObjectByType<MainMenuController>();
        }

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        _chaser = FindFirstObjectByType<ChaserFollower>();
    }

    private void OnEnable()
    {
        ResetRunner();
    }

    private void Update()
    {
        if (menuController != null && !menuController.IsGameRunning())
        {
            UpdateAnimatorState(false);
            return;
        }

        if (!_isStumbling)
            _rampedSpeed = Mathf.Min(maxForwardSpeed, _rampedSpeed + speedRampRate * Time.deltaTime);

        HandleLaneInput();
        TickBoost();
        TickStumble();
        TickFootsteps();
        UpdateMovement();
        UpdateAnimatorState(!_isStumbling);

        if (transform.position.y < failY)
        {
            TriggerGameOver();
        }
    }

    private void HandleLaneInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            currentLane = Mathf.Max(currentLane - 1, -1);
        }

        if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            currentLane = Mathf.Min(currentLane + 1, 1);
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            float effectiveJump = Mathf.Max(jumpVelocity, 12f);
            if (IsGrounded())
            {
                verticalVelocity = effectiveJump;
                GameAudioManager.Instance?.Play(SoundEvent.Jump);
                TriggerAnimator(jumpTriggerName);
            }
            else if (_hasDoubleJump && !_usedDoubleJump)
            {
                verticalVelocity = effectiveJump;
                _usedDoubleJump = true;
                _hasDoubleJump = false;
                GameAudioManager.Instance?.Play(SoundEvent.Jump);
                TriggerAnimator(jumpTriggerName);
            }
        }
    }

    private void UpdateMovement()
    {
        Vector3 position = transform.position;
        float targetX = startPosition.x + currentLane * laneSpacing;

        position.x = Mathf.MoveTowards(position.x, targetX, laneChangeSpeed * Time.deltaTime);
        float zSpeed = _isStumbling
            ? -stumbleKnockbackSpeed
            : _rampedSpeed * _activeSpeedMultiplier;
        position.z += zSpeed * Time.deltaTime;

        float fallScale = verticalVelocity < 0f ? 1.8f : 1f;
        verticalVelocity -= gravity * fallScale * Time.deltaTime;
        position.y += verticalVelocity * Time.deltaTime;

        if (position.y <= groundY)
        {
            position.y = groundY;

            if (verticalVelocity < 0f)
            {
                verticalVelocity = 0f;
            }
        }

        transform.position = position;
    }

    private bool IsGrounded()
    {
        return transform.position.y <= groundY + 0.05f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        RunnerHazard hazard = collision.collider.GetComponentInParent<RunnerHazard>();
        if (hazard != null)
        {
            hazard.Hit(this);
            return;
        }

        if (collision.collider.CompareTag(obstacleTag))
        {
            HandleHazardHit(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        RunnerPickup pickup = other.GetComponentInParent<RunnerPickup>();
        if (pickup != null)
        {
            pickup.Collect(this);
            return;
        }

        RunnerHazard hazard = other.GetComponentInParent<RunnerHazard>();
        if (hazard != null)
        {
            hazard.Hit(this);
            return;
        }

        if (other.CompareTag(obstacleTag))
        {
            HandleHazardHit(true);
        }
    }

    private void TriggerGameOver()
    {
        GameAudioManager.Instance?.Play(SoundEvent.GameOver);
        GameParticleManager.Instance?.PlayAt(ParticleEvent.GameOver, transform.position);

        // Let the chaser teleport in and play the catch/dance sequence.
        // CatchSequence will call ShowGameOver() at the end.
        if (_chaser != null)
        {
            _chaser.TriggerCatchSequenceFromGameOver();
        }
        else if (menuController != null)
        {
            menuController.ShowGameOver();
        }
    }

    public void HandleHazardHit(bool causesStumble = true)
    {
        if (_isStumbling || _isVulnerable)
        {
            TriggerGameOver();
            return;
        }

        if (causesStumble)
        {
            _isStumbling = true;
            _stumbleTimer = stumbleDuration;
            _isVulnerable = true;
            _vulnerableTimer = vulnerabilityWindow;
            if (playerAnimator != null) playerAnimator.speed = stumbleAnimatorSpeed;
            TriggerAnimator(stumbleTriggerName);
            _chaser?.ApplyChaserPenalty();
            GameAudioManager.Instance?.Play(SoundEvent.Stumble);
            GameParticleManager.Instance?.PlayAt(ParticleEvent.ObstacleHit, transform.position);
        }
        else
        {
            TriggerGameOver();
        }
    }

    private void TickStumble()
    {
        if (_isStumbling)
        {
            _stumbleTimer -= Time.deltaTime;

            if (_stumbleTimer <= 0f)
            {
                _isStumbling = false;
                if (playerAnimator != null) playerAnimator.speed = 1f;
                ResetAnimatorTriggers();
            }
        }

        if (_isVulnerable)
        {
            _vulnerableTimer -= Time.deltaTime;

            if (_vulnerableTimer <= 0f)
            {
                _isVulnerable = false;
            }
        }
    }

    public void ResetRunnerToStart()
    {
        ResetRunner();
    }

    public void SetRunningAnimation(bool isRunning)
    {
        UpdateAnimatorState(isRunning);
    }

    private void ResetRunner()
    {
        currentLane = 0;
        verticalVelocity = 0f;
        transform.position = startPosition;
        wasGroundedLastFrame = true;
        _rampedSpeed = Mathf.Max(forwardSpeed, 18f);
        _activeSpeedMultiplier = 1f;
        _boostTimer = 0f;
        _hasDoubleJump = false;
        _usedDoubleJump = false;
        _isStumbling = false;
        _stumbleTimer = 0f;
        _isVulnerable = false;
        _vulnerableTimer = 0f;
        if (playerAnimator != null)
        {
            playerAnimator.speed = 1f;
            playerAnimator.Rebind();
            playerAnimator.Update(0f);
        }
        ResetAnimatorTriggers();
        UpdateAnimatorState(false);
    }

    private void TickFootsteps()
    {
        if (IsGrounded() && !_isStumbling)
        {
            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0f)
            {
                GameAudioManager.Instance?.Play(SoundEvent.Footstep);
                _footstepTimer = 0.35f;
            }
        }
        else
        {
            _footstepTimer = 0f;
        }
    }

    private void TickBoost()
    {
        if (_boostTimer <= 0f)
        {
            return;
        }

        _boostTimer -= Time.deltaTime;

        if (_boostTimer <= 0f)
        {
            _boostTimer = 0f;
            _activeSpeedMultiplier = 1f;
        }
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        _activeSpeedMultiplier = Mathf.Max(1f, multiplier);
        _boostTimer = Mathf.Max(0f, duration);
    }

    public void ApplyDoubleJump()
    {
        _hasDoubleJump = true;
        _usedDoubleJump = false;
    }

    private void UpdateAnimatorState(bool isRunning)
    {
        if (playerAnimator == null)
        {
            return;
        }

        playerAnimator.SetBool(runParameterName, isRunning);

        bool isGrounded = IsGrounded();
        if (isGrounded && !wasGroundedLastFrame)
        {
            playerAnimator.ResetTrigger(jumpTriggerName);
            _usedDoubleJump = false;
        }

        wasGroundedLastFrame = isGrounded;
    }

    private void TriggerAnimator(string triggerName)
    {
        if (playerAnimator == null || string.IsNullOrWhiteSpace(triggerName))
        {
            return;
        }

        playerAnimator.SetTrigger(triggerName);
    }

    private void ResetAnimatorTriggers()
    {
        if (playerAnimator == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(jumpTriggerName))
        {
            playerAnimator.ResetTrigger(jumpTriggerName);
        }

        if (!string.IsNullOrWhiteSpace(stumbleTriggerName))
        {
            playerAnimator.ResetTrigger(stumbleTriggerName);
        }
    }

    public MainMenuController GetMenuController()
    {
        return menuController;
    }
}
