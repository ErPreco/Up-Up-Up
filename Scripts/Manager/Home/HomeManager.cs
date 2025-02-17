using System.Collections;
using UnityEngine;

public class HomeManager : MonoBehaviour
{
    #region Public References
    public float DelayToGameView => delayToGameView;
    #endregion

    [SerializeField] private GameManager gameManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private UIManager UIManager;

    [Space]
    [SerializeField] private Transform player;

    private PlayerController playerController;
    private SwipeListener swipeListener;

    [Space]
    [SerializeField] private float delayToStart;
    private float delayToGameView;
    private bool isHomeAnimEnded;

    void Start()
    {
        swipeListener = player.GetComponent<SwipeListener>();
        playerController = player.GetComponent<PlayerController>();
    }

    // Invoked method: PlayButton (GameObject)
    //                 GameManager
    /// <summary>
    /// Starts the game.
    /// </summary>
    /// <param name="isFromHome">Whether switching the camera to game or not.</param>
    public void PlayGame(bool isFromHome)
    {
        StartCoroutine(WaitToStart());

        IEnumerator WaitToStart()
        {
            delayToGameView = (isFromHome) ? delayToStart : gameManager.LoadPanelTime + delayToStart;

            yield return new WaitUntil(() => isHomeAnimEnded);

            if (isFromHome)
            {
                cameraController.SwitchToGameView();
            }
            UIManager.SwitchToGameView(delayToStart, !isFromHome);

            yield return new WaitForSeconds(delayToGameView);

            gameManager.IsPlaying = true;
            playerController.AllowYLimitUpdate();
            swipeListener.AllowSwipe(true);
        }
    }

    /// <summary>
    /// Indicates that the home animations are ended.
    /// </summary>
    public void SetHomeAnimEnded()
    {
        isHomeAnimEnded = true;
    }
}
