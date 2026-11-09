// SCENEFADER.CS  -  no setup needed, don't put it on anything.
//
// IN ONE SENTENCE: call SceneFader.FadeToScene("SceneName") instead of SceneManager.LoadScene,
// and it fades to black, holds for a moment, loads the scene, then fades back in.
//
// It builds its own black screen the first time it's used, and keeps it alive between scenes.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static float fadeTime = 0.6f;   // seconds for each fade (out and in)
    public static float holdTime = 0.5f;   // how long to stay on black between scenes (0 = no pause)

    static SceneFader instance;
    CanvasGroup group;
    bool busy;

    // Call this from anywhere: SceneFader.FadeToScene("DucksGallery");
    public static void FadeToScene(string sceneName)
    {
        if (instance == null) Create();
        if (!instance.busy) instance.StartCoroutine(instance.Fade(sceneName));
    }

    // Builds a full-screen black image on its own canvas, drawn on top of everything
    static void Create()
    {
        GameObject go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;                  // always on top
        go.AddComponent<GraphicRaycaster>();        // lets it block clicks while fading

        instance = go.AddComponent<SceneFader>();
        instance.group = go.AddComponent<CanvasGroup>();
        instance.group.alpha = 0f;
        instance.group.blocksRaycasts = false;

        GameObject black = new GameObject("Black");
        black.transform.SetParent(go.transform, false);
        Image img = black.AddComponent<Image>();
        img.color = Color.black;

        // stretch to fill the whole screen
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    IEnumerator Fade(string sceneName)
    {
        busy = true;
        group.blocksRaycasts = true;                          // no clicking buttons mid-fade

        yield return FadeTo(1f);                              // fade to black
        yield return SceneManager.LoadSceneAsync(sceneName);  // load the next scene behind the black
        yield return null;                                    // skip the slow first frames after loading
        yield return null;
        yield return new WaitForSecondsRealtime(holdTime);    // short pause on black
        yield return FadeTo(0f);                              // fade into the new scene

        group.blocksRaycasts = false;
        busy = false;
    }

    IEnumerator FadeTo(float target)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < fadeTime)
        {
            // never count more than 1/30th of a second per frame, so a laggy frame can't skip the fade
            t += Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
            group.alpha = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
        group.alpha = target;
    }
}