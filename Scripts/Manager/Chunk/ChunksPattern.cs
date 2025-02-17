using UnityEngine;

[ExecuteAlways]
public class ChunksPattern : MonoBehaviour
{
    #region Public Refernces
    public int StartHeight => startHeightInUnityUnits;
    public float MinDistance => minDistance;

    public ChunkLayout[] Chunks => GetChunksLayout();
    #endregion

    [Tooltip("The height in meters from which start using this pattern")]
    [SerializeField] private int startHeight;
    [SerializeField] private float minDistance;
    private int startHeightInUnityUnits;

    private GameManager gameManager;
    private ChunksManager chunksManager;

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            if (chunksManager == null)
            {
                chunksManager = FindObjectOfType<ChunksManager>();
            }
        }

        StartHeightInUnityUnits();
    }

    /// <summary>
    /// Calculates the start height of the pattern.
    /// </summary>
    private void StartHeightInUnityUnits()
    {
        if (gameManager == null || chunksManager == null) return;

        int height = Mathf.RoundToInt(startHeight / gameManager.HeightMultiplier);
        int chunkSize = Mathf.RoundToInt(chunksManager.Size * 2);
        int mod = height % (chunkSize * 2);

        if (mod == 0)
        {
            startHeightInUnityUnits = height;
        }
        else if (mod < chunkSize)
        {
            startHeightInUnityUnits = height - mod;
        }
        else
        {
            startHeightInUnityUnits = height + (chunkSize * 2) - mod;
        }
    }

    /// <summary>
    /// Gets an array of the chunks layout.
    /// </summary>
    /// <returns>An array of the chunks layout.</returns>
    private ChunkLayout[] GetChunksLayout()
    {
        ChunkLayout[] chunks = new ChunkLayout[6];

        for (int i = 0; i < transform.childCount; i++)
        {
            chunks[i] = transform.GetChild(i).GetComponent<ChunkLayout>();
        }

        return chunks;
    }
}
