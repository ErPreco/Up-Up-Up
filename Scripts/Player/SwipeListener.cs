using UnityEngine;
using UnityEngine.Events;

public class SwipeListener : MonoBehaviour
{
    #region Public References
    public Vector2 Direction => direction;
    #endregion

    [SerializeField] private PlayerMovement movement;

    [Space]
    [SerializeField] private float swipeRange = 50f;        // Range in pixels
    [SerializeField] private float tapRange = 10f;          // Range in pixels
    [SerializeField] private float tapTollerance = 0.2f;    // Tollerance in seconds

    [Space]
    [SerializeField] private bool useAngleRange;
    [SerializeField] [Range(1, 180)] private float angleRange;       // Range, in degrees, symmetrical about the local y axis
    private float minAngle;     // Minimum angle, in degrees, relative to the local system of axes
    private float maxAngle;     // Maximum angle, in degrees, relative to the local system of axes

    private Vector2 startTouchPos;
    private Vector2 direction;

    private float timer;

    private bool isSwipeStarted;
    private bool allowSwipe;

    private int swipeTechniqueMultiplier = 1;
    private float swipeSensitivity;

    [Space]
    [SerializeField] private Transform origin;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] [Range(1, 100)] private int steps;

    [Space]
    public UnityEvent OnSwipeStart;
    public UnityEvent OnSwipeEnd;
    public UnityEvent OnTapEnd;

    void Start()
    {
        RetrievePlayerPrefs();
    }

    void Update()
    {
        if (!allowSwipe) return;

        InitSwipe();

        CheckSwipe();

        EndSwipe();
    }

    /// <summary>
    /// Retrieves the player prefs values.
    /// </summary>
    private void RetrievePlayerPrefs()
    {
        int swipeTechniqueMultiplierStored = DataManager.GetData<int>("swipeTechniqueMultiplier");
        if (swipeTechniqueMultiplierStored == 0)
        {
            DataManager.SaveData("swipeTechniqueMultiplier", 1);
        }
        else
        {
            swipeTechniqueMultiplier = swipeTechniqueMultiplierStored;
        }

        float swipeSensitivityStored = DataManager.GetData<float>("swipeSensitivity");
        if (swipeSensitivityStored == 0)
        {
            DataManager.SaveData("swipeSensitivity", 1f);
        }
        else
        {
            swipeSensitivity = swipeSensitivityStored;
        }
    }

    /// <summary>
    /// Gets the position of the first touch.
    /// </summary>
    private void InitSwipe()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startTouchPos = Input.GetTouch(0).position;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
        }
    }

    /// <summary>
    /// Gets the current touch and draws the trajectory.
    /// </summary>
    private void CheckSwipe()
    {
        if (startTouchPos == Vector2.zero) return;

        // Keeps track of the time elapsed from the first touch
        timer += Time.deltaTime;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            direction = Input.GetTouch(0).position - startTouchPos;
            direction *= swipeTechniqueMultiplier * swipeSensitivity;
        }
        else if (Input.GetMouseButton(0))
        {
            direction = (Vector2)Input.mousePosition - startTouchPos;
            direction *= swipeTechniqueMultiplier * swipeSensitivity;
        }

        if (direction.magnitude > swipeRange)
        {
            // Draws the plot
            ClampDirection();

            Vector2[] trajectory = Plot(origin.position, direction / 10f, steps);

            lineRenderer.positionCount = trajectory.Length;

            Vector3[] positions = new Vector3[trajectory.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = trajectory[i];
            }
            lineRenderer.SetPositions(positions);

            if (!isSwipeStarted)
            {
                //TODO: toggles player animation
                OnSwipeStart?.Invoke();

                isSwipeStarted = true;
            }
        }
        else if (lineRenderer.positionCount != 0)
        {
            // Cancels the plot if direction.magnitude < swipeRange
            lineRenderer.positionCount = 0;
        }
    }

    /// <summary>
    /// Clamps the direction vector about its angle and its magnitude.
    /// </summary>
    private void ClampDirection()
    {
        direction = Vector2.ClampMagnitude(direction, movement.MaxVelocity);

        if (!useAngleRange) return;

        // Sets the clamp angles based on the player's y axis turning counterclockwise
        if (transform.position.y < 1)
        {
            // Changes clamp angles if the player is on the ground
            maxAngle = 80;
            minAngle = 280;
        }
        else
        {
            maxAngle = angleRange;
            minAngle = 360 - angleRange;
        }

        float directionAngleInGlobalSystem = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float directionAngleInLocalSystem = directionAngleInGlobalSystem - transform.eulerAngles.z;
        directionAngleInLocalSystem += (directionAngleInLocalSystem < -180) ? 270 : -90;
        directionAngleInLocalSystem += (directionAngleInLocalSystem < 0) ? 360 : 0;

        if (directionAngleInLocalSystem > maxAngle && directionAngleInLocalSystem < 180)
        {
            directionAngleInGlobalSystem = ((maxAngle + 90 > 180) ? maxAngle - 270 : maxAngle + 90) + transform.eulerAngles.z;
        }
        else if (directionAngleInLocalSystem < minAngle && directionAngleInLocalSystem > 180)
        {
            directionAngleInGlobalSystem = ((minAngle + 90 > 180) ? minAngle - 270 : minAngle + 90) + transform.eulerAngles.z;
        }

        direction = (new Vector2(Mathf.Cos(directionAngleInGlobalSystem * Mathf.Deg2Rad), Mathf.Sin(directionAngleInGlobalSystem * Mathf.Deg2Rad))) * direction.magnitude;
    }

    /// <summary>
    /// Manages the logic of the player's interaction.
    /// </summary>
    private void EndSwipe()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            ClassifySwipe();
            timer = 0;
            direction = Vector2.zero;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ClassifySwipe();
            timer = 0;
            direction = Vector2.zero;
        }
    }

    /// <summary>
    /// Invokes the related tap and swipe events.
    /// </summary>
    private void ClassifySwipe()
    {
        if (direction.magnitude < tapRange && timer <= tapTollerance)
        {
            // Tap logic
            OnTapEnd?.Invoke();
        }
        else if (direction.magnitude > swipeRange)
        {
            // Swipe logic
            OnSwipeEnd?.Invoke();
            AllowSwipe(false);

            // Cancels the plot
            lineRenderer.positionCount = 0;

            isSwipeStarted = false;
        }
    }

    /// <summary>Calculates the points to draw the line of the trajectory.</summary>
    /// <param name="rigidbody">The rigidbody of the player.</param>
    /// <param name="position">The start position of the player.</param>
    /// <param name="velocity">The initial velocity.</param>
    /// <param name="steps">The amount of points used to draw the line.</param>
    /// <returns>The array of the trajectory points.</returns>
    private Vector2[] Plot(Vector2 position, Vector2 velocity, int steps)
    {
        Vector2[] results = new Vector2[steps];

        float timestep = Time.fixedDeltaTime / Physics2D.velocityIterations;
        Vector2 gravittyAccel = Physics2D.gravity * movement.GravityScale * timestep * timestep;

        Vector2 moveStep = timestep * velocity;

        for (int i = 0; i < steps; i++)
        {
            moveStep += gravittyAccel;
            position += moveStep * movement.MoveMultiplier;
            results[i] = position;
        }

        return results;
    }

    // Invoked method: PlayerMovement
    /// <summary>Whether allow swipe or not.</summary>
    /// <param name="value">The value of the parameter.</param>
    public void AllowSwipe(bool value)
    {
        allowSwipe = value;

        if (!allowSwipe)
        {
            lineRenderer.positionCount = 0;
            startTouchPos = Vector2.zero;
        }
    }

    // Invoked method: PushButton (GameObject)
    //                 PullButton (GameObject)
    /// <summary>Sets the swipe technique multiplier (1 = push, -1 = pull).</summary>
    /// <param name="value">The value to store.</param>
    public void SetSwipeTechniqueMultiplier(int value)
    {
        swipeTechniqueMultiplier = value;
        DataManager.SaveData("swipeTechniqueMultiplier", value);
    }

    // Invoked method: Slider (GameObject)
    /// <summary>Sets the swipe sensitivity.</summary>
    /// <param name="value">The value to store.</param>
    public void SetSwipeSensitivity(float value)
    {
        swipeSensitivity = value;
        PlayerPrefs.SetFloat("swipeSensitivity", value);
    }
}
