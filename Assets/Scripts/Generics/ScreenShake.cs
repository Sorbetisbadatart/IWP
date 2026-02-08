using UnityEngine;
using System.Collections;

public static class ScreenShake
{
    // Shake a RectTransform (for UI elements)
    public static IEnumerator ShakeRectTransform(RectTransform rectTransform, float duration, float intensity)
    {
        Vector3 originalPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Left-right shake using sine wave
            float x = originalPosition.x + Mathf.Sin(elapsed * 30f) * intensity;
            float y = originalPosition.y;

            rectTransform.anchoredPosition = new Vector3(x, y, originalPosition.z);

            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
    }

    // Shake a regular Transform (for world objects)
    public static IEnumerator ShakeTransform(Transform transform, float duration, float intensity)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Random shake in all directions
            float x = originalPosition.x + Random.Range(-intensity, intensity);
            float y = originalPosition.y + Random.Range(-intensity, intensity);

            transform.localPosition = new Vector3(x, y, originalPosition.z);

            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    // Shake with decay (starts strong, ends weak)
    public static IEnumerator ShakeWithDecay(RectTransform rectTransform, float duration, float maxIntensity)
    {
        Vector3 originalPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Calculate decay factor
            float decay = 1f - (elapsed / duration);

            // Apply shake with decay
            float x = originalPosition.x + Mathf.Sin(elapsed * 40f) * maxIntensity * decay;
            float y = originalPosition.y + Mathf.Cos(elapsed * 35f) * maxIntensity * decay * 0.5f;

            rectTransform.anchoredPosition = new Vector3(x, y, originalPosition.z);

            yield return null;
        }

        rectTransform.anchoredPosition = originalPosition;
    }
}