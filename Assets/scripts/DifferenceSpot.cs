using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DifferenceSpot : MonoBehaviour
{
    [Header("圈圈預置物")]
    public GameObject circlePrefab;      // 指到 CircleMark.prefab
    [Tooltip("圈圈要放在哪個 UI 爸爸下面")]public RectTransform canvasRect;     // 通常拖 Canvas 或 CircleLayer
    bool found = false;
    [Tooltip("計算圈圈數量")]
    public SpotManager manager;          // 由 Manager 幫你塞進來

    public void OnClickSpot()
    {
        if (found) return;
        found = true;

        
        GameObject circle = Instantiate(circlePrefab, canvasRect);// 生成圈圈在點的位置
        // 把圈圈的位置對齊這個 Spot
        var spotRect = GetComponent<RectTransform>();
        var circleRect = circle.GetComponent<RectTransform>();
        circleRect.anchoredPosition = spotRect.anchoredPosition;

        // 播圈圈填滿動畫
        var filler = circle.GetComponent<CircleFill>();
        if (filler != null)
        {
            filler.Play();
        }

        // ? 告訴管理員：我被找到囉
        if (manager != null)
        {
            manager.OnSpotFound(this);
        }
    }

    // 👇 如果之後有要重複使用同一個 spot，可以呼叫這個還原
    public void ResetSpot()
    {
        found = false;
    }
}

