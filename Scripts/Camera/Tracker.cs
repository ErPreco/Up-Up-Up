using UnityEngine;

public class Tracker : MonoBehaviour
{
    #region Public Refernces
    public float LookaheadTime => trackerOffset.magnitude / (lookaheadRange * LookaheadMultiplier);
    public float LookaheadMultiplier { private get; set; } = 1;
    #endregion

    [SerializeField] private Transform player;
    private SwipeListener swipeListener;
    private PlayerMovement playerMovement;

    [SerializeField] [Range(0, 5)] private float lookaheadRange = 4;
    private float ratio;
    private Vector3 lookaheadVector;
    private Vector3 trackerOffset;

    void Start()
    {
        swipeListener = player.GetComponent<SwipeListener>();
        playerMovement = player.GetComponent<PlayerMovement>();

        transform.position = player.position;
        ratio = lookaheadRange / playerMovement.MaxVelocity;
    }

    void FixedUpdate()
    {
        if (swipeListener.Direction == Vector2.zero)
        {
            if (playerMovement.Steps != 0 && trackerOffset.magnitude > 0.1f)
            {
                if (Mathf.Abs(trackerOffset.x) - Mathf.Abs(lookaheadVector.x) / playerMovement.Steps > 0 &&
                    Mathf.Abs(trackerOffset.y) - Mathf.Abs(lookaheadVector.y) / playerMovement.Steps > 0)
                {
                    // Decreases the tracker offset so that it smoothly reaches the player position if he is jumping
                    trackerOffset -= lookaheadVector / playerMovement.Steps;
                }
            }
        }
        else
        {
            lookaheadVector = swipeListener.Direction * ratio * LookaheadMultiplier;
            trackerOffset = lookaheadVector;
        }

        transform.position = player.position + trackerOffset;
    }

    // Invoked method: GameManager
    /// <summary>
    /// Resets the game view.
    /// </summary>
    public void ResetToPlay()
    {
        transform.position = player.position;
    }
}
