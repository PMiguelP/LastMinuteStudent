using UnityEngine;

[CreateAssetMenu(fileName = "SpawnItem", menuName = "Runner/Spawn Item Definition")]
public class SpawnItemDefinition : ScriptableObject
{
    [SerializeField] private string itemId = "item";
    [SerializeField] private RunnerItemType itemType = RunnerItemType.Coin;
    [SerializeField] private GameObject prefab;
    [SerializeField] private int weight = 10;
    [SerializeField] private RunnerLane allowedLanes = RunnerLane.All;
    [SerializeField] private float minScore = 0f;
    [SerializeField] private float maxScore = 999999f;
    [SerializeField] private int coinValue = 1;

    [Header("Spawn Fit")]
    [SerializeField] private Vector3 localPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;
    [SerializeField] private Vector3 scaleMultiplier = Vector3.one;
    [SerializeField] private bool useSocketRotation = true;

    [Header("Gem Animation (Optional)")]
    [SerializeField] private bool configureSimpleGemsAnim;
    [SerializeField] private bool gemsRotate = true;
    [SerializeField] private bool gemsRotateY = true;
    [SerializeField] private float gemsRotationSpeed = 90f;
    [SerializeField] private bool gemsFloat = true;
    [SerializeField] private float gemsFloatHeight = 0.3f;
    [SerializeField] private float gemsFloatSpeed = 1.5f;

    public string ItemId => itemId;
    public RunnerItemType ItemType => itemType;
    public GameObject Prefab => prefab;
    public int Weight => Mathf.Max(0, weight);
    public RunnerLane AllowedLanes => allowedLanes;
    public float MinScore => minScore;
    public float MaxScore => maxScore;
    public int CoinValue => Mathf.Max(1, coinValue);
    public Vector3 LocalPositionOffset => localPositionOffset;
    public Vector3 RotationOffsetEuler => rotationOffsetEuler;
    public Vector3 ScaleMultiplier => new Vector3(
        Mathf.Max(0.001f, scaleMultiplier.x),
        Mathf.Max(0.001f, scaleMultiplier.y),
        Mathf.Max(0.001f, scaleMultiplier.z)
    );
    public bool UseSocketRotation => useSocketRotation;
    public bool ConfigureSimpleGemsAnim => configureSimpleGemsAnim;
    public bool GemsRotate => gemsRotate;
    public bool GemsRotateY => gemsRotateY;
    public float GemsRotationSpeed => gemsRotationSpeed;
    public bool GemsFloat => gemsFloat;
    public float GemsFloatHeight => gemsFloatHeight;
    public float GemsFloatSpeed => gemsFloatSpeed;

    public bool IsEligible(float score, RunnerLane lane)
    {
        if (prefab == null)
        {
            return false;
        }

        if (score < minScore || score > maxScore)
        {
            return false;
        }

        return (allowedLanes & lane) != 0;
    }
}