// MAINMENU.CS  -  goes on an empty GameObject in your MAIN MENU scene.
//
// IN ONE SENTENCE: it has the functions your title-screen buttons call, slides the menu left
// and a panel (Options / How To Play) in from the right, and saves the option settings.
//
// SETUP:
//   - Put the logo, the ButtonGroup and the BestScoreText inside an empty "MainContent" object
//     (stretched to fill the Canvas). That's what slides left. The background stays put.
//   - OptionsPanel and HowToPlayPanel: left switched ON. The script moves them off-screen
//     to the right when the game starts.
//   - Buttons (OnClick list):
//       New Game     -> MainMenu.StartGame
//       How To Play  -> MainMenu.ToggleHowToPlay   (its Back button too)
//       Options      -> MainMenu.ToggleOptionsMenu (its Back button too)
//       Quit         -> MainMenu.QuitGame
//   - Toggles (On Value Changed, pick from the "Dynamic bool" section at the top):
//       Family Friendly -> SetFamilyFriendly, Rumble -> SetRumble, Camera Lean -> SetCameraLean
//   - Sliders (On Value Changed, "Dynamic float" section):
//       Music -> SetMusicVolume, Sound FX -> SetSfxVolume

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public string firstScene = "DucksGallery";

    [Header("Sliding")]
    public RectTransform mainContent;      // logo + buttons + best score (slides left)
    public RectTransform optionsPanel;     // slides in from the right
    public RectTransform howToPlayPanel;   // slides in from the right
    public float slideTime = 0.35f;        // seconds for one slide

    [Header("Options")]
    public Toggle familyFriendlyToggle;
    public Toggle rumbleToggle;
    public Toggle cameraLeanToggle;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Optional")]
    public TMP_Text bestScoreText;

    [Header("Sound")]
    public AudioClip clickSound;            // plays when you "shoot" a button
    public AudioClip sfxPreviewSound;       // plays while dragging the Sound FX slider (the quack)
    public float loadDelay = 0f;            // extra wait before fading

    bool loading;                           // stops double-clicks on New Game / Quit
    bool sliding;                           // stops clicks while a slide is happening
    bool ready;                             // so setting up the toggles doesn't play click sounds
    RectTransform openPanel;                // which panel is showing right now (null = none)
    float lastPreview;                      // so the preview sound doesn't spam while dragging

    // how far "off-screen" is: the width of the Canvas
    float OffScreen => ((RectTransform)mainContent.parent).rect.width;

    void Start()
    {
        Time.timeScale = 1f;   // make sure the game isn't still frozen after quitting from the pause menu
        Cursor.visible = true;

        if (bestScoreText != null)
            bestScoreText.text = "BEST!" + PlayerPrefs.GetInt("BestScore", 0).ToString("0000000");

        // menu in the middle, both panels waiting off-screen to the right
        if (mainContent != null) mainContent.anchoredPosition = Vector2.zero;
        ParkOffScreen(optionsPanel);
        ParkOffScreen(howToPlayPanel);

        // show the saved settings
        if (familyFriendlyToggle != null) familyFriendlyToggle.isOn = GameSettings.FamilyFriendly;
        if (rumbleToggle != null) rumbleToggle.isOn = GameSettings.Rumble;
        if (cameraLeanToggle != null) cameraLeanToggle.isOn = GameSettings.CameraLean;
        SetupSlider(musicSlider, GameSettings.MusicVolume);
        SetupSlider(sfxSlider, GameSettings.SfxVolume);

        ready = true;
    }

    void SetupSlider(Slider slider, float value)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
    }

    void ParkOffScreen(RectTransform panel)
    {
        if (panel == null) return;
        panel.gameObject.SetActive(true);
        panel.anchoredPosition = new Vector2(OffScreen, 0f);
    }

    // --- Button actions ---

    public void StartGame()
    {
        if (loading || sliding) return;
        StartCoroutine(LoadFirstScene());
    }

    IEnumerator LoadFirstScene()
    {
        loading = true;
        PlayClick();
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetAll();
        yield return new WaitForSeconds(loadDelay);
        SceneFader.FadeToScene(firstScene);
    }

    public void ToggleOptionsMenu()
    {
        TogglePanel(optionsPanel);
    }

    public void ToggleHowToPlay()
    {
        TogglePanel(howToPlayPanel);
    }

    public void QuitGame()
    {
        if (loading || sliding) return;
        StartCoroutine(QuitAfterSound());
    }

    IEnumerator QuitAfterSound()
    {
        loading = true;
        PlayClick();

        float wait = (clickSound != null) ? clickSound.length : 0.5f;
        yield return new WaitForSecondsRealtime(wait);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // stops Play mode in the editor
#else
        Application.Quit();                                // closes the real game
#endif
    }

    // --- Option controls (hook these to the toggles and sliders) ---

    public void SetFamilyFriendly(bool on)
    {
        GameSettings.FamilyFriendly = on;
        if (ready) PlayClick();
    }

    public void SetRumble(bool on)
    {
        GameSettings.Rumble = on;
        if (ready) PlayClick();
    }

    public void SetCameraLean(bool on)
    {
        GameSettings.CameraLean = on;
        if (ready) PlayClick();
    }

    public void SetMusicVolume(float value)
    {
        GameSettings.MusicVolume = value;   // the menu music changes live while you drag
    }

    public void SetSfxVolume(float value)
    {
        GameSettings.SfxVolume = value;

        // play a preview at the new volume, at most every 0.2 seconds while dragging
        if (ready && Time.unscaledTime - lastPreview > 0.2f)
        {
            lastPreview = Time.unscaledTime;
            if (SoundManager.Instance != null) SoundManager.Instance.Play(sfxPreviewSound, 1f, 0f);
        }
    }

    // --- Sliding ---

    void TogglePanel(RectTransform panel)
    {
        if (panel == null || sliding || loading) return;
        PlayClick();

        if (openPanel == null)
        {
            // OPEN: menu slides left, panel slides in from the right
            openPanel = panel;
            StartCoroutine(Slide(new Vector2(-OffScreen, 0f), panel, Vector2.zero));
        }
        else
        {
            // CLOSE: the reverse, panel slides back out right, menu slides back in
            RectTransform closing = openPanel;
            openPanel = null;
            StartCoroutine(Slide(Vector2.zero, closing, new Vector2(OffScreen, 0f)));
        }
    }

    IEnumerator Slide(Vector2 mainTo, RectTransform panel, Vector2 panelTo)
    {
        sliding = true;

        Vector2 mainFrom = mainContent.anchoredPosition;
        Vector2 panelFrom = panel.anchoredPosition;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / slideTime;
            float e = EaseInOut(Mathf.Clamp01(t));
            mainContent.anchoredPosition = Vector2.LerpUnclamped(mainFrom, mainTo, e);
            panel.anchoredPosition = Vector2.LerpUnclamped(panelFrom, panelTo, e);
            yield return null;
        }

        mainContent.anchoredPosition = mainTo;
        panel.anchoredPosition = panelTo;
        sliding = false;
    }

    // starts slow, speeds up, slows down at the end: a smooth slide
    float EaseInOut(float x)
    {
        return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }

    void PlayClick()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.Play(clickSound, 1f, 0f);
    }
}