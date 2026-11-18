// PAUSEMENU.CS  -  put it on an empty GameObject called "PauseMenu" in each BOOTH scene.
//
// IN ONE SENTENCE: Esc (or Start on a controller) freezes the game and shows the pause screen,
// which asks if you want to quit to the main menu. Esc again (or Resume) unfreezes it.
//
// SETUP:
//   - pausePanel: the pause screen UI (switched OFF, the script turns it on).
//   - Buttons: RESUME -> PauseMenu.Resume, QUIT -> PauseMenu.QuitToMenu
//
// "Time.timeScale = 0" freezes everything that uses normal game time (targets, the round timer,
// Curly's typing), while the menu, fades and sounds keep working.

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseMenu : MonoBehaviour
{
    // other scripts check this, so you can't shoot or skip dialogue while paused
    public static bool IsPaused { get; private set; }

    public GameObject pausePanel;
    public string mainMenuScene = "MainMenu";
    public AudioClip clickSound;

    bool leaving;   // stops pausing/unpausing once we're heading back to the menu

    void Start()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (leaving) return;
        if (PausePressed())
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;   // freeze the game
        if (pausePanel != null) pausePanel.SetActive(true);
        PlayClick();
    }

    public void Resume()
    {
        if (leaving) return;
        IsPaused = false;
        Time.timeScale = 1f;   // unfreeze
        if (pausePanel != null) pausePanel.SetActive(false);
        PlayClick();
    }

    public void QuitToMenu()
    {
        if (leaving) return;
        leaving = true;
        PlayClick();

        // throw away this run's score so it doesn't count toward the best score
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetAll();

        // stays frozen during the fade (the fade runs on real time), then the menu unfreezes it
        SceneFader.FadeToScene(mainMenuScene);
    }

    // if this scene closes for any reason, never leave the game frozen
    void OnDestroy()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }

    bool PausePressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool start = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
        return esc || start;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    void PlayClick()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.Play(clickSound, 1f, 0f);
    }
}