using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Space]
    [SerializeField] private CinemachineVirtualCamera homeCM;
    [SerializeField] private CinemachineVirtualCamera gameCM;
    private CinemachineFramingTransposer gameCMframingTransposer;

    private Vector3 trackedObjectOffset;
    private float yDamping;

    [HideInInspector] public bool useZoomOut;
    [HideInInspector] public Tracker tracker;
    [HideInInspector] public float zoomOutMultiplier;

    private float yOffset;
    private float normalOrthoSize;

    private bool isStopped;
    private float timer = 0.5f;

    void Start()
    {
        homeCM.Priority = 1;
        gameCM.Priority = 0;

        gameCMframingTransposer = gameCM.GetCinemachineComponent<CinemachineFramingTransposer>();
        trackedObjectOffset = gameCMframingTransposer.m_TrackedObjectOffset;
        yDamping = gameCMframingTransposer.m_YDamping;
        yOffset = gameCMframingTransposer.m_TrackedObjectOffset.y;
        normalOrthoSize = gameCM.m_Lens.OrthographicSize;
    }

    void Update()
    {
        if (!useZoomOut) return;

        if (tracker.LookaheadTime > 0.01f)
        {
            ZoomLerp(zoomOutMultiplier, tracker.LookaheadTime);
        }
    }

    void FixedUpdate()
    {
        if (isStopped)
        {
            if (timer < 0)
            {
                gameCM.m_Follow = null;
                return;
            }

            timer -= Time.fixedDeltaTime;

            // Smoothly stops following the player
            gameCMframingTransposer.m_TrackedObjectOffset.y = yOffset * (timer / 0.5f);
            gameCMframingTransposer.m_YDamping = 1 + (1 - (timer / 0.5f)) * 19;
        }
    }

    /// <summary>Zooms in or out applying a linera interpolation to the orthographic size.</summary>
    /// <param name="zoomMultiplier">The multiplier with which the maximum size is calculated.</param>
    /// <param name="time">The value between 0 and 1 by which the orthographic size is interpolated.</param>
    public void ZoomLerp(float zoomMultiplier, float time)
    {
        gameCM.m_Lens.OrthographicSize = Mathf.Lerp(normalOrthoSize, normalOrthoSize * zoomMultiplier, time);
    }

    // Invoked method: PlayerController
    /// <summary>
    /// Stops the camera from following the player.
    /// </summary>
    public void StopFollow()
    {
        if (gameCM.m_Follow != null)
        {
            isStopped = true;
        }
    }

    /// <summary>
    /// Switches the cinemachine priority from home to game view.
    /// </summary>
    public void SwitchToGameView()
    {
        homeCM.Priority = 0;
        gameCM.Priority = 1;
    }

    // Invoked method: GameManager
    /// <summary>
    /// Resets the game view.
    /// </summary>
    public void ResetToPlay()
    {
        timer = 0.5f;
        isStopped = false;

        gameCM.m_Follow = null;
        gameCMframingTransposer.m_TrackedObjectOffset = trackedObjectOffset;
        gameCMframingTransposer.m_YDamping = yDamping;

        homeCM.Priority = 1;
        gameCM.Priority = 0;

        StartCoroutine(WaitToSwitchView());

        IEnumerator WaitToSwitchView()
        {
            // Enusres that the "stop logic" in FixedUpdate does not remove the target to gameCM
            yield return new WaitForSeconds(0.1f);

            gameCM.m_Follow = tracker.transform;

            yield return new WaitForSeconds(gameManager.LoadPanelTime);

            SwitchToGameView();
        }
    }
}
