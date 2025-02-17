using UnityEngine;
using System.Collections;

public enum PlatformType
{
    // Platforms
    Square,
    Circular,
    Spinner,

    // Helpers
    Zoomer,
    Sprinter,
    Shield
}

public class PlatformScript : MonoBehaviour
{
    #region Public References
    public PlatformType PlatformType => platformType;
    #endregion

    [SerializeField] private PlatformType platformType;

    private Transform player;
    private PlayerController playerController;
    private PlayerMovement playerMovement;
    private bool isPlayerSticked;
    private bool wasPlayerSticked;

    private DotweenManager dotweenManager;

    #region Spinner
    [HideInInspector] public float spinStep;
    #endregion

    #region Zoomer
    [HideInInspector] public float zoomOutMultiplier;
    [HideInInspector] public float zoomTime;

    private float zoomTimer;
    private CameraController cameraController;
    #endregion

    #region Sprinter
    [HideInInspector] public float sprintMultiplier;
    [HideInInspector] public float lookaheadMultiplier;

    private Tracker tracker;
    #endregion

    #region Shield
    [HideInInspector] public GameObject playerShieldPrefab;
    [HideInInspector] public float shieldRadius;
    [HideInInspector] public float shieldTime;

    private Transform playerShield;
    #endregion

    [HideInInspector] public bool disappearAfterUse;
    private bool isDisapeared;

    void Start()
    {
        if (IsPlatformType(PlatformType.Zoomer))
        {
            cameraController = FindObjectOfType<CameraController>();
        }
        else if (IsPlatformType(PlatformType.Sprinter))
        {
            tracker = FindObjectOfType<Tracker>();
        }
        else if (IsPlatformType(PlatformType.Shield))
        {
            dotweenManager = FindObjectOfType<DotweenManager>();
        }

        if (disappearAfterUse)
        {
            if (dotweenManager == null)
            {
                dotweenManager = FindObjectOfType<DotweenManager>();
            }
        }
    }

    void Update()
    {
        if (IsPlatformType(PlatformType.Zoomer))
        {
            if (isPlayerSticked != wasPlayerSticked)
            {
                zoomTimer = zoomTime;
            }

            if (zoomTimer > 0)
            {
                zoomTimer -= Time.deltaTime;
                float normalizedTimer;
                if (isPlayerSticked)
                {
                    // Zooms out
                    normalizedTimer = 1 - zoomTimer / zoomTime;
                    cameraController.ZoomLerp(zoomOutMultiplier, normalizedTimer);
                }
                else
                {
                    // Zooms in
                    normalizedTimer = zoomTimer / zoomTime;
                    cameraController.ZoomLerp(zoomOutMultiplier, normalizedTimer);
                }
            }

            wasPlayerSticked = isPlayerSticked;
        }
        else if (IsPlatformType(PlatformType.Shield))
        {
            if (playerShield != null && player.position.y < 20)
            {
                // Awful way to destroy the shield if the player restarts the game
                Destroy(playerShield.gameObject);
                playerController.IsShielded = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (IsPlatformType(PlatformType.Spinner))
        {
            transform.eulerAngles += new Vector3(0, 0, spinStep);
            if (isPlayerSticked)
            {
                player.RotateAround(transform.position, Vector3.forward, spinStep);
            }
        }
    }

    /// <summary>Sets the player reference.</summary>
    /// <param name="player">The player's transform.</param>
    public void SetPlayerReference(Transform player)
    {
        this.player = player;
        playerController = player.GetComponent<PlayerController>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    /// <summary>Sets the value of the bool isPlayerSticked.</summary>
    /// <param name="value">The value to set.</param>
    public void SetIsPlayerSticked(bool value)
    {
        isPlayerSticked = value;

        if (IsPlatformType(PlatformType.Sprinter))
        {
            playerMovement.MaxVelocityMultiplier = (isPlayerSticked) ? sprintMultiplier : 1;
            tracker.LookaheadMultiplier = (isPlayerSticked) ? lookaheadMultiplier : 1;
        }
        else if (IsPlatformType(PlatformType.Shield))
        {
            if (isPlayerSticked)
            {
                playerShield = Instantiate(playerShieldPrefab, player).transform;
                playerShield.position = player.position + player.right * playerMovement.Center.x + player.up * playerMovement.Center.y;
                playerShield.GetComponent<CircleCollider2D>().radius = shieldRadius;
                playerShield.GetChild(0).localScale = new Vector3(shieldRadius * 2, shieldRadius * 2);

                playerController.IsShielded = true;
            }
            else
            {
                StartCoroutine(LifeCounter());

                IEnumerator LifeCounter()
                {
                    yield return new WaitForSeconds(shieldTime - 2);

                    playerShield.GetComponent<Animator>().SetTrigger("end");

                    yield return new WaitForSeconds(2);

                    if (playerShield != null)
                    {
                        Destroy(playerShield.gameObject);
                        playerController.IsShielded = false;
                    }
                }
            }
        }

        if (!isPlayerSticked && disappearAfterUse)
        {
            // Destroys the collider of the platform not to interrupt the logic
            if (!isDisapeared)
            {
                Destroy(transform.GetChild(1).gameObject);
                dotweenManager.PlatformDisappear(transform.GetChild(0), transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>());
                isDisapeared = true;
            }
        }
    }

    /// <summary>Checks if the platform corresponds to the platform type given.</summary>
    /// <param name="platformType">The platform type to use to check.</param>
    /// <returns>Whether the platform type corresponds to the platform type given or not.</returns>
    private bool IsPlatformType(PlatformType platformType)
    {
        return this.platformType == platformType;
    }
}

public class Platform
{
    public GameObject Prefab { get; set; }
    public GameObject Object { get; set; }
    public PlatformType Type { get; set; }

    public Vector3 Position { get; private set; }
    public Vector3 EulerRotation { get; set; }

    public bool IsDestroyed { get; set; }

    /// <summary>Creates a virtual platform.</summary>
    /// <param name="position">The position of the platform.</param>
    public Platform(Vector3 position)
    {
        Position = position;
    }
}