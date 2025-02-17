using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public enum Rarity
{
    Low,
    Medium,
    High
}

public class UIManager : MonoBehaviour
{
    #region Public References
    public float HomeLoadingDelay => homeLoadingDelay;
    #endregion

    [SerializeField] private GameManager gameManager;
    [SerializeField] private ChunksManager chunksManager;
    private DotweenManager dotweenManager;

    [Header("Canvas")]
    [SerializeField] private GameObject homeCanvas;
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private GameObject legendCanvas;
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject deathCanvas;
    [SerializeField] private GameObject loadCanvas;
    [SerializeField] private GameObject quitCanvas;
    private GameObject[] canvases;

    [Space]
    [SerializeField] private float homeLoadingDelay;

    [Header("Player")]
    [SerializeField] private Transform player;
    private PlayerController playerController;
    private SwipeListener swipeListener;
    private int score;
    private bool isNewRecord;

    [Header("Legend")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject platformsPanel;
    [SerializeField] private GameObject helpersPanel;
    [SerializeField] private GameObject obstaclesPanel;
    private GameObject[] panels;

    [Space]
    [SerializeField] private Transform platformsContent;
    [SerializeField] private Transform helpersContent;
    [SerializeField] private Transform obstaclesContent;

    [Header("Game")]
    [SerializeField] private TMP_Text gameHeight;
    [SerializeField] private TMP_Text gameMaxHeight;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider swipeSensitivitySlider;
    private bool isFirstPaused;     // Is the first time the player paused the game?

    [Header("Death")]
    [SerializeField] private float showDeathMenuDelay;
    [SerializeField] private TMP_Text deathScore;
    [SerializeField] private TMP_Text deathCoins;

    void Start()
    {
        dotweenManager = GetComponent<DotweenManager>();
        playerController = player.GetComponent<PlayerController>();
        swipeListener = player.GetComponent<SwipeListener>();

        canvases = new GameObject[]
        {
            homeCanvas,
            shopCanvas,
            legendCanvas,
            gameCanvas,
            pauseCanvas,
            deathCanvas,
            loadCanvas,
            quitCanvas
        };
        Array.ForEach(canvases, c => c.SetActive(false));

        panels = new GameObject[]
        {
            introPanel,
            platformsPanel,
            helpersPanel,
            obstaclesPanel
        };
        Array.ForEach(panels, p => p.SetActive(false));
        panels[0].SetActive(true);

        float swipeSensitivity = DataManager.GetData<float>("swipeSensitivity");
        swipeSensitivitySlider.value = (swipeSensitivity == 0) ? 1 : swipeSensitivity;

        pausePanel.SetActive(false);
        isFirstPaused = true;

        InitLegend();
    }

    void Update()
    {
        UpdateHeight();
    }

    /// <summary>
    /// Initializes the slot of platforms, helpers and obstacles.
    /// </summary>
    private void InitLegend()
    {
        // Platforms
        int[] startHeights = chunksManager.GetPlatformsStartHeights();
        for (int i = 0; i < startHeights.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Transform slot = platformsContent.GetChild(platformsContent.childCount - 1 - j - 3 * i - i);
                slot.GetChild(0).GetChild(0).GetComponent<Image>().sprite = chunksManager.PlatformsSO[j + 3 * i].Icon;
                slot.GetChild(1).GetComponent<TMP_Text>().text = chunksManager.PlatformsSO[j + 3 * i].Caption;
            }
        }

        for (int i = 1; i < startHeights.Length; i++)
        {
            int altitudeSignIndex = platformsContent.childCount - (4 * i);
            platformsContent.GetChild(altitudeSignIndex).GetChild(0).GetComponent<TMP_Text>().text = "- - - - -  " + startHeights[i] + " m  - - - - -";
        }

        // Helpers
        for (int i = helpersContent.childCount - 1; i >= 0; i--)
        {
            PlatformSO so = chunksManager.HelpersSO[(i - (helpersContent.childCount - 1)) * -1];
            helpersContent.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().sprite = so.Icon;
            helpersContent.GetChild(i).GetChild(1).GetComponent<TMP_Text>().text = "Altitude:  " + so.StartHeight.ToString() + " m";
            helpersContent.GetChild(i).GetChild(2).GetComponent<TMP_Text>().text = "Rarity:  " + so.Rarity.ToString().ToLower();
            helpersContent.GetChild(i).GetChild(3).GetComponent<TMP_Text>().text = so.Caption;
        }

        // Obstaclces
        for (int i = obstaclesContent.childCount - 1; i >= 0; i--)
        {
            ObstacleSO so = chunksManager.ObstaclesSO[(i - (obstaclesContent.childCount - 1)) * -1];
            obstaclesContent.GetChild(i).GetChild(0).GetChild(0).GetComponent<Image>().sprite = so.Icon;
            obstaclesContent.GetChild(i).GetChild(1).GetComponent<TMP_Text>().text = "Altitude:  " + so.StartHeight.ToString() + " m";
            obstaclesContent.GetChild(i).GetChild(2).GetComponent<TMP_Text>().text = "Rarity:  " + so.Rarity.ToString().ToLower();
            obstaclesContent.GetChild(i).GetChild(3).GetComponent<TMP_Text>().text = so.Caption;
        }
    }

    /// <summary>
    /// Updates the score and the maximum score if necessary.
    /// </summary>
    private void UpdateHeight()
    {
        int height = gameManager.Height;
        if (height < 0)
        {
            height = 0;
        }
        if (height > score)
        {
            score = height;
        }
        
        gameHeight.text = height.ToString() + " m";
        gameMaxHeight.text = gameManager.MaxHeight.ToString() + " m";
    }

    // Invoked method: PauseCanvas/PauseButton (GameObject)
    //                 PauseCanvas/ExitPanel (GameObject)
    /// <summary>
    /// Performs the logic about pausing the game.
    /// </summary>
    public void OnPauseButtonClicked()
    {
        if (isFirstPaused)
        {
            isFirstPaused = false;

            int buttonIndex = (PlayerPrefs.GetInt("swipeTechniqueMultiplier") == -1) ? 2 : 1;
            Button swipeTechniqueButton = pausePanel.transform.GetChild(1).GetChild(buttonIndex).GetComponent<Button>();
            swipeTechniqueButton.Select();
            swipeTechniqueButton.interactable = false;
        }

        pausePanel.SetActive(!pausePanel.activeSelf);

        swipeListener.AllowSwipe(!pausePanel.activeSelf);
    }

    /// <summary>
    /// Switches from home view to game view.
    /// </summary>
    /// <param name="delay">The delay to active the game view.</param>
    /// <param name="activeLoadCanvas">Whether active the load canvas or not.</param>
    public void SwitchToGameView(float delay, bool activeLoadCanvas)
    {
        gameCanvas.SetActive(false);
        pauseCanvas.SetActive(false);
        deathCanvas.SetActive(false);

        pausePanel.SetActive(false);
        loadCanvas.SetActive(activeLoadCanvas);

        StartCoroutine(WaitToActiveGameView());

        IEnumerator WaitToActiveGameView()
        {
            if (activeLoadCanvas)
            {
                yield return new WaitForSeconds(gameManager.LoadPanelTime);

                loadCanvas.SetActive(false);
            }

            yield return new WaitForSeconds(delay);

            gameCanvas.SetActive(true);
            pauseCanvas.SetActive(true);

            score = 0;
            isNewRecord = false;
        }
    }

    // Invoked method: PlayerController
    /// <summary>
    /// Shows the death menu.
    /// </summary>
    public void ShowDeathMenu()
    {
        StartCoroutine(WaitToShowDeathMenu());

        IEnumerator WaitToShowDeathMenu()
        {
            yield return new WaitForSeconds(showDeathMenuDelay);

            gameCanvas.SetActive(false);
            pauseCanvas.SetActive(false);
            deathCanvas.SetActive(true);

            deathScore.text = "0 m";
            deathCoins.text = "0";
            dotweenManager.GoToDeathMenu(score, playerController.Coins);
        }
    }

    /// <summary>
    /// Sets the score text on the death menu.
    /// </summary>
    /// <param name="value">The value of the score.</param>
    public void SetDeathScore(int value)
    {
        deathScore.text = value.ToString() + " m";

        if (value >= gameManager.PrevMaxHeight && !isNewRecord)
        {
            dotweenManager.AnimateNewSticker();
            isNewRecord = true;
        }
    }

    /// <summary>
    /// Sets the coins text on the death menu.
    /// </summary>
    /// <param name="value">The value of the score.</param>
    public void SetDeathCoins(int value)
    {
        deathCoins.text = value.ToString();
    }

    // Invoked method: PlayerController
    /// <summary>
    /// Hides thee pause panel.
    /// </summary>
    public void HidePausePanel()
    {
        pauseCanvas.SetActive(false);
    }

    // Invoked method: PauseCanvas/HomeButton (GameObject)
    //                 DeathCanvas/HomeButton (GameObject)
    /// <summary>
    /// Reloads the scene and backs home.
    /// </summary>
    public void BackHome()
    {
        gameCanvas.SetActive(false);
        pauseCanvas.SetActive(false);
        deathCanvas.SetActive(false);
        loadCanvas.SetActive(true);
    }

    /// <summary>
    /// Activates the UI object by using its index.
    /// </summary>
    /// <param name="index">The index of the object.</param>
    /// <param name="isCanvas">Whether the object is a canvas or not.</param>
    public void ActivateObject(int index, bool isCanvas)
    {
        if (isCanvas)
        {
            canvases[index].SetActive(true);
        }
        else
        {
            panels[index].SetActive(true);
        }
    }

    /// <summary>
    /// Deactivates the UI object by using its index.
    /// </summary>
    /// <param name="index">The index of the object.</param>
    /// <param name="isCanvas">Whether the object is a canvas or not.</param>
    public void DeactivateObject(int index, bool isCanvas)
    {
        if (isCanvas)
        {
            canvases[index].SetActive(false);
        }
        else
        {
            panels[index].SetActive(false);
        }
    }
}
