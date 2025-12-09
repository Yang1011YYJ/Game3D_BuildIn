using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueSystemDes : MonoBehaviour
{
    [Header("UI")]
    public GameObject TextPanel;
    public TextMeshProUGUI DiaText;
    public Image FaceImage;
    public TextMeshProUGUI Name;
    public RectTransform dialogueBoxRect;
    RectTransform textRT;

    [Header("對話框寬度")]
    public float minWidth = 400f;
    public float maxWidth = 900f;
    [Tooltip("從 Scene 讀出來的 padding")] public float leftPadding;
    [Tooltip("從 Scene 讀出來的 padding")] public float rightPadding;

    [Header("文本")]
    public TextAsset TextfileCurrent;
    [Tooltip("剩三分鐘進站")]public TextAsset Textfile01;

    [Header("劇情進度")]
    [Tooltip("車進站的時間")]public bool text01Finished = false;

    [Header("打字設定")]
    [Tooltip("讀到第幾行")]
    public int index = 0;
    [Tooltip("控制打字節奏（字元出現的間隔時間）")]public float TextSpeed = 0.06f;
    [Tooltip("繼續對話")] public bool KeepTalk;
    [Tooltip("對話中")] public bool IsTalking;

    [Header("自動播放設定")]
    [Tooltip("true 就自動下一行")] public bool autoNextLine = false;
    [Tooltip("每行播完後停多久再自動下一行")] public float autoNextDelay = 0.5f;

    [Header("控制設定")]
    [Tooltip("物件啟用時是否自動開始播放對話")]public bool playOnEnable = false;
    // 內部狀態
    private List<string> TextList = new List<string>();
    [Tooltip("標記是否正在打字")]public bool isTyping = false;
    private Coroutine typingRoutine;

    void Awake()
    {
        textRT = DiaText.rectTransform;

        // 讀 Scene 原本排好的距離
        leftPadding = textRT.offsetMin.x;      // 左邊到父物件的距離
        rightPadding = -textRT.offsetMax.x;     // 右邊是負的，所以要取負號
    }

    void Start()
    {
        TextPanel.SetActive(false);
    }

    void Update()
    {
        // 對話框沒開就不用理會
        if (TextPanel == null || !TextPanel.activeSelf) return;

        if (autoNextLine) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // 正在打字 → 直接補完這一行
                FinishCurrentLineImmediately();
                return;
            }
            // 這一行已經打完 → 換下一行或結束
            index++;
            if (index >= TextList.Count)
            {
                // 所有行數都播完 → 統一交給收尾函式
                HandleDialogueEnd();

            } 
            else
            {
                SetTextUI();
            }
        }
    }

    // 從 TextAsset 讀進所有行
    void GetTextFromFile(TextAsset file)
    {
        TextList.Clear();
        index = 0;

        if (file == null) return;

        var lineData = file.text.Split('\n');

        foreach (var line in lineData)
        {
            // 去掉尾巴的 \r，避免 Windows 換行造成奇怪字元
            TextList.Add(line.TrimEnd('\r'));
        }
    }
    // 從外部開始對話（可以指定要播哪個 TextAsset）
    public void StartDialogue(TextAsset textAsset)
    {
        playOnEnable = true;
        if (textAsset != null)
        {
            TextfileCurrent = textAsset;
            GetTextFromFile(TextfileCurrent);
        }

        if (TextList.Count == 0)
        {
            Debug.LogWarning("[DialogueSystemGame00] 目前 TextList 是空的，沒有東西可以播放。");
            return;
        }

        index = 0;
        TextPanel.SetActive(true);
        SetTextUI();
    }

    /// <summary>
    /// 顯示 index 對應的那一行，啟動打字機效果
    /// </summary>
    void SetTextUI()
    {
        if (index < 0 || index >= TextList.Count) return;

        string line = TextList[index];

        // 先依照這一行內容調整對話框的寬度
        UpdateDialogueBoxWidth(line);

        // 如果之前有打字中的協程，先停掉
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        // 開始打字機，這次記得把協程存起來
        typingRoutine = StartCoroutine(TypeLine(line));
    }
    //打字機：一個字一個蹦出來
    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        DiaText.text = "";

        foreach (char c in line)
        {
            DiaText.text += c;
            yield return new WaitForSeconds(TextSpeed);
        }

        isTyping = false;
        typingRoutine = null;

        if (autoNextLine)
        {
            // 已經是最後一行了
            if (index >= TextList.Count-1)
            {
                // 如果這份對話是 Textfile01，可以在這裡做結束處理
                if (TextfileCurrent == Textfile01)
                {
                    TextPanel.SetActive(false);
                    text01Finished = true;
                    index = 0;
                }
                yield break;
            }

            // 還有下一行 → 等一小段時間再播下一句
            yield return new WaitForSeconds(autoNextDelay);
            index++;
            SetTextUI();
        }
        else
        {
            // 手動模式：停在這裡，等玩家按空白
            typingRoutine = null;
        }
    }

    /// <summary>
    /// 正在打字時按 Space：立刻把這一行顯示完整
    /// </summary>
    void FinishCurrentLineImmediately()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (index < 0 || index >= TextList.Count) return;

        DiaText.text = TextList[index];
        isTyping = false;
    }

    // 播完所有對話時要做什麼：統一收尾都來這裡
    void HandleDialogueEnd()
    {
        // 收狀態
        isTyping = false;
        autoNextLine = false;

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        // 這裡可以根據「是哪一份文本」決定不同行為
        if (TextfileCurrent == Textfile01)
        {
            text01Finished = true;
        }

        // 預設行為：關閉對話框、重置 index
        TextPanel.SetActive(false);
        index = 0;
    }








    /// 依照目前這一行的文字長度調整對話框寬度
    /// （記得對話框背景圖請用 Sliced Sprite 才不會變形）
    void UpdateDialogueBoxWidth(string line)
    {
        if (dialogueBoxRect == null || DiaText == null) return;

        // 內文字區域能用的最大寬度（對話框最大寬度扣掉左右 padding）
        float innerMaxWidth = maxWidth - leftPadding - rightPadding;

        // 用 TMP 算這一行理論上需要的寬度（不限制高度，寬度給一個上限）
        // 這裡給 innerMaxWidth，是在問：「如果我最多給你這麼寬，你會排多少」
        Vector2 pref = DiaText.GetPreferredValues(line, innerMaxWidth, Mathf.Infinity);
        float neededWidth = pref.x;

        // 取得文字 RectTransform
        RectTransform textRT = DiaText.rectTransform;

        float finalBoxWidth;  // 背景框實際寬度

        if (neededWidth <= innerMaxWidth)
        {
            // ✅ 文字一行就裝得下：拉到剛好包住文字＋padding，不要硬換行
            DiaText.enableWordWrapping = false;

            float textAreaWidth = neededWidth;

            finalBoxWidth = Mathf.Clamp(textAreaWidth + leftPadding + rightPadding,
                                        minWidth, maxWidth);

            // 這裡用 offset 來維持 padding：左邊固定 leftPadding，右邊固定 rightPadding
            textRT.anchorMin = new Vector2(0, textRT.anchorMin.y);
            textRT.anchorMax = new Vector2(1, textRT.anchorMax.y);

            // offsetMin.x = 左邊距父物件的距離
            // offsetMax.x = 右邊距父物件的距離（注意為負）
            textRT.offsetMin = new Vector2(leftPadding, textRT.offsetMin.y);
            textRT.offsetMax = new Vector2(-rightPadding, textRT.offsetMax.y);
        }
        else
        {
            // ❗裝不下一行：固定內文字區為 innerMaxWidth，交給 TMP 自己換行
            DiaText.enableWordWrapping = true;

            float textAreaWidth = innerMaxWidth;
            finalBoxWidth = maxWidth;   // 整個對話框就用最大寬度

            textRT.anchorMin = new Vector2(0, textRT.anchorMin.y);
            textRT.anchorMax = new Vector2(1, textRT.anchorMax.y);
            textRT.offsetMin = new Vector2(leftPadding, textRT.offsetMin.y);
            textRT.offsetMax = new Vector2(-rightPadding, textRT.offsetMax.y);
        }

        // 套到背景對話框（記得背景的 Image 用 Sliced）
        var boxSize = dialogueBoxRect.sizeDelta;
        boxSize.x = finalBoxWidth;
        dialogueBoxRect.sizeDelta = boxSize;
    }
}

