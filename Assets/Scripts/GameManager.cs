// GAMEMANAGER.CS  -  goes on an empty GameObject in your RESULTS scene (the one after the last booth).
//
// IN ONE SENTENCE: when the results scene opens, it reads your total score, gives you a RANK
// (Rookie, Sharpshooter, or Carnival Legend), shows your score and personal best, and turns on
// the ending that matches your rank.
//
// There is no winning or losing - the point is to test your skills and beat your own best.
//
// HOW THE GAME FLOWS: each booth's BoothManager has a "Next Scene Name", so the booths chain
// together by themselves:  Booth 1 -> Booth 2 -> Results.  This script only handles the end.
//
// SETUP:
//   - endings: a list of 3 objects (a panel, or the ending cutscene), in order:
//       Element 0 = Rookie, Element 1 = Sharpshooter, Element 2 = Carnival Legend.
//     Only the matching one is switched on. Leave all of them OFF in the editor to start.
//   - Buttons: on a UI Button, use the OnClick list to call PlayAgain, GoToMenu, or QuitGame.
//   - Add every scene to File > Build Settings, or LoadScene won't find it.
//
// TUNING THE RANKS: play the game a few times, then set sharpshooterScore to a decent run and
// legendScore to a great run.

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Score needed for each rank")]
    public int sharpshooterScore = 800;
    public int legendScore = 1500;

    [Header("Ending screens: 0 = Rookie, 1 = Sharpshooter, 2 = Legend (leave all off in the editor)")]
    public GameObject[] endings;

    [Header("Optional text")]
    public TMP_Text yourScoreText;
    public TMP_Text bestScoreText;
    public TMP_Text rankText;
    public string[] rankNames = { "ROOKIE", "SHARPSHOOTER", "CARNIVAL LEGEND" };

    [Header("Scene names for the buttons")]
    public string playAgainScene = "Booth_Ducks";
    public string menuScene = "MainMenu";

    void Start()
    {
        // If you start straight from this scene while testing, there's no ScoreManager, so total = 0
        int total = (ScoreManager.Instance != null) ? ScoreManager.Instance.totalScore : 0;
        int rank = GetRank(total, sharpshooterScore, legendScore);

        // PlayerPrefs saves a small number on the player's computer, so the best score survives closing the game
        int best = PlayerPrefs.GetInt("BestScore", 0);
        if (total > best)
        {
            best = total;
            PlayerPrefs.SetInt("BestScore", best);
            PlayerPrefs.Save();
        }

        if (yourScoreText != null) yourScoreText.text = "Your score: " + total;
        if (bestScoreText != null) bestScoreText.text = "Best: " + best;
        if (rankText != null && rank < rankNames.Length) rankText.text = rankNames[rank];

        // Turn on only the ending that matches your rank
        for (int i = 0; i < endings.Length; i++)
        {
            if (endings[i] != null) endings[i].SetActive(i == rank);
        }
    }

    // "static" = you can ask it without needing an object. Returns 0 (Rookie), 1 (Sharpshooter), or 2 (Legend).
    public static int GetRank(int score, int sharpshooterScore, int legendScore)
    {
        if (score >= legendScore) return 2;
        if (score >= sharpshooterScore) return 1;
        return 0;
    }

    // --- Button actions (hook these up in a Button's OnClick list) ---

    public void PlayAgain()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetAll(); // fresh run, scores back to 0
        SceneManager.LoadScene(playAgainScene);
    }

    public void GoToMenu()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetAll();
        SceneManager.LoadScene(menuScene);
    }

    public void QuitGame()
    {
        Application.Quit(); // does nothing inside the editor, only in a built game
    }
}
