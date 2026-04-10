using System.Collections.Generic;
using Benjathemaker;
using UnityEngine;

public class ChunkSpawnController : MonoBehaviour
{
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private SpawnTable spawnTable;
    [SerializeField] private SpawnSocket[] sockets;
    [SerializeField] private float spawnChance = 1.0f;
    [SerializeField] private bool guaranteeObstacleSpawn = true;

    [Header("Fairness")]
    [SerializeField] private int maxObstaclesPerRow = 1;
    [SerializeField] private float gracePeriodScore = 0f;

    [Header("Coin Patterns")]
    [SerializeField] private float laneSpacing = 2.5f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float spawnChanceAtMaxDifficulty = 1.0f;
    [SerializeField] private float maxDifficultyScore = 500f;

    [Header("Chunk")]
    [SerializeField] private string environmentName = "Environment 0";
    [SerializeField] private float chunkLength = 30f;

    public float ChunkLength => chunkLength;

    private readonly List<(GameObject instance, GameObject prefab)> spawnedInstances =
        new List<(GameObject instance, GameObject prefab)>();



    private readonly struct CoinPoint
    {
        public readonly float relZ;
        public readonly int laneOffset;
        public readonly float heightY;
        public CoinPoint(float z, int lane, float y = 1f)
        { relZ = z; laneOffset = lane; heightY = y; }
    }

    private static readonly CoinPoint[][] CoinPatterns = new CoinPoint[][]
    {

        new[] { new CoinPoint(0,0), new CoinPoint(2,0), new CoinPoint(4,0),
                new CoinPoint(6,0), new CoinPoint(8,0) },


        new[] { new CoinPoint(0,0),  new CoinPoint(2,0),  new CoinPoint(4,0),
                new CoinPoint(6,0),  new CoinPoint(8,0),  new CoinPoint(10,0),
                new CoinPoint(12,0), new CoinPoint(14,0) },


        new[] { new CoinPoint(0,0,1f),   new CoinPoint(2,0,1.7f), new CoinPoint(4,0,2.4f),
                new CoinPoint(6,0,2.4f), new CoinPoint(8,0,1.7f), new CoinPoint(10,0,1f) },


        new[] { new CoinPoint(0,-1),  new CoinPoint(3,0),  new CoinPoint(6,1),
                new CoinPoint(9,0),   new CoinPoint(12,-1), new CoinPoint(15,0) },


        new[] { new CoinPoint(0,-1), new CoinPoint(3,-1), new CoinPoint(6,0),
                new CoinPoint(9,0),  new CoinPoint(12,1), new CoinPoint(15,1) },


        new[] { new CoinPoint(0,1),  new CoinPoint(3,1),  new CoinPoint(6,0),
                new CoinPoint(9,0),  new CoinPoint(12,-1), new CoinPoint(15,-1) },


        new[] { new CoinPoint(0,-1), new CoinPoint(2,-1), new CoinPoint(4,-1),
                new CoinPoint(0,1),  new CoinPoint(2,1),  new CoinPoint(4,1) },


        new[] { new CoinPoint(0,-1,1f),  new CoinPoint(2,-1,1.4f),
                new CoinPoint(4,0,1.8f), new CoinPoint(6,0,2.2f),
                new CoinPoint(8,1,2.2f), new CoinPoint(10,1,1f) },
    };

    private void Awake()
    {
        sockets = GetComponentsInChildren<SpawnSocket>(true);

        if (menuController == null)
        {
            menuController = FindFirstObjectByType<MainMenuController>();
        }
    }

    private void Start()
    {
        SpawnIntoChunk();
    }

    public void SpawnIntoChunk()
    {
        ClearChunk();

        if (spawnTable == null || sockets == null || sockets.Length == 0)
        {
            return;
        }

        float score = menuController != null ? menuController.GetScore() : 0f;
        bool inGracePeriod = score < gracePeriodScore;
        float baseChance = inGracePeriod ? 0.4f : spawnChance;
        float t = maxDifficultyScore > 0f ? Mathf.Clamp01(score / maxDifficultyScore) : 1f;
        float effectiveChance = Mathf.Lerp(baseChance, spawnChanceAtMaxDifficulty, t);


        Dictionary<int, List<SpawnSocket>> rows = new Dictionary<int, List<SpawnSocket>>();
        foreach (SpawnSocket socket in sockets)
        {
            if (socket == null)
            {
                continue;
            }

            int rowKey = Mathf.RoundToInt(socket.transform.localPosition.z);
            if (!rows.TryGetValue(rowKey, out List<SpawnSocket> rowList))
            {
                rowList = new List<SpawnSocket>();
                rows[rowKey] = rowList;
            }
            rowList.Add(socket);
        }

        bool spawnedObstacle = false;

        foreach (List<SpawnSocket> rowSockets in rows.Values)
        {

            List<(SpawnSocket socket, SpawnItemDefinition definition)> candidates =
                new List<(SpawnSocket, SpawnItemDefinition)>();

            foreach (SpawnSocket socket in rowSockets)
            {
                if (Random.value > effectiveChance)
                {
                    continue;
                }

                SpawnItemDefinition definition = spawnTable.PickItem(score, socket.Lane);
                if (definition == null)
                {
                    continue;
                }


                if (definition.ItemType == RunnerItemType.Coin)
                {
                    continue;
                }


                if (inGracePeriod && definition.ItemType == RunnerItemType.Obstacle)
                {
                    continue;
                }

                candidates.Add((socket, definition));
            }


            int obstacleCount = 0;
            foreach ((SpawnSocket s, SpawnItemDefinition d) in candidates)
            {
                if (d.ItemType == RunnerItemType.Obstacle)
                {
                    obstacleCount++;
                }
            }

            while (obstacleCount > maxObstaclesPerRow)
            {
                List<int> obstacleIndices = new List<int>();
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i].definition.ItemType == RunnerItemType.Obstacle)
                    {
                        obstacleIndices.Add(i);
                    }
                }

                candidates.RemoveAt(obstacleIndices[Random.Range(0, obstacleIndices.Count)]);
                obstacleCount--;
            }


            foreach ((SpawnSocket socket, SpawnItemDefinition definition) in candidates)
            {
                if (definition.ItemType == RunnerItemType.Obstacle)
                {
                    spawnedObstacle = true;
                }

                SpawnFromDefinition(socket, definition);
            }
        }


        if (guaranteeObstacleSpawn && !spawnedObstacle && !inGracePeriod)
        {
            TrySpawnGuaranteedObstacle(score);
        }


        SpawnCoinPatterns(score);
    }

    public void ClearChunk()
    {
        RunnerObjectPool pool = RunnerObjectPool.Instance;

        foreach ((GameObject instance, GameObject prefab) entry in spawnedInstances)
        {
            if (entry.instance != null)
            {
                pool.Release(entry.instance, entry.prefab);
            }
        }

        spawnedInstances.Clear();
    }

    private void TrySpawnGuaranteedObstacle(float score)
    {
        if (sockets == null || sockets.Length == 0 || spawnTable == null)
        {
            return;
        }

        int startIndex = Random.Range(0, sockets.Length);

        for (int i = 0; i < sockets.Length; i++)
        {
            SpawnSocket socket = sockets[(startIndex + i) % sockets.Length];
            if (socket == null)
            {
                continue;
            }

            SpawnItemDefinition obstacleDefinition = spawnTable.PickItemByType(score, socket.Lane, RunnerItemType.Obstacle);
            if (obstacleDefinition == null)
            {
                continue;
            }

            SpawnFromDefinition(socket, obstacleDefinition);
            return;
        }
    }

    private void SpawnCoinAt(SpawnItemDefinition def, Vector3 worldPosition)
    {
        Quaternion spawnRotation = Quaternion.Euler(def.RotationOffsetEuler);
        Vector3 spawnPosition = worldPosition + def.LocalPositionOffset;

        GameObject instance = RunnerObjectPool.Instance.Get(def.Prefab, transform, spawnPosition, spawnRotation);
        Vector3 desiredWorld = Vector3.Scale(def.Prefab.transform.lossyScale, def.ScaleMultiplier);
        Vector3 parentWorld = transform.lossyScale;
        instance.transform.localScale = new Vector3(
            parentWorld.x > 0.001f ? desiredWorld.x / parentWorld.x : desiredWorld.x,
            parentWorld.y > 0.001f ? desiredWorld.y / parentWorld.y : desiredWorld.y,
            parentWorld.z > 0.001f ? desiredWorld.z / parentWorld.z : desiredWorld.z);
        spawnedInstances.Add((instance, def.Prefab));

        // Ensure coin has a trigger collider so OnTriggerEnter fires on the player
        if (instance.GetComponentInChildren<Collider>() == null)
        {
            SphereCollider sc = instance.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.5f;
            sc.center = new Vector3(0f, 0.5f, 0f);
        }
        else
        {
            foreach (Collider c in instance.GetComponentsInChildren<Collider>())
                c.isTrigger = true;
        }

        RunnerCoin coin = instance.GetComponentInChildren<RunnerCoin>();
        if (coin != null) coin.SetValue(def.CoinValue);
    }

    private void SpawnCoinPatterns(float score)
    {
        SpawnItemDefinition coinDef = spawnTable.PickItemByType(score, RunnerLane.All, RunnerItemType.Coin);
        if (coinDef == null || coinDef.Prefab == null) return;

        int patternCount = 2;
        float halfChunk = chunkLength * 0.5f;

        for (int p = 0; p < patternCount; p++)
        {
            CoinPoint[] pattern = CoinPatterns[Random.Range(0, CoinPatterns.Length)];
            int centerLane = Random.Range(-1, 2);

            float maxRelZ = 0f;
            foreach (CoinPoint cp in pattern) maxRelZ = Mathf.Max(maxRelZ, cp.relZ);

            float zMin = p == 0 ? 2f : halfChunk;
            float zMax = p == 0 ? halfChunk - maxRelZ : chunkLength - maxRelZ - 2f;
            if (zMin >= zMax) continue;
            float zStart = Random.Range(zMin, zMax);

            foreach (CoinPoint cp in pattern)
            {
                int actualLane = Mathf.Clamp(centerLane + cp.laneOffset, -1, 1);
                Vector3 worldPos = new Vector3(
                    transform.position.x + actualLane * laneSpacing,
                    transform.position.y + cp.heightY,
                    transform.position.z + zStart + cp.relZ);
                SpawnCoinAt(coinDef, worldPos);
            }
        }
    }

    private void SpawnFromDefinition(SpawnSocket socket, SpawnItemDefinition definition)
    {
        if (definition.Prefab == null)
        {
            Debug.LogWarning($" Prefab is null for definition '{definition.ItemId}'. Skipping spawn.");
            return;
        }

        Quaternion baseRotation = definition.UseSocketRotation ? socket.transform.rotation : Quaternion.identity;
        Quaternion spawnRotation = baseRotation * Quaternion.Euler(definition.RotationOffsetEuler);
        Vector3 spawnPosition = socket.transform.position + socket.transform.TransformVector(definition.LocalPositionOffset);

        GameObject instance = RunnerObjectPool.Instance.Get(definition.Prefab, transform, spawnPosition, spawnRotation);
        Vector3 desiredWorld = Vector3.Scale(definition.Prefab.transform.lossyScale, definition.ScaleMultiplier);
        Vector3 parentWorld = transform.lossyScale;
        instance.transform.localScale = new Vector3(
            parentWorld.x > 0.001f ? desiredWorld.x / parentWorld.x : desiredWorld.x,
            parentWorld.y > 0.001f ? desiredWorld.y / parentWorld.y : desiredWorld.y,
            parentWorld.z > 0.001f ? desiredWorld.z / parentWorld.z : desiredWorld.z
        );
        spawnedInstances.Add((instance, definition.Prefab));

        if (definition.ItemType == RunnerItemType.Obstacle)
        {

            if (instance.GetComponentInChildren<RunnerHazard>() == null)
                instance.AddComponent<RunnerObstacle>();




            if (instance.GetComponentInChildren<Collider>() == null)
            {
                BoxCollider bc = instance.AddComponent<BoxCollider>();
                bc.isTrigger = true;

                MeshFilter mf = instance.GetComponentInChildren<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    bc.center = mf.sharedMesh.bounds.center;
                    bc.size = mf.sharedMesh.bounds.size;
                }
                else
                {
                    bc.center = new Vector3(0f, 0.75f, 0f);
                    bc.size = new Vector3(0.8f, 1.5f, 0.8f);
                }
            }
            else
            {

                bool hasTrigger = false;
                foreach (Collider c in instance.GetComponentsInChildren<Collider>())
                    if (c.isTrigger) { hasTrigger = true; break; }
                if (!hasTrigger)
                    instance.GetComponentInChildren<Collider>().isTrigger = true;
            }





            if (instance.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = instance.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        RunnerCoin coin = instance.GetComponentInChildren<RunnerCoin>();
        if (coin != null)
        {
            coin.SetValue(definition.CoinValue);
        }

        if (!definition.ConfigureSimpleGemsAnim)
        {
            return;
        }

        SimpleGemsAnim gemsAnim = instance.GetComponentInChildren<SimpleGemsAnim>(true);
        if (gemsAnim != null)
        {
            gemsAnim.isRotating = definition.GemsRotate;
            gemsAnim.rotateX = false;
            gemsAnim.rotateY = definition.GemsRotateY;
            gemsAnim.rotateZ = false;
            gemsAnim.rotationSpeed = definition.GemsRotationSpeed;
            gemsAnim.isFloating = definition.GemsFloat;
            gemsAnim.floatHeight = definition.GemsFloatHeight;
            gemsAnim.floatSpeed = definition.GemsFloatSpeed;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        const float chunkHeight = 4f;

        Vector3 origin = transform.position;

        
        float startZ = 0f;
        float endZ = chunkLength;
        Transform entryChild = transform.Find("EntryPoint");
        Transform exitChild = transform.Find("ExitPoint");
        if (entryChild != null) startZ = entryChild.localPosition.z;
        if (exitChild != null) endZ = exitChild.localPosition.z;
        float realLength = endZ - startZ;

        
        float leftX = -2.5f;
        float midX = 0f;
        float rightX = 2.5f;
        Transform leftGuide = transform.Find("LaneLeftGuide");
        Transform midGuide = transform.Find("LaneMidGuide");
        Transform rightGuide = transform.Find("LaneRightGuide");
        if (leftGuide != null) leftX = leftGuide.localPosition.x;
        if (midGuide != null) midX = midGuide.localPosition.x;
        if (rightGuide != null) rightX = rightGuide.localPosition.x;

        
        float boxWidth = Mathf.Abs(rightX - leftX) + 1f;
        Vector3 boxCenter = origin + new Vector3(midX, chunkHeight * 0.5f, startZ + realLength * 0.5f);
        Vector3 boxSize = new Vector3(boxWidth, chunkHeight, realLength);
        UnityEditor.Handles.color = new Color(1f, 0.9f, 0f, 0.25f);
        UnityEditor.Handles.DrawWireCube(boxCenter, boxSize);

        
        float[] laneXPositions = { leftX, midX, rightX };
        Color[] laneColors = { Color.red, Color.green, Color.blue };
        string[] laneLabels = { "LEFT", "MID", "RIGHT" };

        for (int i = 0; i < laneXPositions.Length; i++)
        {
            UnityEditor.Handles.color = laneColors[i];
            Vector3 start = origin + new Vector3(laneXPositions[i], 0.1f, startZ);
            Vector3 end = origin + new Vector3(laneXPositions[i], 0.1f, endZ);
            UnityEditor.Handles.DrawLine(start, end);

            UnityEditor.Handles.Label(
                origin + new Vector3(laneXPositions[i], 0.5f, startZ + realLength * 0.5f),
                laneLabels[i]);
        }

        
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            origin + new Vector3(midX, chunkHeight + 0.5f, startZ + realLength * 0.5f),
            string.IsNullOrEmpty(environmentName) ? gameObject.name : environmentName);
    }
#endif
}
