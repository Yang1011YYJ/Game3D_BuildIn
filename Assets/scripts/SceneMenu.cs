using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMenu : MonoBehaviour
{
    [Header("腳本")]
    public AnimationScript animationScript;
    public SceneChange sceneChangeScript;

    [Header("其他")]
    public GameObject BlackPanel;//黑色遮罩
    private void Awake()
    {
        animationScript = GetComponent<AnimationScript>();
        sceneChangeScript = GetComponent<SceneChange>();
        BlackPanel = GameObject.Find("BlackPanel");
    }
    private void Start()
    {
        BlackPanel.SetActive(false);
    }
    public void SceneChangeToDes()
    {
        BlackPanel.SetActive(true);
        animationScript.Fade(
            BlackPanel, 
            1.5f,
            0f,
            1f,
            () => sceneChangeScript.SceneC("des")
        );
        //BlackPanel.SetActive(false );
    }
}
