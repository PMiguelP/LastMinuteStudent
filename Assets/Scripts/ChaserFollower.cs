using System.Collections;
using UnityEngine;

public class ChaserFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private Transform target;
    [SerializeField] private Animator chaserAnimator;

    [Header("Follow")]
    [SerializeField] private float forwardGap = 6f;
    [SerializeField] private float followSpeed = 7.5f;
    [SerializeField] private float laneFollowSpeed = 8f;
    [SerializeField] private float catchDistance = 1.35f;
    [SerializeField] private float lookAhead = 4f;

    [Header("Dynamic Gap")]
    [SerializeField] private float recoveryRate = 0.8f;
    [SerializeField] private float maxGap = 12f;
    [SerializeField] private float hitPenalty = 5f;
    [SerializeField] private float minGap = 0.8f;

    [Header("Catch Sequence")]
    [SerializeField] private string catchTriggerName = "Catch";
    [SerializeField] private float catchAnimationDelay = 2.0f;

    private Vector3 startPosition;
    private float _defaultForwardGap;
    private bool _isCatchingPlayer;

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
    }

    private void Update()
    {
        if (menuController != null && !menuController.IsGameRunning())
        {
            return;
        }

        if (target == null || _isCatchingPlayer)
        {
            return;
        }

        
        forwardGap = Mathf.Min(maxGap, forwardGap + recoveryRate * Time.deltaTime);

        Vector3 position = transform.position;
        Vector3 targetPosition = target.position;
        float desiredZ = targetPosition.z - forwardGap;

        position.z = Mathf.MoveTowards(position.z, desiredZ, followSpeed * Time.deltaTime);
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

        
        float side = transform.position.x <= target.position.x ? -1.5f : 1.5f;
        Vector3 sideTarget = new Vector3(target.position.x + side, target.position.y, target.position.z);

        while (Vector3.Distance(transform.position, sideTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, sideTarget, followSpeed * 2f * Time.deltaTime);
            yield return null;
        }

        
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

    public void ResetChaserToStart()
    {
        StopAllCoroutines();
        _isCatchingPlayer = false;
        forwardGap = _defaultForwardGap;
        transform.position = startPosition;
    }
}
