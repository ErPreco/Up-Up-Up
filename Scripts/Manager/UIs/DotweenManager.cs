using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

public class DotweenManager : MonoBehaviour
{
    private UIManager UIManager;
    [SerializeField] private HomeManager homeManager;

    private bool isCanvasChanged;

    [Header("UI")]
    [SerializeField] private float buttonAnimDuration;
    [SerializeField] private float scrollViewAnimDuration;
    [SerializeField] private float pauseAnimDuration;
    [SerializeField] private float scoreAnimDuration;
    [SerializeField] private float newStickerAnimDuration;

    [Header("Platform")]
    [SerializeField] private float platformDisappearAnimDuration;

    [Space]
    [SerializeField] private DotweenButtons[] buttons;

    [Space]
    [SerializeField] private RectTransform[] scrollViews;

    [Header("Death menu")]
    [SerializeField] private RectTransform deathMenu;
    [SerializeField] private RectTransform newSticker;

    void Start()
    {
        UIManager = GetComponent<UIManager>();

        StartCoroutine(WaitToAnimateHome());

        IEnumerator WaitToAnimateHome()
        {
            // Animates the home only after the canvases are set in UIManager
            yield return new WaitForEndOfFrame();

            ChangeCanvas(0, true, true);
            isCanvasChanged = true;
            SequenceIn(new Func<float>[] { () => ButtonsPopUp(0, UIManager.HomeLoadingDelay) });
        }
    }

    #region UI
    /// <summary>
    /// Creates the "in animation" for the buttons group at the index given.
    /// </summary>
    /// <param name="buttonsGroupIndex">The index of the buttons group.</param>
    /// <param name="delay">The dealy after which to start the animation.</param>
    /// <returns>The entire time used for the animations.</returns>
    private float ButtonsPopUp(int buttonsGroupIndex, float delay = 0)
    {
        foreach (Transform t in buttons[buttonsGroupIndex].Buttons)
        {
            t.localScale = Vector2.one * 0.3f;
            t.GetComponent<CanvasGroup>().alpha = 0;
        }

        StartCoroutine(ButtonsAnimation());

        IEnumerator ButtonsAnimation()
        {
            yield return new WaitForSeconds(delay);

            foreach (Transform button in buttons[buttonsGroupIndex].Buttons)
            {
                button.DOScale(1, buttonAnimDuration).SetEase(Ease.OutBack).SetUpdate(true);
                button.GetComponent<CanvasGroup>().DOFade(1, buttonAnimDuration).SetEase(Ease.OutBack).SetUpdate(true);

                yield return new WaitForSeconds(0.2f);
            }
        }

        int buttonsCount = buttons[buttonsGroupIndex].Buttons.Length;
        return (buttonsCount - 1) * 0.2f + buttonAnimDuration;
    }

    /// <summary>
    /// Creates the "out animation" for the buttons group at the index given.
    /// </summary>
    /// <param name="buttonsGroupIndex">The index of the buttons group.</param>
    /// <returns>The entire time used for the animations.</returns>
    private float ButtonsPopOut(int buttonsGroupIndex)
    {
        StartCoroutine(ButtonsAnimation());

        IEnumerator ButtonsAnimation()
        {
            foreach (Transform button in buttons[buttonsGroupIndex].Buttons)
            {
                button.DOScale(0.3f, buttonAnimDuration).SetEase(Ease.InBack).SetUpdate(true);
                button.GetComponent<CanvasGroup>().DOFade(0, buttonAnimDuration - 0.1f).SetEase(Ease.InBack).SetUpdate(true);

                yield return new WaitForSeconds(0.2f);
            }
        }

        int buttonsCount = buttons[buttonsGroupIndex].Buttons.Length;
        return (buttonsCount - 1) * 0.2f + buttonAnimDuration;
    }

    /// <summary>
    /// Creates the "in animation" for the scroll view at the index given.
    /// </summary>
    /// <param name="index">The index of the scroll view.</param>
    /// <returns>The entire time used for the animations.</returns>
    private float ScrollViewBounceIn(int index)
    {
        scrollViews[index].anchoredPosition = new Vector2(0, 2000);
        scrollViews[index].GetComponent<CanvasGroup>().alpha = 0;

        scrollViews[index].DOAnchorPos(new Vector2(0, 70), scrollViewAnimDuration).SetEase(Ease.InOutBack).SetUpdate(true);
        scrollViews[index].GetComponent<CanvasGroup>().DOFade(1, scrollViewAnimDuration + 0.5f).SetUpdate(true);

        return scrollViewAnimDuration;
    }

    /// <summary>
    /// Creates the "out animation" for the scroll view at the index given.
    /// </summary>
    /// <param name="index">The index of the scroll view.</param>
    /// <returns>The entire time used for the animations.</returns>
    private float ScrollViewBounceOut(int index)
    {
        scrollViews[index].DOAnchorPos(new Vector2(0, 2000), scrollViewAnimDuration).SetEase(Ease.InOutBack).SetUpdate(true);
        scrollViews[index].GetComponent<CanvasGroup>().DOFade(0, scrollViewAnimDuration - 0.1f).SetUpdate(true);

        return scrollViewAnimDuration;
    }

    /// <summary>
    /// Activates or deactivates the UI object by using its index.
    /// </summary>
    /// <param name="objectIndex">The index of the object.</param>
    /// <param name="isCanvas">Whether the object is a canvas or not.</param>
    /// <param name="isActive">Whether the object must be activated or not.</param>
    /// <param name="isToGame">Whether the method is called for deactivating the home canvas and activating the game view.</param>
    /// <returns>False if the UI object is the quit canvas so that we can continue to use SequenceIn, true otherwise.</returns>
    private bool ChangeCanvas(int objectIndex, bool isCanvas, bool isActive, bool isToGame = false)
    {
        if (isActive)
        {
            UIManager.ActivateObject(objectIndex, isCanvas);
            return true;
        }
        else
        {
            UIManager.DeactivateObject(objectIndex, isCanvas);
            if (isToGame)
            {
                homeManager.SetHomeAnimEnded();
            }
            return isToGame;
        }
    }

    /// <summary>
    /// Deactivates and activates UI objects by using thier index.
    /// </summary>
    /// <param name="deactiveObjectIndex">The index of the object that will be deactivated.</param>
    /// <param name="isDeactiveCanvas">Whether the object that will be deactivated is a canvas or not.</param>
    /// <param name="activeCanvasIndex">The index of the object that will be activated.</param>
    /// <param name="isActiveCanvas">Whether the object that will be activated is a canvas or not.</param>
    /// <returns>True because the view has been switched.</returns>
    private bool ChangeCanvas(int deactiveObjectIndex, bool isDeactiveCanvas, int activeCanvasIndex, bool isActiveCanvas)
    {
        UIManager.DeactivateObject(deactiveObjectIndex, isDeactiveCanvas);
        UIManager.ActivateObject(activeCanvasIndex, isActiveCanvas);

        return true;
    }

    /// <summary>
    /// Puts the "in animations" in order and performs them in sequence.
    /// </summary>
    /// <param name="methods">The methods used for the animations.</param>
    private void SequenceIn(Func<float>[] methods)
    {
        StartCoroutine(Sequence());

        IEnumerator Sequence()
        {
            // Waits until the canvas is activated
            yield return new WaitUntil(() => isCanvasChanged);

            isCanvasChanged = false;

            foreach (Func<float> method in methods)
            {
                float waitingTime = method();

                yield return new WaitForSecondsRealtime(waitingTime);
            }
        }
    }

    /// <summary>
    /// Puts the "out animations" in order and performs them in sequence.
    /// </summary>
    /// <param name="methods">The methods used for the animations.</param>
    /// <param name="changeCanvas">The method for changing the canvas.</param>
    private void SequenceOut(Func<float>[] methods, Func<bool> changeCanvas)
    {
        StartCoroutine(Sequence());

        IEnumerator Sequence()
        {
            for (int i = 0; i < methods.Length; i++)
            {
                float waitingTime = methods[i]();

                yield return new WaitForSecondsRealtime(waitingTime);
                
                if (i == methods.Length - 1)
                {
                    // Changes the canvas after the last animation
                    isCanvasChanged = changeCanvas();
                }
            }
        }
    }

    // Invoked method: HomeCanvas/LegendButton (GameObject)
    /// <summary>
    /// Creates the animations for the transition to the legend canvas.
    /// </summary>
    public void GoToLegend()
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(0) }, () => ChangeCanvas(0, true, 2, true));
        SequenceIn(new Func<float>[] { () => ButtonsPopUp(1) });
    }

    // Invoked method: LegendCanvas/BackButton (GameObject)
    /// <summary>
    /// Creates the animations for the transition to the home canvas.
    /// </summary>
    public void GoToHomeFromLegend()
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(1) }, () => ChangeCanvas(2, true, 0, true));
        SequenceIn(new Func<float>[] { () => ButtonsPopUp(0) });
    }

    // Invoked method: LegendCanvas/PlatformsButton (GameObject)
    /// <summary>
    /// Creates the animations for the transition to the platforms menu.
    /// </summary>
    public void GoToPlatforms()
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(1) }, () => ChangeCanvas(0, false, 1, false));
        SequenceIn(new Func<float>[] { () => ScrollViewBounceIn(0), () => ButtonsPopUp(2) });
    }

    // Invoked method: LegendCanvas/HelpersButton (GameObject)
    /// <summary>
    /// Creates the animations for the transition to the helpers menu.
    /// </summary>
    public void GoToHelpers()
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(1) }, () => ChangeCanvas(0, false, 2, false));
        SequenceIn(new Func<float>[] { () => ScrollViewBounceIn(1), () => ButtonsPopUp(3) });
    }

    // Invoked method: LegendCanvas/ObstaclesButton (GameObject)
    /// <summary>
    /// Creates the animations for the transition to the obstacles menu.
    /// </summary>
    public void GoToObstacles()
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(1) }, () => ChangeCanvas(0, false, 3, false));
        SequenceIn(new Func<float>[] { () => ScrollViewBounceIn(2), () => ButtonsPopUp(4) });
    }

    // Invoked method: LegendCanvas/PlatformsPanel/BackButton (GameObject)
    //                 LegendCanvas/HelpersPanel/BackButton (GameObject)
    //                 LegendCanvas/ObstaclesPanel/BackButton (GameObject)
    /// <summary>
    /// Creates the animations for the transition to the legend intro menu.
    /// </summary>
    /// <param name="fromIndex">the index of the menu from which to start the transition.</param>
    public void GoToLegendIntro(int fromIndex)
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(fromIndex + 2), () => ScrollViewBounceOut(fromIndex) }, () => ChangeCanvas(fromIndex + 1, false, 0, false));
        SequenceIn(new Func<float>[] { () => ButtonsPopUp(1) });
    }

    // Invoked method: HomeCanvas/ShopButton (GameObject)
    /// <summary>
    /// Creates the animations for the transition to the shop canvas.
    /// </summary>
    public void GoToShop()
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(0) }, () => ChangeCanvas(0, true, 1, true));
        SequenceIn(new Func<float>[] { () => ButtonsPopUp(5) });
    }

    // Invoked method: ShopCanvas/BackButton (GameObject)
    /// <summary>
    /// Creates the animations for the transition to the home canvas.
    /// </summary>
    public void GoToHomeFromShop()
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(5) }, () => ChangeCanvas(1, true, 0, true));
        SequenceIn(new Func<float>[] { () => ButtonsPopUp(0) });
    }

    // Invoked method: HomeCanvas/PlayButton (GameObject)
    //                 GameManager
    /// <summary>
    /// Creates the animations for the transition to the game view.
    /// </summary>
    /// <param name="isFromHome">Whether the method is called from the home canvas or not.</param>
    public void GoToGame(bool isFromHome)
    {
        if (isFromHome)
        {
            SequenceOut(new Func<float>[] { () => ButtonsPopOut(0) }, () => ChangeCanvas(0, true, false, true));
        }
        else
        {
            isCanvasChanged = true;
        }
        SequenceIn(new Func<float>[] { () => ButtonsPopUp(6, homeManager.DelayToGameView) });
    }

    // Invoked method: PauseButton (GameObject)
    /// <summary>
    /// Creates the animations for the transitions to the pause canvas.
    /// </summary>
    public void GoToPause()
    {
        foreach (Transform t in buttons[7].Buttons)
        {
            t.localScale = Vector2.one * 0.6f;
            t.GetComponent<CanvasGroup>().alpha = 0;
        }

        foreach (Transform t in buttons[7].Buttons)
        {
            t.DOScale(1, pauseAnimDuration).SetEase(Ease.OutBack).SetUpdate(true);
            t.GetComponent<CanvasGroup>().DOFade(1, pauseAnimDuration).SetUpdate(true);
        }
    }

    /// <summary>
    /// Creates the animations for the transitions to the death menu canvas.
    /// </summary>
    /// <param name="score">The end value of the score.</param>
    public void GoToDeathMenu(int score, int coins)
    {
        deathMenu.anchoredPosition = new Vector2(0, 2000);
        deathMenu.GetComponent<CanvasGroup>().alpha = 0;

        deathMenu.DOAnchorPos(Vector2.zero, scrollViewAnimDuration).SetEase(Ease.InOutBack);
        deathMenu.GetComponent<CanvasGroup>().DOFade(1, scrollViewAnimDuration + 0.5f).OnComplete(() =>
        {
            DOVirtual.Int(0, score, scoreAnimDuration, v => { UIManager.SetDeathScore(v); }).SetEase(Ease.OutCirc).OnComplete(() =>
            {
                DOVirtual.Int(0, coins, scoreAnimDuration, v => { UIManager.SetDeathCoins(v); }).SetEase(Ease.OutSine);
            });
        });

        newSticker.localScale = Vector2.one * 1.5f;
        newSticker.GetComponent<CanvasGroup>().alpha = 0;
    }

    /// <summary>
    /// Creates the animation for the sticker of the new record.
    /// </summary>
    public void AnimateNewSticker()
    {
        newSticker.DOScale(1, newStickerAnimDuration).SetEase(Ease.OutExpo);
        newSticker.GetComponent<CanvasGroup>().DOFade(1, newStickerAnimDuration);
    }

    // Invoked method: HomeCanvas/QuitButton (GameObject)
    /// <summary>
    /// Creates the animations for the transitions to the quit canvas.
    /// </summary>
    public void GoToQuit()
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(0) }, () => ChangeCanvas(0, true, 7, true));
        SequenceIn(new Func<float>[] { () => ButtonsPopUp(8) });
    }

    // Invoked method: QuitCanvas/CancelButton (GameObject)
    //                 QuitCanvas/ExitPanel (GameObject)
    /// <summary>
    /// Creates the animations to close the quit canvas.
    /// </summary>
    public void CloseQuit()
    {
        SequenceOut(new Func<float>[] { () => ButtonsPopOut(8) }, () => ChangeCanvas(7, true, 0, true));
        SequenceIn(new Func<float>[] { () => ButtonsPopUp(0) });
    }
    #endregion

    #region Platforms
    /// <summary>
    /// Creates the animation to make the platform disappear.
    /// </summary>
    /// <param name="module">The trasform of the module of the platform.</param>
    /// <param name="sr">The SpriteRenderer component of the platform.</param>
    public void PlatformDisappear(Transform module, SpriteRenderer sr)
    {
        module.DOScale(0.5f, platformDisappearAnimDuration).SetEase(Ease.InBack);
        sr.DOFade(0, platformDisappearAnimDuration - 0.1f).SetEase(Ease.InBack);
    }
    #endregion

    [Serializable]
    public class DotweenButtons
    {
        public Transform[] Buttons => buttons;

        [SerializeField] private string name;
        [SerializeField] private Transform[] buttons;
    }
}
