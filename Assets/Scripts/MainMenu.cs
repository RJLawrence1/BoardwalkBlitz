// MAINMENU.CS  -  goes on an empty GameObject in your MAIN MENU scene.
//
// IN ONE SENTENCE: it has the functions your title-screen buttons call (Play, How To Play,
// Options, Quit) and shows your best score.
//
// SETUP:
//   - Make UI Buttons. On each Button, use the OnClick list:
//       New Game     -> MainMenu.StartGame
//       How To Play  -> MainMenu.ToggleHowToPlay
//       Options      -> MainMenu.ToggleOptionsMenu
//       Quit         -> MainMenu.QuitGame
//   - howToPlayPanel / optionsPanel: UI Panels. Leave them switched OFF in the editor.
//     A "Back" button inside each panel can call the same Toggle function to close it.
//   - bestScoreText (optional): a TextMeshPro text that shows the best score.
//   - firstScene: the scene that starts the game.
//   - Add every scene to File > Build Settings, or LoadScene won't find it.

using System.Collections;
using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public string firstScene = "DucksGallery";

    [Header("Panels")]
    public GameObject howToPlayPanel;
    public GameObject optionsPanel;

    [Header("Optional")]
    public TMP_Text bestScoreText;

    [Header("Sound")]
    public AudioClip clickSound;            // plays when you "shoot" a button
    public float loadDelay = 0f;            // extra wait before fading (the fade already gives the sound time)

    bool loading;                           // stops double-clicks from loading twice

    void Start()
    {
        Cursor.visible = true;

        if (bestScoreText != null)
        {
            bestScoreText.text = "Best: " + PlayerPrefs.GetInt("BestScore", 0);
        }

        // make sure both panels start closed
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    // --- Button actions ---

    public void StartGame()
    {
        if (loading) return;
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

    public void ToggleHowToPlay()
    {
        PlayClick();
        TogglePanel(howToPlayPanel, optionsPanel);
    }

    public void ToggleOptionsMenu()
    {
        PlayClick();
        TogglePanel(optionsPanel, howToPlayPanel);
    }

    public void QuitGame()
    {
        if (loading) return;            // ignore extra clicks
        StartCoroutine(QuitAfterSound());
    }

    IEnumerator QuitAfterSound()
    {
        loading = true;
        PlayClick();

        // wait for the sound to finish (or half a second if there's no sound)
        float wait = (clickSound != null) ? clickSound.length : 0.5f;
        yield return new WaitForSecondsRealtime(wait);

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // stops Play mode in the editor
    #else
        Application.Quit();                                // closes the real game
    #endif
    }

    // Opens or closes "panel", and always closes "other" so they never overlap
    void TogglePanel(GameObject panel, GameObject other)
    {
        if (panel == null) return;
        bool open = !panel.activeSelf;
        panel.SetActive(open);
        if (open && other != null) other.SetActive(false);
    }

    void PlayClick()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.Play(clickSound, 1f, 0f);
    }
}