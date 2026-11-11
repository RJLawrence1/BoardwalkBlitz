// RESULTSSCREEN.CS  -  put it on an empty GameObject in the ScoreTally scene.
//
// IN ONE SENTENCE: counts up your total score, saves it if it's a new best, then shows your
// rank and Curly's reaction (family friendly or not, based on the Options setting).
//
// LED GHOSTS: the gray "unlit segment" texts are built automatically to match the real text's
// length, so they always line up. Give each ghost the SAME font, size, alignment and
// Rect Transform as the text it sits behind, and put it ABOVE that text in the Hierarchy.
//
// SETUP: drag in the texts and ghosts. Two buttons: "Play Again" -> PlayAgain(),
// "Main Menu" -> BackToMenu(). Ranks go LOWEST FIRST.

using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Rank
{
    public string title;
    public int minScore;                 // you need at least this many points for this rank
    [TextArea] public string curlyLine;  // family friendly (default)
    [TextArea] public string altLine;    // used when Family Friendly is OFF (leave empty to use curlyLine)
}

public class ResultsScreen : MonoBehaviour
{
    [Header("Score display (DSEG7)")]
    public TMP_Text scoreText;
    public TMP_Text scoreGhost;          // gray text behind the score
    public char scoreGhostChar = '8';    // "8" lights every segment in DSEG7
    public int scoreDigits = 7;

    [Header("Rank display (DSEG14)")]
    public TMP_Text rankText;
    public TMP_Text rankGhost;           // gray text behind the rank
    public char rankGhostChar = '~';     // "~" lights every segment in DSEG14

    [Header("Curly")]
    public TMP_Text curlyText;

    [Header("Ranks (lowest first)")]
    public Rank[] ranks =
    {
        new Rank {
            title = "NO SHOW", minScore = 0,
            curlyLine = "...Did you forget to pull the trigger? That's okay, friend. Everyone had a lovely evening anyway.",
            altLine   = "Zero. Zero points. I've seen the ticket booth guy score higher, and he was asleep."
        },
        new Rank {
            title = "ROOKIE", minScore = 1,
            curlyLine = "A valiant effort! And by valiant, I mean... you showed up. That counts!",
            altLine   = "Well! You definitely held a cork gun. I'll give you that much."
        },
        new Rank {
            title = "MIDWAY REGULAR", minScore = 15000,
            curlyLine = "Hey, not bad! You've got the makings of a real boardwalk regular.",
            altLine   = "Not bad. Not good either. Very... middle of the midway."
        },
        new Rank {
            title = "SHARPSHOOTER", minScore = 30000,
            curlyLine = "Not bad at all! Everyone here is mildly impressed, and that's hard to do.",
            altLine   = "Okay, okay, you can actually aim. I'm almost impressed. Almost."
        },
        new Rank {
            title = "DEADEYE", minScore = 45000,
            curlyLine = "Spectacular! Phenomenal! I'd tip my hat, but it's stuck.",
            altLine   = "Damn, alright! I'd tip my hat, but honestly I don't get paid enough to fight with it."
        },
        new Rank {
            title = "CARNIVAL ACE", minScore = 60000,
            curlyLine = "Everyone, keep an eye on this one! The prize booth is getting nervous.",
            altLine   = "Somebody lock up the giant teddy bears, this one's coming for all of 'em."
        },
        new Rank {
            title = "BOARDWALK LEGEND", minScore = 80000,
            curlyLine = "...Wait, seriously? Ladies and gentlemen, we have a CHAMPION!",
            altLine   = "...Are you cheating? No? Then what the hell. Ladies and gentlemen, a LEGEND!"
        },
    };

    [Header("Timing and sound")]
    public float countUpTime = 2f;     // how long the score takes to count up
    public AudioClip rankSound;        // plays when the rank appears

    [Header("Scenes")]
    public string firstBoothScene = "DucksGallery";
    public string mainMenuScene = "MainMenu";

    bool leaving;                      // stops double-clicks

    IEnumerator Start()
    {
        int total = (ScoreManager.Instance != null) ? ScoreManager.Instance.totalScore : 0;

        // save a new best score (the main menu shows it)
        if (total > PlayerPrefs.GetInt("BestScore", 0))
        {
            PlayerPrefs.SetInt("BestScore", total);
            PlayerPrefs.Save();
        }

        // start: score shows all zeros, rank and Curly are blank
        SetLED(scoreText, scoreGhost, FormatScore(0), scoreGhostChar);
        SetLED(rankText, rankGhost, "", rankGhostChar);
        if (curlyText != null) curlyText.text = "";

        yield return new WaitForSeconds(1f);   // let the fade-in finish first

        // count the score up from 0
        float t = 0f;
        while (t < countUpTime)
        {
            t += Time.deltaTime;
            int shown = Mathf.RoundToInt(Mathf.Lerp(0, total, t / countUpTime));
            SetLED(scoreText, scoreGhost, FormatScore(shown), scoreGhostChar);
            yield return null;
        }
        SetLED(scoreText, scoreGhost, FormatScore(total), scoreGhostChar);

        yield return new WaitForSeconds(0.5f);   // little dramatic pause

        // show the rank and Curly's line (alt line only if Family Friendly is off AND one exists)
        Rank r = GetRank(total);
        if (r != null)
        {
            SetLED(rankText, rankGhost, r.title, rankGhostChar);

            bool useAlt = !GameSettings.FamilyFriendly && !string.IsNullOrEmpty(r.altLine);
            if (curlyText != null) curlyText.text = useAlt ? r.altLine : r.curlyLine;
        }
        if (SoundManager.Instance != null) SoundManager.Instance.Play(rankSound, 1f, 0f);
    }

    // Pads the score with zeros to a fixed number of digits (e.g. 7 -> "0004250").
    // If a score ever gets longer than that, it just shows all of it.
    string FormatScore(int score)
    {
        return score.ToString(new string('0', scoreDigits));
    }

    // Puts text on an LED display and builds a matching ghost behind it.
    // Spaces become "!" (a same-width blank in DSEG fonts) so letters line up with the ghost.
    void SetLED(TMP_Text main, TMP_Text ghost, string text, char ghostChar)
    {
        string led = text.Replace(' ', '!');
        if (main != null) main.text = led;
        if (ghost != null) ghost.text = new string(ghostChar, led.Length);  // one lit segment per character
    }

    // goes through the ranks lowest to highest and keeps the best one you qualify for
    Rank GetRank(int score)
    {
        Rank best = null;
        foreach (Rank r in ranks)
        {
            if (score >= r.minScore) best = r;
        }
        return best;
    }

    // hook these up to buttons
    public void PlayAgain()
    {
        if (leaving) return;
        leaving = true;
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetAll();
        SceneFader.FadeToScene(firstBoothScene);
    }

    public void BackToMenu()
    {
        if (leaving) return;
        leaving = true;
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetAll();
        SceneFader.FadeToScene(mainMenuScene);
    }
}