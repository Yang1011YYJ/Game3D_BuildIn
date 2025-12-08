using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CircleFill : MonoBehaviour
{
    public Image img;
    public float duration = 0.5f;

    void Awake()
    {
        if (img == null)
            img = GetComponent<Image>();
        img.fillAmount = 0;
    }

    public void Play()
    {
        StartCoroutine(FillAnim());
    }

    IEnumerator FillAnim()
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            img.fillAmount = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }

        img.fillAmount = 1;
    }
}
