using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeavenTextReveal : MonoBehaviour
{
    [Header("文字本體")]
    public TextMeshProUGUI textUI;

    [Header("顯示動畫設定")]
    public float revealDuration = 2f;  // 遮罩從 0 拉滿要多久
    public float padding = 20f;        // 文字左右留一點空間

    RectTransform maskRect;

    void Awake()
    {
        maskRect = GetComponent<RectTransform>();

        if (textUI == null)
        {
            textUI = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    /// <summary>
    /// 呼叫這個來顯示一段文字，並播放遮罩滑開動畫
    /// </summary>
    public void ShowLine(string content)
    {
        if (textUI == null) return;

        // 1. 先把字全部塞進去（一次出現）
        textUI.text = content;

        // 2. 強制更新 Layout，讓 preferredWidth 正確
        LayoutRebuilder.ForceRebuildLayoutImmediate(textUI.rectTransform);

        // 3. 算目標寬度（字多就寬一些）
        float targetWidth = textUI.preferredWidth + padding;

        // 4. 先把 Mask 的寬度設成 0
        maskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);

        // 5. 開始協程，慢慢拉開
        StopAllCoroutines();
        StartCoroutine(RevealMaskRoutine(targetWidth));
    }

    IEnumerator RevealMaskRoutine(float targetWidth)
    {
        float t = 0f;
        float startWidth = 0f;

        while (t < revealDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / revealDuration);
            float currentWidth = Mathf.Lerp(startWidth, targetWidth, lerp);

            maskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);

            yield return null;
        }

        maskRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
    }
}
