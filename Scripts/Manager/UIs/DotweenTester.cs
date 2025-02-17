using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;

public class DotweenTester : MonoBehaviour
{
    [SerializeField, Range(0, 6)] private int multi;
    [SerializeField] private RectTransform[] rects;

    void Start()
    {
        StartCoroutine(AnimateOut());
    }

    private IEnumerator AnimateIn()
    {
        yield return new WaitForSeconds(1);

        for (int i = 5 * multi; i < 5 * (multi + 1); i++)
        {
            float startX = rects[i % 5].anchoredPosition.x;
            rects[i % 5].DOAnchorPos(new Vector2(startX, 0), 1).SetEase((Ease)i);
            rects[i % 5].GetChild(0).GetComponent<TMP_Text>().text = ((Ease)i).ToString();

            yield return new WaitForSeconds(2);
        }
    }

    private IEnumerator AnimateOut()
    {
        yield return new WaitForSeconds(1);

        for (int i = 5 * multi; i < 5 * (multi + 1); i++)
        {
            float startX = rects[i % 5].anchoredPosition.x;
            rects[i % 5].DOAnchorPos(new Vector2(startX, 300), 1).SetEase((Ease)i);
            rects[i % 5].GetChild(0).GetComponent<TMP_Text>().text = ((Ease)i).ToString();

            yield return new WaitForSeconds(2);
        }
    }
}
