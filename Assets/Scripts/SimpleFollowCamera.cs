using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smooth = 14f;

    [Header("Camera Presets (V key cycles)")]
    [SerializeField] private Vector3[] presetOffsets = new Vector3[]
    {
        new Vector3(0f,  6f,  -12f),  // 0 — Standard behind (default)
        new Vector3(-6f, 5f,   -8f),  // 1 — Side view (stays behind, avoids buildings)
        new Vector3(0f,  2.5f, -5f),  // 2 — Low cinematic
    };
    [SerializeField] private float[] presetLookAheadZ = new float[] { 10f, 8f, 8f };

    [Header("Wall Avoidance")]
    [SerializeField] private float wallAvoidRadius = 0.3f;
    [SerializeField] private LayerMask wallAvoidMask = ~0;  // all layers by default

    private int _presetIndex = 0;
    private float _nextSearchTime;

    // Cutscene state
    private bool _cutsceneActive;
    private Transform _cutsceneChaser;

    // ─── Public API ───────────────────────────────────────────────────────────

    public void StartCutscene(Transform chaserTransform)
    {
        _cutsceneActive = true;
        _cutsceneChaser = chaserTransform;
    }

    public void EndCutscene()
    {
        _cutsceneActive = false;
        _cutsceneChaser = null;
    }

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

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
        if (target == null && Time.unscaledTime >= _nextSearchTime)
        {
            _nextSearchTime = Time.unscaledTime + 0.5f;
            TryResolveTarget();
        }

        if (target == null) return;

        // V key cycles camera preset (only when not in cutscene)
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.vKey.wasPressedThisFrame && !_cutsceneActive)
        {
            _presetIndex = (_presetIndex + 1) % presetOffsets.Length;
        }

        if (_cutsceneActive && _cutsceneChaser != null)
        {
            UpdateCutscene();
        }
        else
        {
            UpdateFollowCamera();
        }
    }

    // ─── Camera modes ─────────────────────────────────────────────────────────

    private void UpdateFollowCamera()
    {
        Vector3 offset     = presetOffsets[_presetIndex];
        float   lookAheadZ = presetLookAheadZ[_presetIndex];

        Vector3 desiredPos = target.position + offset;
        // Wall avoidance only on the side-view preset — standard/cinematic offsets
        // are behind the player and don't clip through buildings.
        Vector3 safePos = _presetIndex == 1 ? ClipToWall(desiredPos) : desiredPos;

        transform.position = Vector3.Lerp(transform.position, safePos, smooth * Time.deltaTime);

        Vector3 lookTarget = target.position + new Vector3(0f, 1.5f, lookAheadZ);
        Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smooth * Time.deltaTime);
    }

    /// <summary>
    /// SphereCasts from the target's head toward the desired camera position.
    /// If a wall is in the way, returns a position just in front of it.
    /// </summary>
    private Vector3 ClipToWall(Vector3 desiredPos)
    {
        Vector3 origin    = target.position + Vector3.up * 1.5f;
        Vector3 direction = desiredPos - origin;
        float   distance  = direction.magnitude;

        if (distance < 0.1f) return desiredPos;

        if (Physics.SphereCast(origin, wallAvoidRadius, direction.normalized,
                               out RaycastHit hit, distance, wallAvoidMask,
                               QueryTriggerInteraction.Ignore))
        {
            // Pull camera to just before the hit point
            return origin + direction.normalized * Mathf.Max(0f, hit.distance - wallAvoidRadius - 0.1f);
        }

        return desiredPos;
    }

    private void UpdateCutscene()
    {
        Vector3 midpoint = (target.position + _cutsceneChaser.position) * 0.5f;
        Vector3 desiredPos = midpoint + new Vector3(0f, 5f, -7f);
        transform.position = Vector3.Lerp(transform.position, desiredPos, smooth * Time.deltaTime);

        Vector3 lookAt = midpoint + new Vector3(0f, 1f, 0f);
        Quaternion desiredRotation = Quaternion.LookRotation(lookAt - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smooth * Time.deltaTime);
    }

    // ─── Target resolution ────────────────────────────────────────────────────

    private void TryResolveTarget()
    {
        if (target != null) return;

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
        if (target == null) return;

        Vector3 offset = presetOffsets[_presetIndex];
        float lookAheadZ = presetLookAheadZ[_presetIndex];

        transform.position = target.position + offset;

        Vector3 lookTarget = target.position + new Vector3(0f, 1.5f, lookAheadZ);
        Vector3 direction = lookTarget - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
