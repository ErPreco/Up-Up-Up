using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    #region Public References
    public GameObject Object => chunkObject;

    public Vector3 Position => position;
    public List<Platform> Platforms => platforms;
    public Platform Helper => helper;

    public List<Obstacle> Obstacles => obstacles;

    public List<Coin> Coins => coins;

    public bool IsDestroyed { get; set; }
    #endregion

    private Vector3 position;
    private GameObject chunkObject;
    
    private readonly List<Platform> platforms = new List<Platform>();
    private int[] platformsCount;
    private int altitudeBandIndex;

    private Platform helper;
    private int helperSOIndex = -1;
    private int helperIndex = -1;   // Only one helper can be placed in each chunk

    private readonly List<Obstacle> obstacles = new List<Obstacle>();

    private readonly List<Coin> coins = new List<Coin>();

    /// <summary>
    /// Creates a chunk.
    /// </summary>
    /// <param name="position">The position of the chunk.</param>
    /// <param name="platformsPosition">The list of the local position of the platforms that are going to spawn in the chunk.</param>
    /// <param name="obstaclesPosition">The list of the local position of the obstacles that are going to spawn in the chunk.</param>
    /// <param name="coinsPosition">The list of the local position of the coins that are going to spawn in the chunk.</param>
    /// <param name="minHeightForSpawning">The minimum height in unity unit from which to spawn the elements.</param>
    public Chunk(Vector3 position, List<Vector3> platformsPosition, List<Vector3> obstaclesPosition, List<Vector3> coinsPosition, float minHeightForSpawning)
    {
        this.position = position;
        
        foreach (Vector3 localPos in platformsPosition)
        {
            Vector3 platformPosition = localPos + position;
            if (platformPosition.y >= minHeightForSpawning)
            {
                platforms.Add(new Platform(platformPosition));
            }
        }

        foreach (Vector3 localPos in obstaclesPosition)
        {
            Vector3 obstaclePosition = localPos + position;
            if (obstaclePosition.y >= minHeightForSpawning)
            {
                obstacles.Add(new Obstacle(obstaclePosition));
            }
        }

        foreach (Vector3 localPos in coinsPosition)
        {
            Vector3 coinPosition = localPos + position;
            if (coinPosition.y >= minHeightForSpawning)
            {
                coins.Add(new Coin(coinPosition));
            }
        }
    }

    /// <summary>
    /// Sets how many platforms will be placed according to their spawn probability and the altitude.
    /// </summary>
    /// <param name="chunksManagerPlatforms">The platforms array of the ChunksManager script.</param>
    /// <param name="platformsInAltitudeBand">The amount of platforms in an altitude band.</param>
    public void SetPlatforms(PlatformInChunk[] chunksManagerPlatforms, int platformsInAltitudeBand)
    {
        int altitudeBands = chunksManagerPlatforms.Length / platformsInAltitudeBand;
        altitudeBandIndex = 0;

        for (int i = 0; i < altitudeBands; i++)
        {
            if (chunksManagerPlatforms[i * platformsInAltitudeBand].StartHeight > position.y) break;
            altitudeBandIndex = i * platformsInAltitudeBand;
        }

        platformsCount = new int[platformsInAltitudeBand];
        for (int i = 0; i < platformsInAltitudeBand; i++)
        {
            platformsCount[i] = (Mathf.RoundToInt(platforms.Count * chunksManagerPlatforms[altitudeBandIndex + i].SpawnProbability));
        }
    }

    /// <summary>
    /// Finds a spot to place an helper, if it is possible.
    /// </summary>
    /// <param name="helpersSO">The helpers scriptable object array of the ChunksManager script.</param>
    /// <param name="chunks">The list of chunks.</param>
    /// <param name="helperSpawnThreshold">The minimum distance between two helpers.</param>
    /// <param name="heightMultiplier">The value to convert unity unit to meters.</param>
    public void SetHelpers(PlatformSO[] helpersSO, List<Chunk> chunks, int helperSpawnThreshold, float heightMultiplier)
    {
        for (int i = 0; i < helpersSO.Length; i++)
        {
            float startHeight = helpersSO[i].StartHeight / heightMultiplier;
            if (position.y < startHeight) continue;

            if (Random.value > 0.5f)
            {
                for (int j = 0; j < platforms.Count; j++)
                {
                    if (platforms[j].Position.y < startHeight) continue;

                    // Loops through the platforms of the chunk and checks if there is a valid spot to place the helper
                    bool isGoodPos = true;
                    foreach (Chunk chunk in chunks)
                    {
                        if (chunk.Helper == null) continue;

                        if (Vector2.Distance(chunk.Helper.Position, platforms[j].Position) < helperSpawnThreshold ||
                            (chunk.Helper.Type == helpersSO[i].Type && Vector2.Distance(chunk.Helper.Position, platforms[j].Position) < helpersSO[i].SameTypeSpawnThreshold))
                        {
                            isGoodPos = false;
                            break;
                        }
                    }

                    if (isGoodPos)
                    {
                        // If a good spot has been found, stores the SO index and the chunk platforms list index
                        helperSOIndex = i;
                        helperIndex = j;
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Initializes all the platforms of the chunk.
    /// </summary>
    /// <param name="chunksManagerPlatforms">The platforms array of the ChunksManager script.</param>
    /// <param name="helpersSO">The helpers scriptable object array of the ChunksManager script.</param>
    public void InitAllPlatforms(PlatformInChunk[] chunksManagerPlatforms, PlatformSO[] helpersSO)
    {
        for (int i = 0; i < platformsCount.Length; i++)
        {
            GameObject prefab = chunksManagerPlatforms[altitudeBandIndex + i].ScriptableObject.Prefab;
            PlatformType type = chunksManagerPlatforms[altitudeBandIndex + i].ScriptableObject.Type;
            int maxTilt = chunksManagerPlatforms[altitudeBandIndex + i].ScriptableObject.MaxTilt;

            for (int j = 0; j < platformsCount[i]; j++)
            {
                if (helperIndex != -1 && i == 0 && j == 0)
                {
                    // If an helper must be placed, substitutes it with the first platform of the most populous platform
                    platforms[helperIndex].Prefab = helpersSO[helperSOIndex].Prefab;
                    platforms[helperIndex].Type = helpersSO[helperSOIndex].Type;
                    platforms[helperIndex].EulerRotation = RandomRotation(helpersSO[helperSOIndex].MaxTilt);
                    helper = platforms[helperIndex];
                    continue;
                }

                int index = Random.Range(0, platforms.Count);
                while (platforms[index].Prefab != null)
                {
                    // Increments the random index to find an empty platform slot
                    index++;
                    index %= platforms.Count;
                }

                platforms[index].Prefab = prefab;
                platforms[index].Type = type;
                platforms[index].EulerRotation = RandomRotation(maxTilt);
            }
        }


        //for (int i = 0; i < chunksManagerPlatforms.Length; i++)
        //{
        //    GameObject prefab = chunksManagerPlatforms[i].ScriptableObject.Prefab;
        //    PlatformType type = chunksManagerPlatforms[i].ScriptableObject.Type;
        //    int maxTilt = chunksManagerPlatforms[i].ScriptableObject.MaxTilt;
        //    for (int j = 0; j < platformsCount[i]; j++)
        //    {
        //        if (helperIndex != -1 && i == 0 && j == 0)
        //        {
        //            // If an helper must be placed, substitutes it with the first platform of the platforms dictionary
        //            platforms[helperIndex].Prefab = helpersSO[helperSOIndex].Prefab;
        //            platforms[helperIndex].Type = helpersSO[helperSOIndex].Type;
        //            platforms[helperIndex].EulerRotation = RandomRotation(helpersSO[helperSOIndex].MaxTilt);
        //            helper = platforms[helperIndex];
        //            continue;
        //        }

        //        int index = Random.Range(0, platforms.Count);
        //        while (platforms[index].Prefab != null)
        //        {
        //            // Increments the random index to find an empty platform slot
        //            index++;
        //            index %= platforms.Count;
        //        }

        //        platforms[index].Prefab = prefab;
        //        platforms[index].Type = type;
        //        platforms[index].EulerRotation = RandomRotation(maxTilt);
        //    }
        //}
    }

    /// <summary>
    /// Sets the spot for the obstacles.
    /// </summary>
    /// <param name="obstaclesSO">The obstacles scriptable object array of the ChunksManager script.</param>
    /// <param name="chunks">The list of chunks.</param>
    /// <param name="heightMultiplier">The value to convert unity unit to meters.</param>
    public void SetObstacles(ObstacleSO[] obstaclesSO, List<Chunk> chunks, float heightMultiplier)
    {
        int obstaclesListCount = obstacles.Count;
        int obstaclesRemovedCount = 0;
        for (int i = 0; i < obstaclesListCount; i++)
        {
            Obstacle obstacle = obstacles[i - obstaclesRemovedCount];
            bool isObstaclePlaced = false;
            foreach (ObstacleSO obstacleSO in obstaclesSO)
            {
                if (Random.value > 0.3f)
                {
                    float startHeight = obstacleSO.StartHeight / heightMultiplier;
                    if (obstacle.Position.y < startHeight) continue;

                    isObstaclePlaced = true;
                    foreach (Obstacle otherObstacleInChunk in obstacles)
                    {
                        // Loops thorugh the placed obstacles in the chunk and checks if the current spot is valid
                        if (otherObstacleInChunk == obstacle) continue;
                        if (otherObstacleInChunk.Prefab == null) continue;

                        if (otherObstacleInChunk.Type == obstacleSO.Type && Vector2.Distance(obstacle.Position, otherObstacleInChunk.Position) < obstacleSO.SameTypeSpawnThreshold)
                        {
                            isObstaclePlaced = false;
                            break;
                        }
                    }

                    if (isObstaclePlaced)
                    {
                        foreach (Chunk chunk in chunks)
                        {
                            foreach (Obstacle otherObstacle in chunk.Obstacles)
                            {
                                // Loops through the obstacles of the other chunks and checks if the current spot is valid
                                if (otherObstacle.Type == obstacleSO.Type && Vector2.Distance(otherObstacle.Position, obstacle.Position) < obstacleSO.SameTypeSpawnThreshold)
                                {
                                    isObstaclePlaced = false;
                                    break;
                                }
                            }

                            if (!isObstaclePlaced) break;
                        }
                    }
                }

                if (isObstaclePlaced)
                {
                    // If the obstacle can be placed, initializes the obstacle and go to the next spot
                    InitObstacles(obstacle, obstacleSO);
                    break;
                }
            }

            if (!isObstaclePlaced)
            {
                // If no obstacles will be placed in the current spot, deletes it
                obstacles.Remove(obstacle);
                obstaclesRemovedCount++;
            }
        }
    }

    /// <summary>
    /// Initializes the obstacle given.
    /// </summary>
    /// <param name="obstacle">The obstacle to initializes.</param>
    /// <param name="obstacleSO">The scriptable object to get the information from.</param>
    private void InitObstacles(Obstacle obstacle, ObstacleSO obstacleSO)
    {
        obstacle.Prefab = obstacleSO.Prefab;
        obstacle.Type = obstacleSO.Type;
    }

    /// <summary>
    /// Sets the spot for the coins.
    /// </summary>
    /// <param name="prefab">The prefab of the coin.</param>
    public void SetCoins(GameObject prefab)
    {
        int coinsListCount = coins.Count;
        int coinsRemovedCount = 0;
        for (int i = 0; i < coinsListCount; i++)
        {
            Coin coin = coins[i - coinsRemovedCount];
            if (Random.value > 0.3f)
            {
                coin.Prefab = prefab;
            }
            else
            {
                coins.Remove(coin);
                coinsRemovedCount++;
            }
        }
    }

    /// <summary>
    /// Generates a random rotation in degrees.
    /// </summary>
    /// <param name="maxTilt">The maximum tilt of the platform.</param>
    /// <returns>The platform euler rotation on the z axis.</returns>
    private Vector3 RandomRotation(int maxTilt)
    {
        return new Vector3(0, 0, Random.Range(-maxTilt, maxTilt));
    }

    /// <summary>
    /// Instantiates the new chunk and its elements.
    /// </summary>
    /// <param name="chunksParent">The parent of the chunk object.</param>
    public void InstantiateChunk(Transform chunksParent, float worldLimitX)
    {
        chunkObject = new GameObject("Chunk (" + (chunksParent.childCount) + ")");
        chunkObject.transform.parent = chunksParent;
        chunkObject.transform.position = position;

        foreach (Platform platform in platforms)
        {
            if (Mathf.Abs(platform.Position.x) > worldLimitX) continue;

            Transform platformTransform = UnityEngine.Object.Instantiate(platform.Prefab, chunkObject.transform).transform;
            platform.Object = platformTransform.gameObject;
            platformTransform.position = platform.Position;
            platformTransform.eulerAngles = platform.EulerRotation;
        }

        foreach (Obstacle obstacle in obstacles)
        {
            if (Mathf.Abs(obstacle.Position.x) > worldLimitX) continue;

            Transform obstacleTransform = UnityEngine.Object.Instantiate(obstacle.Prefab, chunkObject.transform).transform;
            obstacle.Object = obstacleTransform.gameObject;
            obstacleTransform.position = obstacle.Position;
        }

        foreach (Coin coin in coins)
        {
            if (Mathf.Abs(coin.Position.x) > worldLimitX) continue;

            Transform coinTransform = UnityEngine.Object.Instantiate(coin.Prefab, chunkObject.transform).transform;
            coin.Object = coinTransform.gameObject;
            coinTransform.position = coin.Position;
        }
    }

    /// <summary>
    /// Activates or deactivates the chunk game object.
    /// </summary>
    /// <param name="value">The activation value.</param>
    public void SetActive(bool value)
    {
        if (chunkObject != null)
        {
            chunkObject.SetActive(value);
        }
    }
}
