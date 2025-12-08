using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;

public class Zero : MonoBehaviour
{
    [Header("腳本")]
    public AnimationScript animationScript;
    public CControll cControllScript;
    [Tooltip("場景中負責計算找錯誤數量的管理員")]public SpotManager spotManager;
    public DialogueSystemGame00 DSG00;


    [Header("異常相關")]
    [Tooltip("異常畫面的背景")]public GameObject ErrorPanel;
    [Tooltip("設定異常的位置組")] public GameObject ErrorPlace;
    [Tooltip("教學-設定異常的位置組")] public GameObject ErrorPlaceTeach;
    [Tooltip("教學-設定異常的圈圈")] public GameObject CirclePlaceTeach;
    [Tooltip("異常光線")] public Light ErrorLight;
    public bool StartError;

    [Header("玩家")]
    public GameObject Player;
    [Tooltip("玩家教學用自動走到的位置")]public Vector2 teachTargetPos = new Vector2(19.3f, -4.3f);

    [Header("教學狀態")]
    [Tooltip("教學是否已經開始（玩家抵達座位後才會 true）")]public bool TeachStart = false;
    [Tooltip("教學是否已經結束（找到足夠異常後）")]public bool teachFinished = false;
    [Tooltip("這次教學要找到幾個異常才算完成")]public int teachNeedFound = 2;

    [Header("手機 UI")]
    [Tooltip("顯示在畫面上的手機介面 Panel")]
    public GameObject PhonePanel;
    [Tooltip("手機裡的『相機』按鈕")]
    public UnityEngine.UI.Button CameraButton;
    [Tooltip("紀錄玩家有沒有按相機")]public bool hasPressedCamera = false;

    [Header("其他")]
    public GameObject BlackPanel;//黑色遮罩
    [Tooltip("控制紅光閃爍的協程")] Coroutine warningCoroutine;

    private void Awake()
    {
        animationScript = GetComponent<AnimationScript>();
        DSG00 = FindAnyObjectByType<DialogueSystemGame00>();
        if (cControllScript == null)
        {
            cControllScript = FindAnyObjectByType<CControll>();
        }
        if (spotManager == null)
        {
            spotManager = FindAnyObjectByType<SpotManager>();
        }
    }
    private void Start()
    {
        ErrorPanel.SetActive(false);
        ErrorPlace.SetActive(false);
        CirclePlaceTeach.SetActive(false);
        ErrorPlaceTeach.SetActive(false);
        ErrorLight.color = new Color(1,1,1,0);

        if (PhonePanel != null)
            PhonePanel.SetActive(false);

        hasPressedCamera = false;

        // 整個場景流程交給協程控制，Start 只負責開頭
        StartCoroutine(SceneFlow());

        
    }

    IEnumerator SceneFlow()
    {
        yield return FadeInStart();

        // 2. 玩家自動走到指定位置
        if (cControllScript != null)
        {
            cControllScript.StartAutoMoveTo(new Vector2(teachTargetPos.x, teachTargetPos.y));

            yield return new WaitUntil(() => cControllScript.autoMoveFinished);
        }

        //2. 開始教學
        StartTeach();

        //2.1紅光亮起
        redLight();
        yield return new WaitUntil(() => ErrorLight.color.a==1f);

        //2.1對話
        DSG00.StartDialogue(DSG00.TextfileHowToPlay);
        yield return new WaitForSeconds(1f);

        //2.2看手機
        cControllScript.animator.SetBool("phone", true);
        yield return StartCoroutine(WaitForAnimation(cControllScript.animator, "phone"));
        //yield return new WaitForSeconds(0.5f);
        hasPressedCamera = false;
        // ⏳ 在這裡乖乖等玩家按
        yield return new WaitUntil(() => hasPressedCamera);

        // 玩家已經按了相機，可以收手機 UI、結束手機動畫
        PhonePanel.SetActive(false);
        cControllScript.animator.SetBool("phone", false);

        //3.errorpanel亮起
        // 🔥 紅光閃完 → 顯示異常提示 Panel
        openErrorPanel();
    }

    // Update is called once per frame
    void Update()
    {
        // 如果教學還沒開始或已經結束，就不用檢查
        if (!TeachStart || teachFinished) return;
        if (spotManager == null) return;

        // 🔎 檢查目前找到幾個異常
        if (TeachStart && !teachFinished && spotManager.foundCount >= teachNeedFound)
            StartCoroutine(OnTeachComplete());

    }

    public IEnumerator FadeInStart()//一開始的黑幕淡入
    {
        // 1. 黑幕淡出
        if (BlackPanel != null)
        {
            BlackPanel.SetActive(true);
            animationScript.Fade(
                BlackPanel,
                1.5f,
                1f,
                0f,
                null
            );
            
            yield return new WaitForSeconds(1.5f);
            BlackPanel.SetActive(false);
        }
    }
    public IEnumerator AbnormalLightOn(float duration)//讓窗外異常光線啟動（瞬間變紅、變亮）
    {
        float timer = 0f;
        Color c = ErrorLight.color;
        c.a = 0f;
        ErrorLight.color = c;

        // 淡入
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            c.a = Mathf.Lerp(0, 1, t);
            ErrorLight.color = c;

            yield return null;
        }

        c.a = 1;
        ErrorLight.color = c;
    }
    public IEnumerator AbnormalLightOff(float duration)//讓窗外異常光線關閉（瞬間變紅、變亮）
    {
        float timer = 0f;
        Color c = ErrorLight.color;
        c.a = 1f;
        ErrorLight.color = c;

        // 淡入
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            c.a = Mathf.Lerp(1, 0, t);
            ErrorLight.color = c;

            yield return null;
        }

        c.a = 0;
        ErrorLight.color = c;
    }

    //玩家抵達座位後開始教學：紅光閃爍＋允許玩家點異常
    void StartTeach()
    {
        TeachStart = false;
        teachFinished = false;

        Debug.Log("[First] 教學開始：啟動紅光警示、開放找異常");

        if (ErrorPlace != null)
            ErrorPlaceTeach.SetActive(false); // 等紅光閃完再打開
        CirclePlaceTeach.SetActive(false);
        // 可以視情況鎖或放開玩家控制
        if (cControllScript != null)
        {
            cControllScript.playerControlEnabled = false; // 教學期間先鎖住走動
        }
    }
    void redLight()
    {
        // 開始紅光閃爍（等你之後實作 abnormal() 和 RecoverNormalLight()）
        warningCoroutine = StartCoroutine(WindowWarningLoop());
    }

    //教學完成：停止紅光、恢復正常光線、之後可接下一段劇情
    IEnumerator OnTeachComplete()
    {
        yield return new WaitForSeconds(3f);
        teachFinished = true;
        Debug.Log("[First] 教學完成：找到足夠異常，恢復正常光線");

        // 停止紅光閃爍
        if (warningCoroutine != null)
        {
            //StopCoroutine(warningCoroutine);
            warningCoroutine = null;

            
            if (ErrorPlaceTeach != null)
                ErrorPlaceTeach.SetActive(false); // 關閉異常提示界面
            spotManager.ClearAllCircles();
            CirclePlaceTeach.SetActive(false);
            animationScript.Fade(ErrorPanel,2f,1f,0f,null);
            yield return new WaitForSeconds(2f);
            ErrorPanel.SetActive(false);

            // 恢復正常光線
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(AbnormalLightOff(2f));
            yield return new WaitForSeconds(0.5f);

            // 可選：恢復玩家控制／進入下一段劇情
            if (cControllScript != null)
            {
                cControllScript.playerControlEnabled = true;
            }
        }
    }

    //紅光閃爍的循環（之後你可以在裡面呼叫 2D Light 或改 shader 顏色）
    IEnumerator WindowWarningLoop()
    {
        Debug.Log("[First] 紅光閃爍啟動");
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(AbnormalLightOn(2f));
        //while (!teachFinished)
        //{
        //    // TODO：讓窗外變紅、閃爍一次
        //    AbnormalLightOn();

        //    yield return new WaitForSeconds(0.3f);

        //    // TODO：紅光變暗 / 關閉
        //    AbnormalLightOff();
        Debug.Log("開始變紅");
        yield return new WaitForSeconds(2.5f);
        //}
    }

    IEnumerator WaitForAnimation(Animator animator, string stateName)
    {
        // 等到進入該動畫 state
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        // 動畫還在播
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
    }

    public void openErrorPanel()
    {
        if (ErrorPlace != null)
        {
            Debug.Log("[First] 顯示異常提示畫面 ErrorPlace");
            ErrorPlaceTeach.SetActive(true);
            ErrorPanel.SetActive(true);
            animationScript.Fade(ErrorPanel, 2f, 0f, 1f, null);
            CirclePlaceTeach.SetActive(true);
            spotManager.RefreshActiveSpots();
        }

        // 🔥這裡開始允許玩家找錯誤（你本來系統自動會開始找）
        TeachStart = true;
    }

    public void OnCameraButtonClicked()
    {
        Debug.Log("[First] 玩家按下手機裡的相機按鈕");
        hasPressedCamera = true;
    }
}
