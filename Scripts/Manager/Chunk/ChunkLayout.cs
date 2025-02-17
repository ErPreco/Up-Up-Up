using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ChunkLayout : MonoBehaviour
{
    [SerializeField] private bool showBounds;

    [Space]
    [SerializeField] private Transform platformsParent;
    [SerializeField] private Transform obstaclesParent;
    [SerializeField] private Transform coinsParent;

    private ChunksPattern chunksPattern;
    private ChunksManager chunksManager;

    void Start()
    {
        if (!Application.isPlaying)
        {
            if (chunksPattern == null)
            {
                chunksPattern = transform.parent.GetComponent<ChunksPattern>();
            }

            if (chunksManager == null)
            {
                chunksManager = FindObjectOfType<ChunksManager>();
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (chunksPattern == null || chunksManager == null) return;

            // Draws the chunk bounds
            if (showBounds)
            {
                Gizmos.DrawWireCube(transform.position, 2 * chunksManager.Size * Vector2.one);
                Gizmos.DrawWireCube(transform.position, Vector2.one * (chunksManager.Size * 2 - chunksPattern.MinDistance));
            }

            // Draws circles representing the platforms
            foreach (Transform platform in platformsParent)
            {
                Gizmos.DrawWireSphere(platform.position, 1.5f);
            }
            // Draws squares representing the obstacles
            foreach (Transform obstacle in obstaclesParent)
            {
                Gizmos.DrawWireCube(obstacle.position, Vector3.one * 3);
            }
            // Draws little yellow circles representing the coins
            foreach (Transform coin in coinsParent)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(coin.position, 0.8f);
                Gizmos.color = Color.white;
            }

            // Draws a line between two platforms that are too close
            for (int i = 0; i < platformsParent.childCount; i++)
            {
                for (int j = 0; j < platformsParent.childCount; j++)
                {
                    if (i == j) continue;

                    if (Vector2.Distance(platformsParent.GetChild(i).position, platformsParent.GetChild(j).position) < chunksPattern.MinDistance)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(platformsParent.GetChild(i).position, platformsParent.GetChild(j).position);
                        Gizmos.color = Color.white;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets a list containing the local position of all platforms.
    /// </summary>
    /// <returns>The list of the local position of the platforms.</returns>
    public List<Vector3> GetPlatformsPosition()
    {
        List<Vector3> list = new List<Vector3>();
        foreach (Transform platform in platformsParent)
        {
            list.Add(platform.localPosition);
        }

        return list;
    }

    /// <summary>
    /// Gets a list containing the local position of all obstacles.
    /// </summary>
    /// <returns>The list of the local position of the obstacles.</returns>
    public List<Vector3> GetObstaclesPosition()
    {
        List<Vector3> list = new List<Vector3>();
        foreach (Transform obstacle in obstaclesParent)
        {
            list.Add(obstacle.localPosition);
        }

        return list;
    }

    /// <summary>
    /// Gets a list containing the local position of all obstacles.
    /// </summary>
    /// <returns>The list of the local position of the coins.</returns>
    public List<Vector3> GetCoinsPosition()
    {
        List<Vector3> list = new List<Vector3>();
        foreach (Transform coin in coinsParent)
        {
            list.Add(coin.localPosition);
        }

        return list;
    }
}
