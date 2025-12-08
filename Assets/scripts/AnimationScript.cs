using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimationScript : MonoBehaviour
{
    void Start()
    {
    }

    public void Fade(GameObject panel, float fadeDuration, float start, float end, Action onCpmplete)
    {
        StartCoroutine(FadeE(panel, fadeDuration, start, end, onCpmplete));
    }
    public IEnumerator FadeE(GameObject panel, float fadeDuration,float start,float end, Action onComplete)
    {
        CanvasGroup canvasGroup= panel.GetComponent<CanvasGroup>();

        float timer = 0f;
        canvasGroup.alpha = start;

        // 淡入
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = end;

        // 🔔 淡入做完，通知外面「可以下一步囉」
        onComplete?.Invoke();
    }
}
