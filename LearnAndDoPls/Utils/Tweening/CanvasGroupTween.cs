using System;
using System.Collections;
using UnityEngine;

namespace CDTU.Utils
{
    /// <summary>
    /// Dependency-free CanvasGroup fades. Run the returned enumerator as a coroutine.
    /// </summary>
    public static class CanvasGroupTween
    {
        public static IEnumerator FadeIn(
            CanvasGroup canvasGroup,
            float durationSeconds,
            float targetAlpha = 1f,
            bool useUnscaledTime = true)
        {
            return FadeTo(canvasGroup, targetAlpha, durationSeconds, useUnscaledTime);
        }

        public static IEnumerator FadeOut(
            CanvasGroup canvasGroup,
            float durationSeconds,
            float targetAlpha = 0f,
            bool useUnscaledTime = true)
        {
            return FadeTo(canvasGroup, targetAlpha, durationSeconds, useUnscaledTime);
        }

        public static IEnumerator FadeTo(
            CanvasGroup canvasGroup,
            float targetAlpha,
            float durationSeconds,
            bool useUnscaledTime = true)
        {
            if (canvasGroup == null)
                throw new ArgumentNullException(nameof(canvasGroup));
            if (!IsFinite(durationSeconds) || durationSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            if (!IsFinite(targetAlpha))
                throw new ArgumentOutOfRangeException(nameof(targetAlpha));

            targetAlpha = Mathf.Clamp01(targetAlpha);
            if (durationSeconds == 0f)
            {
                canvasGroup.alpha = targetAlpha;
                yield break;
            }

            var startAlpha = canvasGroup.alpha;
            var elapsedSeconds = 0f;
            while (elapsedSeconds < durationSeconds)
            {
                if (canvasGroup == null)
                    yield break;

                elapsedSeconds += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                var progress = Mathf.Clamp01(elapsedSeconds / durationSeconds);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = targetAlpha;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
