using UnityEngine;

public class RunnerChunkRecycler : MonoBehaviour
{
    [SerializeField] private ChunkSpawnController[] chunks;
    [SerializeField] private float chunkLength = 30f;
    [SerializeField] private float recycleBuffer = 15f;

    [Header("References")]
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private Transform player;

    private float[] _initialZOffsets;

    private void Awake()
    {
        if (menuController == null)
        {
            menuController = FindFirstObjectByType<MainMenuController>();
        }

        if (player == null)
        {
            PlayerRunnerController runner = FindFirstObjectByType<PlayerRunnerController>();

            if (runner != null)
            {
                player = runner.transform;
            }
        }

        CacheInitialOffsets();
    }

    private void CacheInitialOffsets()
    {
        if (chunks == null)
        {
            return;
        }

        _initialZOffsets = new float[chunks.Length];

        for (int i = 0; i < chunks.Length; i++)
        {
            if (chunks[i] != null)
            {
                _initialZOffsets[i] = chunks[i].transform.position.z;
            }
        }
    }

    private void Update()
    {
        if (menuController != null && !menuController.IsGameRunning())
        {
            return;
        }

        if (player == null || chunks == null)
        {
            return;
        }

        float playerZ = player.position.z;

        for (int i = 0; i < chunks.Length; i++)
        {
            ChunkSpawnController chunk = chunks[i];

            if (chunk == null)
            {
                continue;
            }

            float chunkZ = chunk.transform.position.z;

            float thisChunkLength = chunk.ChunkLength > 0f ? chunk.ChunkLength : chunkLength;
            if (chunkZ + thisChunkLength + recycleBuffer < playerZ)
            {
                float frontZ = GetFrontmostChunkZ();
                Vector3 pos = chunk.transform.position;
                pos.z = frontZ + thisChunkLength;
                chunk.transform.position = pos;
                chunk.SpawnIntoChunk();
                break;
            }
        }
    }

    private float GetFrontmostChunkZ()
    {
        float maxZ = float.MinValue;

        foreach (ChunkSpawnController chunk in chunks)
        {
            if (chunk != null && chunk.transform.position.z > maxZ)
            {
                maxZ = chunk.transform.position.z;
            }
        }

        return maxZ;
    }

    public void ResetChunks()
    {
        if (chunks == null)
        {
            return;
        }

        for (int i = 0; i < chunks.Length; i++)
        {
            ChunkSpawnController chunk = chunks[i];

            if (chunk == null)
            {
                continue;
            }

            Vector3 pos = chunk.transform.position;

            if (_initialZOffsets != null && i < _initialZOffsets.Length)
            {
                pos.z = _initialZOffsets[i];
            }

            chunk.transform.position = pos;
            chunk.SpawnIntoChunk();
        }
    }

    public void SetChunks(ChunkSpawnController[] newChunks)
    {
        chunks = newChunks;
        CacheInitialOffsets();
    }

    public void SetChunkLength(float length)
    {
        chunkLength = length;
    }
}
