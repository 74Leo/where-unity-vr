using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndLevel : MonoBehaviour
{
    [Tooltip("Nom de la scène cible")]
    public string targetSceneName;

    [Tooltip("Bloque les retriggers pendant le chargement.")]
    public bool oneShot = true;

    bool used = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && oneShot) return;
        if (!other.CompareTag("Player")) return;

        used = true;
        StartCoroutine(LoadSceneWithFade());
    }

    IEnumerator LoadSceneWithFade()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[EndLevel] targetSceneName est vide.");
            used = false;
            yield break;
        }

        if (FadeScreen.Instance != null)
            yield return FadeScreen.Instance.FadeOut();

        AsyncOperation op = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        if (FadeScreen.Instance != null)
            FadeScreen.Instance.SetAlpha(1f);

        op.allowSceneActivation = true;
        while (!op.isDone)
            yield return null;

        yield return null;
        yield return new WaitForEndOfFrame();

        if (FadeScreen.Instance != null)
            yield return FadeScreen.Instance.FadeIn();

        if (!oneShot) used = false;
    }
}