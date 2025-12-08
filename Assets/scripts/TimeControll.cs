using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeControll : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;  // 選填，有就顯示，沒有就不顯示
    private Coroutine countdownRoutine;
    
    [Header("時間")]
    public bool isRunning = false;
    public int currentTime = 0;

    private void Start()
    {
        timerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 由外部呼叫：開始倒數
    /// </summary>
    public void StartCountdown(int seconds)
    {
        // 停止上一個倒數（保險）
        ForceEnd();

        countdownRoutine = StartCoroutine(Countdown(seconds));
    }
    // 對外呼叫：強制結束倒數（提前終止）
    public void ForceEnd()
    {
        if (!isRunning) return;

        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        ResetTimer();
    }

    /// <summary>
    /// 真正跑倒數的協程
    /// </summary>
    private IEnumerator Countdown(int totalSeconds)
    {
        isRunning = true;
        currentTime = totalSeconds;

        while (currentTime > 0)
        {
            // 更新 UI（如果有）
            if (timerText != null)
                timerText.text = currentTime.ToString();

            yield return new WaitForSeconds(1f);

            currentTime--;
        }

        // 最後顯示 0
        if (timerText != null)
            timerText.text = "0";

        // TODO：倒數結束後做什麼 → 開事件也好、觸發門開啟也行
        OnCountdownEnd();
        ResetTimer();
    }
    // 將狀態完全重置為「未開始」的模式
    private void ResetTimer()
    {
        isRunning = false;
        currentTime = 0;

        if (timerText != null)
            timerText.text = "00 : 00"; // 重置 UI （你可改成 "--"）

        countdownRoutine = null;
    }

    /// <summary>
    /// 倒數結束事件（外部可綁 delegate、或覆寫）
    /// </summary>
    protected virtual void OnCountdownEnd()
    {
        Debug.Log("倒數結束！");
    }
}
