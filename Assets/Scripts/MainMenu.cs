// MAINMENU.CS  -  goes on an empty GameObject in your MAIN MENU scene.
//
// IN ONE SENTENCE: it has the functions your title-screen buttons call (Play, How To Play, Quit)
// and shows your best score.
//
// SETUP:
//   - Make UI Buttons (Play, How To Play, Quit). On each Button, use the OnClick list:
//       Play         -> MainMenu.StartGame
//       How To Play  -> MainMenu.ToggleHowToPlay
//       Quit         -> MainMenu.QuitGame
//   - howToPlayPanel: a UI Panel with the instructions on it. Leave it switched OFF in the editor.
//   - bestScoreText (optional): a TextMeshPro text that shows "Best: 1234".
//   - firstScene: the scene that starts the game (your intro cutscene scene).
//   - Add every scene to File > Build Settings, or LoadScene won't find it.

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public string firstScene = "Intro";

    [Header("Optional")]
    public GameObject howToPlayPanel;
    public TMP_Text bestScoreText;

    void Start()
    {
        // The booths hide the mouse cursor (the crosshair replaces it), so make sure it's back here
        Cursor.visible = true;

        // PlayerPrefs is where GameManager saved the best score
        if (bestScoreText != null)
        {
            bestScoreText.text = "Best: " + PlayerPrefs.GetInt("BestScore", 0);
        }
    }

    // --- Button actions ---

    public void StartGame()
    {
        // A fresh run starts with every score back at zero
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetAll();
        SceneManager.LoadScene(firstScene);
    }

    // Click once to show the instructions, click again to hide them
    public void ToggleHowToPlay()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(!howToPlayPanel.activeSelf);
    }

    public void QuitGame()
    {
        Application.Quit(); // does nothing inside the editor, only in a built game
    }
}
