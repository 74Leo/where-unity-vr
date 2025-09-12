using UnityEngine;
using System.Collections;

public class FadeScreen : MonoBehaviour
{
    public static FadeScreen Instance { get; private set; }

    [Header("Réglages")]
    public float fadeDuration = 0.7f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    CanvasGroup group;
    bool isFading;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();

        group.interactable = false;
        group.blocksRaycasts = false;
    }

    void Start()
    {
        if (group.alpha > 0.99f)
            StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
    }

    public IEnumerator FadeOut()
    {
        yield return Fade(0f, 1f);
    }

    IEnumerator Fade(float from, float to)
    {
        if (isFading) yield break;
        isFading = true;

        float t = 0f;
        group.alpha = from;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            float k = curve.Evaluate(Mathf.Clamp01(t));
            group.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }

        group.alpha = to;
        isFading = false;
    }

    public void SetAlpha(float a)
    {
        if (!group) group = GetComponent<CanvasGroup>();
        group.alpha = a;
    }
}
