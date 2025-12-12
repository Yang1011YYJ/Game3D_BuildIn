using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostExposureFader : MonoBehaviour
{
    public PostProcessVolume volume;
    ColorGrading cg;

    void Awake()
    {
        volume.profile.TryGetSettings(out cg);
    }

    public IEnumerator FadeExposure(float duration, float from, float to)
    {
        float t = 0f;
        cg.postExposure.value = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.postExposure.value = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        cg.postExposure.value = to;
    }
}
