using System.Collections;
using UnityEngine;

public class ChaserFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private Transform target;
    [SerializeField] private Animator chaserAnimator;

    [Header("Follow")]
    [SerializeField] private float forwardGap = 15f;
    [SerializeField] private float approachSpeed = 4f;   // speed to close gap after stumble
    [SerializeField] private float laneFollowSpeed = 8f;
    [SerializeField] private float catchDistance = 1.35f;
    [SerializeField] private float lookAhead = 4f;

    [Header("Dynamic Gap")]
    [SerializeField] private float maxGap = 15f;
    [SerializeField] private float hitPenalty = 7f;     // stumble 1: 15→8 (visible), stumble 2: 8→1 (catch)
    [SerializeField] private float minGap = 0.8f;

    [Header("Catch Sequence")]
    [SerializeField] private string catchTriggerName = "End";
    [SerializeField] private float catchAnimationDelay = 2.0f;

    private Vector3 startPosition;
    private float _defaultForwardGap;
    private bool _isCatchingPlayer;
    private float _prevTargetZ;

    private void Awake()
    {
        startPosition = transform.position;
        _defaultForwardGap = forwardGap;

        if (menuController == null)
        {
            menuController = FindFirstObjectByType<MainMenuController>();
        }

        if (target == null)
        {
            PlayerRunnerController playerRunner = FindFirstObjectByType<PlayerRunnerController>();

            if (playerRunner != null)
            {
                target = playerRunner.transform;
            }
        }

        if (chaserAnimator == null)
        {
            chaserAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (menuController != null && !menuController.IsGameRunning())
        {
            chaserAnimator?.SetBool("IsRunning", false);
            return;
        }

        if (target == null || _isCatchingPlayer)
        {
            return;
        }

        chaserAnimator?.SetBool("IsRunning", true);

        Vector3 position = transform.position;
        Vector3 targetPosition = target.position;

        // Mirror the player's Z movement exactly so the gap stays constant.
        float playerDeltaZ = targetPosition.z - _prevTargetZ;
        _prevTargetZ = targetPosition.z;
        position.z += playerDeltaZ;

        // Slowly close the gap only when a stumble penalty has reduced forwardGap.
        float desiredZ = targetPosition.z - forwardGap;
        float gapError = desiredZ - position.z;
        if (gapError > 0.05f)
            position.z += approachSpeed * Time.deltaTime;
        position.x = Mathf.MoveTowards(position.x, targetPosition.x, laneFollowSpeed * Time.deltaTime);
        position.y = Mathf.MoveTowards(position.y, targetPosition.y, laneFollowSpeed * Time.deltaTime);
        transform.position = position;

        Vector3 lookTarget = targetPosition + new Vector3(0f, 1f, lookAhead);
        Vector3 lookDirection = lookTarget - transform.position;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookDirection), laneFollowSpeed * Time.deltaTime);
        }

        float distance = Vector3.Distance(transform.position, target.position);

        
        float proximityT = Mathf.InverseLerp(minGap, maxGap, forwardGap);
        menuController?.SetChaserProximity(proximityT);

        if (distance <= catchDistance)
        {
            StartCoroutine(CatchSequence());
        }
    }

    private IEnumerator CatchSequence()
    {
        _isCatchingPlayer = true;

        
        menuController?.BeginCatchSequence();

        // Move chaser directly behind the player (they came from -Z)
        Vector3 behindTarget = new Vector3(target.position.x, target.position.y, target.position.z - 1.5f);

        while (Vector3.Distance(transform.position, behindTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, behindTarget, approachSpeed * 8f * Time.deltaTime);
            yield return null;
        }

        // Face the camera so the dance animation is visible
        transform.rotation = Quaternion.LookRotation(Vector3.back);

        GameAudioManager.Instance?.Play(SoundEvent.CatchSequence);
        FindFirstObjectByType<SimpleFollowCamera>()?.StartCutscene(transform);

        
        if (chaserAnimator != null && !string.IsNullOrWhiteSpace(catchTriggerName))
        {
            chaserAnimator.SetTrigger(catchTriggerName);
        }

        yield return new WaitForSeconds(catchAnimationDelay);

        FindFirstObjectByType<SimpleFollowCamera>()?.EndCutscene();
        menuController?.ShowGameOver();
    }

    public void ApplyChaserPenalty()
    {
        forwardGap = Mathf.Max(minGap, forwardGap - hitPenalty);
    }

    // Called when the player dies from any reason other than being physically caught.
    // Teleports the chaser beside the player and plays the catch/dance sequence.
    public void TriggerCatchSequenceFromGameOver()
    {
        if (_isCatchingPlayer || target == null) return;
        transform.position = new Vector3(target.position.x, target.position.y, target.position.z - 1.5f);
        StartCoroutine(CatchSequence());
    }

    public void ResetChaserToStart()
    {
        StopAllCoroutines();
        _isCatchingPlayer = false;
        forwardGap = _defaultForwardGap;

        if (target != null)
        {
            transform.position = new Vector3(target.position.x, target.position.y,
                                             target.position.z - _defaultForwardGap);
            _prevTargetZ = target.position.z;
        }
        else
        {
            transform.position = startPosition;
            _prevTargetZ = startPosition.z + _defaultForwardGap;
        }

        chaserAnimator?.SetBool("IsRunning", false);
    }
}
