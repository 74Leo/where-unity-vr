using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FadeScreen : MonoBehaviour
{
    public static FadeScreen Instance { get; private set; }

    [Header("Réglages")]
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    private CanvasGroup canvasGroup;
    private UnityEngine.UI.Image fadeImage;
    private bool isFading = false;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupFadeScreen();
    }

    void SetupFadeScreen()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(transform, false);
        
        fadeImage = imageObj.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = fadeColor;
        
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (canvasGroup != null && canvasGroup.alpha > 0.1f)
        {
            StartCoroutine(DelayedFadeIn());
        }
    }

    IEnumerator DelayedFadeIn()
    {
        yield return new WaitForSeconds(0.1f);
        yield return FadeIn();
    }

    public IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f);
    }

    public IEnumerator FadeOut()
    {
        yield return Fade(0f, 1f);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (isFading) yield break;
        isFading = true;

        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
        isFading = false;
    }

    public static void LoadSceneWithFadeAsync(string sceneName)
    {
        if (Instance == null)
        {
            GameObject fadeObj = new GameObject("FadeScreen");
            Instance = fadeObj.AddComponent<FadeScreen>();
        }

        Instance.StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    static IEnumerator LoadSceneCoroutine(string sceneName)
    {
        Instance.SetAlpha(0f);
        yield return Instance.FadeOut();
        SceneManager.LoadScene(sceneName);
    }

    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }
}
