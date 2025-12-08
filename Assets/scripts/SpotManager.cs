using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class SpotManager : MonoBehaviour
{
    public List<DifferenceSpot> activeSpots = new List<DifferenceSpot>();// 自動抓，不用手動填

    public int totalCount;          // 總共有幾個 spot
    public int foundCount;          // 已經找到幾個
                                    // Start is called before the first frame update
                                    // 自動抓場景中所有 DifferenceSpot（包含沒啟用的，用 true）
    [Header("UI")]
    public TextMeshProUGUI text;//計算數量

    void Awake()
    {
        
    }
    public void RefreshActiveSpots()
    {
        activeSpots.Clear();

        // 抓場景內所有 DifferenceSpot（包含沒啟用的）
        DifferenceSpot[] allSpots = FindObjectsOfType<DifferenceSpot>(true);

        // 順便幫每個 spot 設定 manager
        foreach (var s in allSpots)
        {
            if (s.gameObject.activeInHierarchy)
            {
                activeSpots.Add(s);
                s.manager = this;       // 幫它綁 manager
                s.ResetSpot();         // 還原 found 狀態（避免舊狀態殘留）
            }
        }
        totalCount = activeSpots.Count;
        foundCount = 0;
        if (text != null)
            text.text = $"{foundCount} / {totalCount}";

        Debug.Log($"總共有 {totalCount} 個可以找的地方");
    }

    //被 DifferenceSpot 呼叫：某個 spot 被找到時進來
    public void OnSpotFound(DifferenceSpot spot)
    {
        // 如果這個 spot 不在目前 active 列表，就不算
        if (!activeSpots.Contains(spot))
        {
            Debug.Log($"[SpotManager] 收到一個不在 active 列表內的 spot：{spot.name}");
            return;
        }
        foundCount++;

        Debug.Log($"找到第 {foundCount} 個，進度：{foundCount} / {totalCount}");

        // 這裡之後可以加：
        text.text = foundCount+" / "+totalCount;
        // - 如果 foundCount == totalCount → 顯示「全部找完」畫面
        if (foundCount >= totalCount)
        {
            Debug.Log("全部找完啦！");
            // TODO：彈出完成畫面、下一關按鈕等等
        }
    }

    public void ClearAllCircles()
    {
        CircleFill[] circles = FindObjectsOfType<CircleFill>(true);

        foreach (var c in circles)
        {
            Destroy(c.gameObject);
        }

        Debug.Log("[SpotManager] 已清掉所有標記圈圈");
    }

}
