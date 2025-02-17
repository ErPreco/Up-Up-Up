using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ObstacleType
{
    ElectricShock,
    Laser,
    Shooter
}

public class ObstacleScript : MonoBehaviour
{
    #region Public References
    public ObstacleType ObstacleType => obstacleType;
    #endregion

    [SerializeField] private ObstacleType obstacleType;

    private GameManager gameManager;
    
    private Transform player;
    private PlayerController playerController;
    private PlayerMovement playerMovement;
    private SwipeListener swipeListener;

    #region Electric Shock / Laser
    [HideInInspector] public GameObject lightning;
    [HideInInspector] public GameObject lightningCollider;

    [HideInInspector] public float activeTime;
    [HideInInspector] public float inactiveTime;

    [HideInInspector] public float stunTime;

    private bool useElectrickShock = true;
    private bool useLaser = true;
    #endregion

    #region Shooter
    [HideInInspector] public GameObject projectilePrefab;
    [HideInInspector] public Transform shootPoint;

    [HideInInspector] public int activationThreshold;
    [HideInInspector] public float reloadingTime;

    private bool useShooter = true;
    private bool activateShooter;
    private bool hasShot;
    private readonly List<GameObject> projectiles = new List<GameObject>();
    #endregion

    void Start()
    {
        if (IsObstacleType(ObstacleType.ElectricShock) || IsObstacleType(ObstacleType.Laser))
        {
            StartCoroutine(Clock());
        }
        else if (IsObstacleType(ObstacleType.Shooter))
        {
            gameManager = FindObjectOfType<GameManager>();

            player = FindObjectOfType<PlayerController>().transform;
            playerController = player.GetComponent<PlayerController>();
            playerMovement = player.GetComponent<PlayerMovement>();
            swipeListener = player.GetComponent<SwipeListener>();
        }
    }

    void Update()
    {
        if (IsObstacleType(ObstacleType.Shooter))
        {
            if (!gameManager.IsPlaying)
            {
                if (projectiles.Count > 0)
                {
                    projectiles.ForEach((p) => Destroy(p));
                    projectiles.Clear();
                }
                return;
            }

            if (!useShooter) return;

            if (Vector2.Distance(player.position, transform.position) < activationThreshold)
            {
                activateShooter = true;
                if (!hasShot)
                {
                    StartCoroutine(Shoot());
                }
            }
            else
            {
                activateShooter = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (IsObstacleType(ObstacleType.Shooter) && activateShooter)
        {
            transform.LookAt(player, Vector3.right);
        }
    }

    #region Electric Shock / Laser
    /// <summary>
    /// Alternates the active/inactive state of the laser.
    /// </summary>
    private IEnumerator Clock()
    {
        while (true)
        {
            // Inactive logic
            lightning.SetActive(false);
            lightningCollider.SetActive(false);

            yield return new WaitForSeconds(inactiveTime);

            // Active logic
            lightning.SetActive(true);
            lightningCollider.SetActive(true);

            yield return new WaitForSeconds(activeTime);
        }
    }

    #region Electric Shock
    /// <summary>
    /// Stuns the player and then reactivates the electric shock.
    /// </summary>
    private IEnumerator WaitToReactivateElectricShock()
    {
        yield return new WaitForSeconds(stunTime);

        playerMovement.Fall(false);

        yield return new WaitForSeconds(0.2f);

        useElectrickShock = true;
    }
    #endregion

    #region Laser
    /// <summary>
    /// Stuns the player and then makes him die.
    /// </summary>
    private IEnumerator StunAndKill()
    {
        yield return new WaitForSeconds(stunTime);

        playerMovement.Fall(true);
    }
    #endregion
    #endregion

    #region Shooter
    /// <summary>
    /// Shoots a projectile.
    /// </summary>
    private IEnumerator Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab);
        projectile.transform.position = shootPoint.position;
        projectile.transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.x);

        projectile.transform.GetChild(0).GetComponent<OnTriggerEvent>().OnTriggerEnter.AddListener(OnTriggeredEnter);
        projectiles.Add(projectile);

        hasShot = true;

        yield return new WaitForSeconds(reloadingTime);

        hasShot = false;
    }
    #endregion

    /// <summary>Checks if the obstacle corresponds to the obstacle type given.</summary>
    /// <param name="obstacleType">The obstacle type to use to check.</param>
    /// <returns>Whether the obstacle type corresponds to the obstacle type given or not.</returns>
    private bool IsObstacleType(ObstacleType obstacleType)
    {
        return this.obstacleType == obstacleType;
    }

    /// <summary>
    /// Gets the player transform searching through the parents of the collided object.
    /// </summary>
    /// <param name="transform">The transform of the collided object.</param>
    /// <returns>The transform of the player root.</returns>
    private Transform GetPlayerTransform(Transform transform)
    {
        PlayerController controller = transform.GetComponent<PlayerController>();
        if (controller == null)
        {
            return GetPlayerTransform(transform.parent);
        }

        return transform;
    }

    // Invoked method: ElectricShock/Collider/OnTriggerEvent
    //                 Laser/Collider/OnTriggerEvent
    /// <summary>
    /// Callbacks when the player passes through the obstacle.
    /// </summary>
    /// <param name="other">The collider that has passed through the obstacle.</param>
    public void OnTriggeredEnter(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (player == null)
        {
            player = GetPlayerTransform(other.transform);
            playerController = player.GetComponent<PlayerController>();
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        if (playerController.IsShielded) return;

        if (IsObstacleType(ObstacleType.ElectricShock) && useElectrickShock)
        {
            useElectrickShock = false;
            playerMovement.StopMove();
            StartCoroutine(WaitToReactivateElectricShock());
        }
        else if (IsObstacleType(ObstacleType.Laser) && useLaser)
        {
            useLaser = false;
            playerMovement.StopMove();
            StartCoroutine(StunAndKill());
        }
        else if (IsObstacleType(ObstacleType.Shooter))
        {
            projectiles.ForEach((p) => Destroy(p));
            projectiles.Clear();
            useShooter = false;

            swipeListener.AllowSwipe(false);
            playerMovement.StopMove();
            playerMovement.Fall(true, Vector2.up * 15);
        }
    }
}

public class Obstacle
{
    public GameObject Prefab { get; set; }
    public GameObject Object { get; set; }
    public ObstacleType Type { get; set; }

    public Vector3 Position { get; private set; }

    public bool IsDestroyed { get; set; }

    /// <summary>Creates a virtual obstacle.</summary>
    /// <param name="position">The position of the obstacle.</param>
    public Obstacle(Vector3 position)
    {
        Position = position;
    }
}