using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    #region Public References
    public Vector3 Center => center;

    public float GravityScale => gravityScale;
    public float MaxVelocity => maxVelocity * MaxVelocityMultiplier;
    public float MaxVelocityMultiplier { private get; set; } = 1;
    public float MoveMultiplier => moveMultiplier;

    public int Steps => positions.Count;
    #endregion

    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager UIManager;
    private PlayerController controller;
    private SwipeListener swipeListener;

    [Header("Collider")]
    [SerializeField] private Vector2 center;
    [SerializeField] [Range(0.1f, 1)] private float radius;

    [Header("Movement")]
    [SerializeField] private float gravityScale = 5;
    [SerializeField] private float maxVelocity = 500;
    [SerializeField] [Range(0.1f, 1.5f)] private float moveMultiplier;
    [SerializeField] private LayerMask platformLayer;
    
    private Transform playerPlatform;   // The platform the player is sticked on
    private PlatformScript playerPlatformScript;    // The script of the platform the player is sticked on
    private readonly List<Vector3> positions = new List<Vector3>();
    private int positionsIndex;

    private float rotationStep;
    private Vector3 apex;

    private bool isInAir;
    private bool isOnWorldLimit;
    private bool isStopped;
    private bool ignoreCollisions;

    [Space]
    public UnityEvent OnPlayerStick;

    void Start()
    {
        controller = GetComponent<PlayerController>();
        swipeListener = GetComponent<SwipeListener>();
    }

    void FixedUpdate()
    {
        if (!isInAir) return;

        Move();

        Rotate();
    }

    // Invoked method: SwipeListener
    /// <summary>
    /// Callbacks the method for calculating the trajectory of the jump.
    /// </summary>
    public void OnJumpCalculate()
    {
        CalculateJump();
    }

    /// <summary>
    /// Calculates the trajectory of the jump.
    /// </summary>
    /// <param name="forcedInitialVelocity">The initial velocity that overrides the one given by the player.</param>
    private void CalculateJump(Vector2 forcedInitialVelocity = default)
    {
        int steps = 150;
        Vector3 position = transform.position;
        isStopped = false;

        positions.Clear();
        if (playerPlatformScript != null)
        {
            playerPlatformScript.SetIsPlayerSticked(false);
        }

        float timestep = Time.fixedDeltaTime;
        Vector3 gravittyAccel = gravityScale * timestep * timestep * Physics2D.gravity;

        Vector2 initialVelocity = (forcedInitialVelocity == Vector2.zero) ? swipeListener.Direction / 10 : forcedInitialVelocity;
        Vector3 moveStep = initialVelocity * timestep;
        Vector3 offset = transform.up * (radius + 0.05f) + transform.right * center.x;
        float x = (moveMultiplier * initialVelocity.x * initialVelocity.y) / (gravityScale * Mathf.Abs(Physics2D.gravity.y)) + transform.position.x;
        float y = (moveMultiplier * Mathf.Pow(initialVelocity.y, 2)) / (2 * gravityScale * Mathf.Abs(Physics2D.gravity.y)) + transform.position.y;
        apex = (initialVelocity.y <= 0) ? Vector3.zero : new Vector3(x, y);

        for (int i = 0; i < steps; i++)
        {
            moveStep += gravittyAccel;
            position += moveStep * moveMultiplier;

            Vector3 lastPosition = (i == 0) ? transform.position : positions[^1];
            Vector3 direction = position - lastPosition;

            if (ignoreCollisions)
            {
                positions.Add(position);
                continue;
            }

            if (isOnWorldLimit)
            {
                direction = new Vector3(0, moveStep.y);
                position = lastPosition + direction;
                if (CheckCollisionsInTrajectory(lastPosition + offset, direction, (forcedInitialVelocity != Vector2.zero)))
                {
                    isOnWorldLimit = false;
                    return;
                }
            }
            else if (CheckCollisionsInTrajectory(lastPosition + offset, direction, (forcedInitialVelocity != Vector2.zero)))
            {
                if (isOnWorldLimit) continue;

                // Fixes the positions according to the end position
                Vector3 delta = position - positions[^1];
                Vector3 deltaStep = delta / positions.Count;
                for (int j = 0; j < positions.Count - 1; j++)
                {
                    positions[j] -= deltaStep * (j + 1);
                }

                return;
            }

            positions.Add(position);
        }

        // The player will fall in the void
        isInAir = true;
        rotationStep = 10;
        playerPlatform = null;
    }

    /// <summary>Checks if the player will collides with a platform.</summary>
    /// <param name="center">The start point of the trajectory's segment.</param>
    /// <param name="direction">The direction of the trajectory's segment.</param>
    /// <param name="isFalling">Wheter the player is falling or not (with no intention). </param>
    /// <returns>Whether the circle cast has collided with a platform or not.</returns>
    private bool CheckCollisionsInTrajectory(Vector3 center, Vector3 direction, bool isFalling)
    {
        Vector3 endPosition = center + direction;
        float platformLimitX = Mathf.Sign(center.x) * gameManager.WorldLimitX;
        if (Mathf.Abs(endPosition.x) > gameManager.WorldLimitX)
        {
            // Clamps the x movement if the player is jumping over the world limits
            float y = center.y + (direction.y / direction.x) * (platformLimitX - center.x);
            positions.Add(new Vector3(platformLimitX, y));

            isOnWorldLimit = true;

            return true;
        }

        RaycastHit2D[] hits = Physics2D.CircleCastAll(center, radius, direction, direction.magnitude);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.CompareTag("Player")) continue;

            if (hit.transform == playerPlatform)
            {
                // If the player is jumping downwards, ignores the collisions with the platform that the player is sticked on
                if (apex == Vector3.zero && !isFalling) break;
                else
                {
                    // If the player is jumping upwards and he is going to fall down, ignores the collisions with the platform that the player is sticked on
                    // before he reaches the apex if it is off the platform
                    if (!hit.collider.OverlapPoint(apex) && direction.y > 0) break;
                }
            }

            if ((platformLayer & 1 << hit.collider.gameObject.layer) == 1 << hit.collider.gameObject.layer)
            {
                if (apex.y > controller.YUseLimit)
                {
                    // Ignores the collisions with the hit platform if it will be below the y limit
                    if (apex.y - controller.YOffset > hit.transform.position.y) break;
                }

                positions.Add(hit.point);

                float startAngle = (transform.eulerAngles.z + 90) * Mathf.PI / 180f;
                Vector2 startDirection = new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle));

                rotationStep = Vector2.SignedAngle(startDirection, hit.normal) / positions.Count;

                isInAir = true;

                playerPlatform = hit.transform;
                playerPlatformScript = GetPlatformScriptInPlatform(hit.transform);
                if (playerPlatformScript != null)
                {
                    playerPlatformScript.SetPlayerReference(transform);
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>Gets the PlatformScript searching through the parents of the transform of the collided platform.</summary>
    /// <param name="transform">The transform in which trying to get the script.</param>
    /// <returns>The script of the collided platform.</returns>
    private PlatformScript GetPlatformScriptInPlatform(Transform transform)
    {
        PlatformScript ps = transform.GetComponent<PlatformScript>();
        if (ps == null && transform.parent != null)
        {
            return GetPlatformScriptInPlatform(transform.parent);
        }

        return ps;
    }

    /// <summary>
    /// Moves the player traslating him.
    /// </summary>
    private void Move()
    {
        if (isStopped) return;

        if (playerPlatform == null && transform.position.y < controller.YLimit - 30)
        {
            // Stops the player from falling in the void
            positionsIndex = 0;
            isInAir = false;
            return;
        }

        if (positionsIndex == positions.Count - 1)
        {
            // Makes the player stick
            transform.position = positions[positionsIndex];
            OnPlayerStick?.Invoke();

            if (playerPlatformScript != null)
            {
                playerPlatformScript.SetIsPlayerSticked(true);
            }

            positionsIndex = 0;
            isInAir = false;
            return;
        }

        transform.position = positions[positionsIndex];

        positionsIndex++;
    }

    /// <summary>
    /// Rotates the player so that he is always standing on the platform.
    /// </summary>
    private void Rotate()
    {
        transform.eulerAngles += new Vector3(0, 0, rotationStep);
    }

    /// <summary>
    /// Stops the player to move.
    /// </summary>
    public void StopMove()
    {
        if (!isInAir) return;

        positions.RemoveRange(positionsIndex + 1, positions.Count - (positionsIndex + 1));
        positionsIndex = 0;
        rotationStep = 0;
        isStopped = true;
    }

    /// <summary>
    /// Makes the player to fall.
    /// </summary>
    /// <param name="ignoreCollisions">Whether ignoring all type of collisions or not.</param>
    public void Fall(bool ignoreCollisions)
    {
        this.ignoreCollisions = ignoreCollisions;
        CalculateJump(Vector2.down * 0.2f);
    }

    /// <summary>
    /// Makes the player to fall applying an initial velocity that overrides the one given by the player.
    /// </summary>
    /// <param name="ignoreCollisions">Whether ignoring all type of collisions or not.</param>
    /// <param name="forcedInitialVelocity">The initial velocity that overrides the one given by the player.</param>
    public void Fall(bool ignoreCollisions, Vector2 forcedInitialVelocity)
    {
        this.ignoreCollisions = ignoreCollisions;
        CalculateJump(forcedInitialVelocity);
    }

    // Invoked method: GameManager
    /// <summary>
    /// Resets the game view.
    /// </summary>
    public void ResetToPlay()
    {
        positions.Clear();
        positionsIndex = 0;
        isInAir = false;
        ignoreCollisions = false;

        playerPlatform = null;
        if (playerPlatformScript != null)
        {
            playerPlatformScript.SetIsPlayerSticked(false);
            playerPlatformScript = null;
        }
    }
}
