using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFadeIn : MonoBehaviour
{
    [SerializeField] private float duration = 0.45f;

    private CanvasGroup canvasGroup;
    private float elapsed;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        elapsed = 0f;
        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (canvasGroup.alpha >= 1f)
        {
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        float progress = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
        canvasGroup.alpha = progress;
    }
}