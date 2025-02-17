using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    #region Public References
    public float YLimit => yLimit;
    public float YOffset => yOffset;
    public float YUseLimit => yUseLimit;

    public float Height => height;
    public int Coins => coins;

    public bool IsShielded { get; set; }
    #endregion

    [SerializeField] private float yOffset = 30;
    [SerializeField] private float yUseLimit = 15;      // The minimum height from which use limit logic

    private Vector3 startPosition;
    private float height;
    private int coins;

    private float yLimit;

    private bool isDied;
    private bool updateYLimit;

    [Space]
    public UnityEvent OnPlayerDie;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (transform.position.y - yOffset > yLimit && transform.position.y > YUseLimit && updateYLimit)
        {
            yLimit = transform.position.y - yOffset;
        }

        if (transform.position.y < yLimit && !isDied)
        {
            OnPlayerDie?.Invoke();

            isDied = true;
        }

        height = transform.position.y - startPosition.y;
    }

    /// <summary>
    /// Allows to update the y limit.
    /// </summary>
    public void AllowYLimitUpdate()
    {
        updateYLimit = true;
    }

    // Invoked method: GameManager
    /// <summary>
    /// Resets the game view.
    /// </summary>
    public void ResetToPlay()
    {
        transform.position = startPosition;
        transform.eulerAngles = Vector3.zero;

        coins = 0;
        yLimit = 0;
        isDied = false;
        // Ensures the game does not think the player is died when he is teleported to its start position
        updateYLimit = false;
    }

    /// <summary>
    /// Collectes a coin.
    /// </summary>
    /// <param name="value">The value of the coin</param>
    public void CollectCoin()
    {
        coins++;
    }
}
