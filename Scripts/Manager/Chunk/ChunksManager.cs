using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunksManager : MonoBehaviour
{
    #region Public References
    public float Size => size;

    public PlatformSO[] PlatformsSO => PlatformsInAltitudeBand.GetAllPlatforms(platformsSO);
    public PlatformSO[] HelpersSO => helpersSO;
    public ObstacleSO[] ObstaclesSO => obstaclesSO;
    #endregion

    [SerializeField] private Transform cameraTransform;
    private Vector2 lastUpdatedPosition;
    [SerializeField] private GameObject player;
    private PlayerController playerController;
    protected GameManager gameManager;

    [Header("Chunk")]
    [SerializeField] private Transform chunksParent;
    [SerializeField] private Transform[] chunksPatternTransforms;
    private ChunksPattern currentChunksPattern;
    private int currentChunksPatternIndex;
    private int[] startHeights;
    [SerializeField] [Range(10, 20)] private float size;

    [Space]
    [SerializeField] [Range(20, 80)] private float activationThreshold;
    [SerializeField] [Range(40, 100)] private float deactivationThreshold;
    [SerializeField] [Range(0, 5)] private float updateThreshold;

    private readonly List<Chunk> chunks = new List<Chunk>();
    private int seed;

    [Header("Platform")]
    [SerializeField] private float minHeightForSpawning;
    [Tooltip("The minimum distance in unity unit between two platforms signed as single")]
    [SerializeField] private int helperSpawnThreshold;
    [SerializeField] private PlatformsInAltitudeBand[] platformsSO;
    [SerializeField] private Gradient platformSpawnProbability;

    [Header("Helper")]
    [SerializeField] private PlatformSO[] helpersSO;

    private PlatformInChunk[] platforms;

    [Header("Obstacle")]
    [SerializeField] private ObstacleSO[] obstaclesSO;

    [Header("Coin")]
    [SerializeField] private GameObject coinPrefab;

    void OnEnable()
    {
        playerController = player.GetComponent<PlayerController>();
        gameManager = GetComponent<GameManager>();

        InitPlatformsInChunk();

        startHeights = new int[chunksPatternTransforms.Length];
        for (int i = 0; i < startHeights.Length; i++)
        {
            startHeights[i] = chunksPatternTransforms[i].GetComponent<ChunksPattern>().StartHeight;
        }
        InitChunks();
    }

    void Update()
    {
        if (Vector2.Distance(cameraTransform.position, lastUpdatedPosition) > updateThreshold)
        {
            ActivateCloseChunks();

            DeactivateFarChunks();

            DestroyBelowLimitObjects();

            lastUpdatedPosition = cameraTransform.position;
        }
    }

    /// <summary>
    /// Gets an array with the start heights of the altitude bands.
    /// </summary>
    /// <returns>An array with the start heights of the altitude bands.</returns>
    public int[] GetPlatformsStartHeights()
    {
        int[] ar = new int[platformsSO.Length];
        for (int i = 0; i < ar.Length; i++)
        {
            ar[i] = platformsSO[i].StartHeight;
        }

        return ar;
    }

    /// <summary>
    /// Initializes the chunk pattern.
    /// </summary>
    private void InitChunks()
    {
        currentChunksPattern = chunksPatternTransforms[currentChunksPatternIndex].GetComponent<ChunksPattern>();
        seed = UnityEngine.Random.Range(0, chunksPatternTransforms[currentChunksPatternIndex].childCount);

        if (currentChunksPatternIndex != 0) return;

        ActivateCloseChunks();

        lastUpdatedPosition = cameraTransform.position;
    }

    /// <summary>
    /// Initializes the platforms array.
    /// </summary>
    private void InitPlatformsInChunk()
    {
        int platformsInAltitudeBand = platformSpawnProbability.colorKeys.Length;
        int altitudeBands = platformsSO.Length;
        platforms = new PlatformInChunk[platformsInAltitudeBand * altitudeBands];

        for (int i = 0; i < altitudeBands; i++)
        {
            for (int j = 0; j < platformsInAltitudeBand; j++)
            {
                int index = i * platformsInAltitudeBand + j;
                platforms[index] = new PlatformInChunk()
                {
                    StartHeight = Mathf.RoundToInt(platformsSO[i].StartHeight / gameManager.HeightMultiplier),
                    ScriptableObject = platformsSO[i].Platforms[j]
                };

                if (j == 0)
                {
                    platforms[index].SpawnProbability = platformSpawnProbability.colorKeys[0].time;
                }
                else
                {
                    platforms[index].SpawnProbability = platformSpawnProbability.colorKeys[j].time - platformSpawnProbability.colorKeys[j - 1].time;
                }
            }
        }
    }

    /// <summary>
    /// Activates the chunks close enough to the player.
    /// </summary>
    private void ActivateCloseChunks()
    {
        float x = cameraTransform.position.x % (size * 2);
        float xAbs = Mathf.Abs(x);
        float xChunk = Mathf.Sign(x) * (Mathf.Abs(cameraTransform.position.x) + ((xAbs > size) ? size * 2 - xAbs : -xAbs));
        float y = cameraTransform.position.y % (size * 2);
        float yChunk = cameraTransform.position.y + (size - y);

        Vector3 closestChunk = new Vector3(xChunk, yChunk);

        // Checks if chunks are close enough to the player looping through a grid 5x5 starting from the top left corner
        for (int j = 2; j >= -2; j--)
        {
            for (int i = -2; i <= 2; i++)
            {
                Vector3 chunkPos = closestChunk + new Vector3(i * size * 2, j * size * 2);

                if (chunkPos.y < 0) continue;
                if (Mathf.Abs(chunkPos.x) - size > gameManager.PlatformLimitX) continue;

                if (Vector2.Distance(cameraTransform.position, chunkPos) < activationThreshold)
                {
                    bool isNewChunk = true;
                    foreach (Chunk chunk in chunks)
                    {
                        if (chunk.Position == chunkPos)
                        {
                            isNewChunk = false;
                            chunk.SetActive(true);
                        }
                    }

                    if (!isNewChunk) continue;

                    chunks.Add(CreateChunk(chunkPos));
                }
            }
        }
    }

    /// <summary>
    /// Creates a chunk in the position given.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <returns>A chunk in the position given.</returns>
    private Chunk CreateChunk(Vector3 position)
    {
        if (currentChunksPatternIndex < startHeights.Length - 1)
        {
            if (position.y > startHeights[currentChunksPatternIndex + 1])
            {
                // Changes chunks pattern
                currentChunksPatternIndex++;
                InitChunks();
            }
        }

        int id = GetNewChunkId(position);
        List<Vector3> platformsPositionList = currentChunksPattern.Chunks[id].GetPlatformsPosition();
        List<Vector3> obstaclesPositionList = currentChunksPattern.Chunks[id].GetObstaclesPosition();
        List<Vector3> coinsPositionList = currentChunksPattern.Chunks[id].GetCoinsPosition();
        Chunk chunk = new Chunk(position, platformsPositionList, obstaclesPositionList, coinsPositionList, minHeightForSpawning + gameManager.Ground.position.y);

        chunk.SetPlatforms(platforms, platformSpawnProbability.colorKeys.Length);
        chunk.SetHelpers(helpersSO, chunks, helperSpawnThreshold, gameManager.HeightMultiplier);
        chunk.InitAllPlatforms(platforms, helpersSO);

        chunk.SetObstacles(obstaclesSO, chunks, gameManager.HeightMultiplier);

        chunk.SetCoins(coinPrefab);

        chunk.InstantiateChunk(chunksParent, gameManager.PlatformLimitX);

        return chunk;
    }

    /// <summary>Gets the chunk id based on the seed.</summary>
    /// <param name="chunkPosition">The position of the new chunk.</param>
    /// <returns>The id of the new chunk.</returns>
    private int GetNewChunkId(Vector2 chunkPosition)
    {
        int id = 0;
        if ((chunkPosition.y - size) % (size * 4) == 0)
        {
            // The new chunk belongs to the bottom row
            id = (chunkPosition.x == 0) ? 1 : ((chunkPosition.x == size * 2) ? 2 : 0);
        }
        else if ((chunkPosition.y + size) % (size * 4) == 0)
        {
            // The new chunk belongs to the top row
            id = (chunkPosition.x == 0) ? 4 : ((chunkPosition.x == size * 2) ? 5 : 3);
        }

        return (id + seed) % currentChunksPattern.Chunks.Length;
    }

    /// <summary>
    /// Deactivates the chunks far enough from the player.
    /// </summary>
    private void DeactivateFarChunks()
    {
        foreach (Chunk chunk in chunks)
        {
            if (Vector2.Distance(chunk.Position, cameraTransform.position) > deactivationThreshold)
            {
                chunk.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Destroys the objects or the entire chunk if they are below the minimum limit.
    /// </summary>
    private void DestroyBelowLimitObjects()
    {
        foreach (Chunk chunk in chunks)
        {
            if (chunk.IsDestroyed) continue;

            if (chunk.Position.y < playerController.YLimit - size)
            {
                chunk.IsDestroyed = true;
                Destroy(chunk.Object);

                continue;
            }

            if (chunk.Position.y < player.transform.position.y)
            {
                foreach (Platform platform in chunk.Platforms)
                {
                    if (platform.Position.y < playerController.YLimit && !platform.IsDestroyed)
                    {
                        platform.IsDestroyed = true;
                        Destroy(platform.Object);
                    }
                }

                foreach (Obstacle obstacle in chunk.Obstacles)
                {
                    if (obstacle.Position.y < playerController.YLimit && !obstacle.IsDestroyed)
                    {
                        obstacle.IsDestroyed = true;
                        Destroy(obstacle.Object);
                    }
                }

                foreach (Coin coin in chunk.Coins)
                {
                    if (coin.Position.y < playerController.YLimit && !coin.IsDestroyed)
                    {
                        coin.IsDestroyed = true;
                        Destroy(coin.Object);
                    }
                }
            }
        }
    }

    // Invoked method: GameManager
    /// <summary>
    /// Resets the game view.
    /// </summary>
    public void ResetToPlay()
    {
        StartCoroutine(WaitToReloadChunks());

        IEnumerator WaitToReloadChunks()
        {
            // Reloads chunks only after the camera has moved
            yield return new WaitForSeconds(0.2f);

            int chunksCount = chunksParent.childCount;
            for (int i = 0; i < chunksCount; i++)
            {
                DestroyImmediate(chunksParent.GetChild(0).gameObject);
            }
            chunks.Clear();

            InitChunks();
        }
    }
}

[Serializable]
public class PlatformsInAltitudeBand
{
    #region Public References
    public int StartHeight => startHeight;
    public PlatformSO[] Platforms => platforms;
    #endregion

    [SerializeField] private string name;
    [SerializeField] private int startHeight;
    [SerializeField] private PlatformSO[] platforms;

    public static PlatformSO[] GetAllPlatforms(PlatformsInAltitudeBand[] platformsInAltitudeBand)
    {
        List<PlatformSO> list = new List<PlatformSO>();

        foreach (PlatformsInAltitudeBand piab in platformsInAltitudeBand)
        {
            foreach (PlatformSO so in piab.platforms)
            {
                list.Add(so);
            }
        }

        return list.ToArray();
    }
}

public class PlatformInChunk
{
    public int StartHeight { get; set; }
    public PlatformSO ScriptableObject { get; set; }
    public float SpawnProbability { get; set; }
}