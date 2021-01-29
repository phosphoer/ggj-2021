using UnityEngine;
using System.Collections;

public static class Tween
{
  public static IEnumerator HermiteScale(Transform transform, Vector3 startScale, Vector3 endScale, float duration)
  {
    for (float timer = 0; timer < duration; timer += Time.deltaTime)
    {
      float t = Mathf.Clamp01(timer / duration);
      t = Mathfx.Hermite(0.0f, 1.0f, t);
      if (transform != null)
        transform.localScale = Vector3.Lerp(startScale, endScale, t);
      yield return null;
    }

    if (transform != null)
      transform.localScale = endScale;
  }

  public static IEnumerator HermiteScaleRealtime(Transform transform, Vector3 startScale, Vector3 endScale, float duration)
  {
    float startTime = Time.unscaledTime;
    while (Time.unscaledTime < startTime + duration)
    {
      float t = Mathf.Clamp01((Time.unscaledTime - startTime) / duration);
      t = Mathfx.Hermite(0.0f, 1.0f, t);
      if (transform != null)
        transform.localScale = Vector3.Lerp(startScale, endScale, t);
      yield return null;
    }
  }

  public static IEnumerator CurveScaleRealtime(Transform transform, float duration, AnimationCurve curve)
  {
    for (float timer = 0; timer < duration; timer += Time.unscaledDeltaTime)
    {
      float t = Mathf.Clamp01(timer / duration);
      float scaleValue = curve.Evaluate(t);
      if (transform != null)
        transform.localScale = Vector3.one * scaleValue;
      yield return null;
    }
  }
}
