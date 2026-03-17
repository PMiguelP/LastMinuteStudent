using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRunnerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private Animator playerAnimator;

    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private float laneSpacing = 2.5f;
    [SerializeField] private float laneChangeSpeed = 10f;
    [SerializeField] private float jumpVelocity = 8f;
    [SerializeField] private float gravity = 22f;

    [Header("Bounds")]
    [SerializeField] private float groundY = 0f;
    [SerializeField] private float failY = -3f;
    [SerializeField] private string obstacleTag = "Obstacle";

    [Header("Animation")]
    [SerializeField] private string runParameterName = "IsRunning";
    [SerializeField] private string jumpTriggerName = "Jump";
    [SerializeField] private string stumbleTriggerName = "Stumble";

    private Vector3 startPosition;
    private float verticalVelocity;
    private int currentLane;
    private bool wasGroundedLastFrame;

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

        HandleLaneInput();
        UpdateMovement();
        UpdateAnimatorState(true);

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

        if (IsGrounded() && keyboard.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = jumpVelocity;
            TriggerAnimator(jumpTriggerName);
        }
    }

    private void UpdateMovement()
    {
        Vector3 position = transform.position;
        float targetX = startPosition.x + currentLane * laneSpacing;

        position.x = Mathf.MoveTowards(position.x, targetX, laneChangeSpeed * Time.deltaTime);
        position.z += forwardSpeed * Time.deltaTime;

        verticalVelocity -= gravity * Time.deltaTime;
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
        if (collision.collider.CompareTag(obstacleTag))
        {
            TriggerAnimator(stumbleTriggerName);
            TriggerGameOver();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(obstacleTag))
        {
            TriggerAnimator(stumbleTriggerName);
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (menuController != null)
        {
            menuController.ShowGameOver();
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
        ResetAnimatorTriggers();
        UpdateAnimatorState(false);
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
}