using UnityEngine;

public class SimpleFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -8f);
    [SerializeField] private float smooth = 8f;

    private void Awake()
    {
        TryResolveTarget();
        SnapToTarget();
    }

    private void OnEnable()
    {
        TryResolveTarget();
        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            TryResolveTarget();
        }

        if (target == null)
        {
            return;
        }

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, smooth * Time.deltaTime);

        Vector3 lookTarget = target.position + new Vector3(0f, 1.5f, 8f);
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smooth * Time.deltaTime);
    }

    private void TryResolveTarget()
    {
        if (target != null)
        {
            return;
        }

        PlayerRunnerController playerRunner = FindFirstObjectByType<PlayerRunnerController>();

        if (playerRunner != null)
        {
            target = playerRunner.transform;
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    private void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + offset;

        Vector3 lookTarget = target.position + new Vector3(0f, 1.5f, 8f);
        Vector3 direction = lookTarget - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
