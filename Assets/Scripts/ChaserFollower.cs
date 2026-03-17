using UnityEngine;

public class ChaserFollower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private Transform target;

    [Header("Follow")]
    [SerializeField] private float forwardGap = 6f;
    [SerializeField] private float followSpeed = 7.5f;
    [SerializeField] private float laneFollowSpeed = 8f;
    [SerializeField] private float catchDistance = 1.35f;
    [SerializeField] private float lookAhead = 4f;

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;

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

        if (target == null)
        {
            return;
        }

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

        if (Vector3.Distance(transform.position, target.position) <= catchDistance)
        {
            menuController.ShowGameOver();
        }
    }

    public void ResetChaserToStart()
    {
        transform.position = startPosition;
    }
}