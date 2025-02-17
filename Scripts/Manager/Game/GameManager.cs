using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    #region Public References
    public float HeightMultiplier => heightMultiplier;

    public float PlatformLimitX => platformLimit.position.x - 5;
    public float WorldLimitX => platformLimit.position.x;

    public Transform Ground => ground.transform;

    public float LoadPanelTime => loadPanelTime;

    public bool IsPlaying { get { return isPlaying; } set { isPlaying = value; } }

    public int Height => height;
    public int MaxHeight => maxHeight;
    public int PrevMaxHeight => prevMaxHeight;
    #endregion

    private bool isPaused;
    private bool isPlaying;

    [Header("Player")]
    [SerializeField] private Transform player;
    private PlayerController playerController;
    [SerializeField] private float heightMultiplier = 3;
    private int height;
    private int maxHeight;
    private int prevMaxHeight;

    [Header("Platform limit")]
    [SerializeField] private Transform platformLimit;
    [SerializeField] private GameObject platformLimitPrefab;
    private readonly Transform[] platformLimitLines = new Transform[10];

    [Header("World limit")]
    [SerializeField] private Transform worldLimit;
    [SerializeField] private GameObject worldLimitPrefab;
    [SerializeField] [Range(2, 4)] private float motionSpeed;
    private readonly Transform[] worldLimits = new Transform[16];

    [Space]
    [SerializeField] private Transform maxHeightSign;
    private bool isMaxHeightSignSet;
    [SerializeField] private GameObject ground;

    [Space]
    [SerializeField] private float loadPanelTime;

    [Space]
    public UnityEvent OnGameRestart;

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();

        InstantiatePlatformLimits();

        prevMaxHeight = maxHeight = DataManager.GetData<int>("maxHeight");
        maxHeightSign.position = Vector3.down * 20;
    }

    void Update()
    {
        // if (isGameOver) return;

        if (!isMaxHeightSignSet && player.position.y > (prevMaxHeight / heightMultiplier) - 40)
        {
            SetMaxHeightSign();
            isMaxHeightSignSet = true;
        }

        if (ground.activeSelf && playerController.YLimit > 0)
        {
            ground.SetActive(false);
        }

        height = Mathf.RoundToInt(playerController.Height * heightMultiplier);
        if (height > maxHeight)
        {
            maxHeight = height;
        }

        ProceduralPlatformLimitLine();
    }

    /// <summary>
    /// Instantiates the first dashed lines that indicate the platform limit.
    /// </summary>
    private void InstantiatePlatformLimits()
    {
        for (int i = 0; i < platformLimitLines.Length / 2; i++)
        {
            // Left clones
            platformLimitLines[i] = Instantiate(platformLimitPrefab, platformLimit).transform;
            platformLimitLines[i].position = new Vector3(-platformLimit.position.x, 20 * i);

            // Right clones
            platformLimitLines[i + platformLimitLines.Length / 2] = Instantiate(platformLimitPrefab, platformLimit).transform;
            platformLimitLines[i + platformLimitLines.Length / 2].position = new Vector3(platformLimit.position.x, 20 * i);
        }
    }

    /// <summary>
    /// Sets the signs indicating the heighest score.
    /// </summary>
    private void SetMaxHeightSign()
    {
        if (prevMaxHeight == 0) return;

        maxHeightSign.position = (ground.transform.position.y + maxHeight / heightMultiplier) * Vector3.up;
        for (int i = 0; i < maxHeightSign.childCount; i += 2)
        {
            maxHeightSign.GetChild(i).GetChild(1).GetComponent<TMPro.TMP_Text>().text = maxHeight.ToString() + " m";
        }
    }

    // Invoked method: PlayerController
    //                 PauseCanvas/RestartButton (GameObject)
    //                 PauseCanvas/HomeButton (GameObject)
    /// <summary>
    /// Saves the maximum height that the player reached.
    /// </summary>
    public void SaveMaxHeight()
    {
        DataManager.SaveData("maxHeight", maxHeight);
        prevMaxHeight = maxHeight;
    }

    /// <summary>
    /// Generates procedural dashed line to indicate the platform limit.
    /// </summary>
    private void ProceduralPlatformLimitLine()
    {
        int roundedPlayerHeight = Mathf.RoundToInt(player.position.y);
        if (roundedPlayerHeight % 20 == 0 && roundedPlayerHeight >= 60)
        {
            foreach (Transform clone in platformLimitLines)
            {
                if (clone.position.y == roundedPlayerHeight - 60)
                {
                    clone.position += new Vector3(0, 20 * (platformLimitLines.Length / 2));
                }
                else if (clone.position.y == roundedPlayerHeight + 60)
                {
                    clone.position -= new Vector3(0, 20 * (platformLimitLines.Length / 2));
                }
            }
        }
    }

    // Invoked method: ExitPanel (GameObject)
    //                 PauseButton (GameObject)
    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseGame()
    {
        isPaused = !isPaused;

        Time.timeScale = (isPaused) ? 0 : 1;
    }

    // Invoked method: PauseCanvas/RestartButton (GameObject)
    //                 DeathCanvas/RestartButton (GameObject)
    /// <summary>
    /// Resets the game for another play.
    /// </summary>
    public void RestartGame()
    {
        isPaused = false;
        isPlaying = false;
        Time.timeScale = 1;

        OnGameRestart?.Invoke();

        ground.SetActive(true);

        Array.ForEach(platformLimitLines, t => Destroy(t.gameObject));
        InstantiatePlatformLimits();
        isMaxHeightSignSet = false;
    }

    // Invoked method: PauseCanvas/HomeButton (GameObject)
    //                 DeathCanvas/HomeButton (GameObject)
    /// <summary>
    /// Reloads the main scene after a certain amount of time.
    /// </summary>
    public void ReloadGame()
    {
        StartCoroutine(WaitToBackHome());

        IEnumerator WaitToBackHome()
        {
            yield return new WaitForSecondsRealtime(LoadPanelTime);

            Time.timeScale = 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    // Invoked method: QuitCanvas/QuitButton (GameObject)
    /// <summary>
    /// Quits the game.
    /// </summary>
    public void Quit()
    {
        SaveMaxHeight();

        Application.Quit();
    }
}