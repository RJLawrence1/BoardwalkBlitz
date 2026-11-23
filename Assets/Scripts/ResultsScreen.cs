// RESULTSSCREEN.CS  -  put it on an empty GameObject in the ScoreTally scene.
//
// IN ONE SENTENCE: counts up your total score, saves it if it's a new best, then shows your
// rank, Curly in a pose that fits, and types out his reaction with an Undertale-style voice blip
// (family friendly or not).
//
// LED GHOSTS: the gray "unlit segment" texts are built automatically to match the real text's
// length, so they always line up. Give each ghost the SAME font, size, alignment and
// Rect Transform as the text it sits behind, and put it ABOVE that text in the Hierarchy.
//
// CURLY: a UI Image on the Canvas (curlyImage). It stays hidden until the rank appears,
// then shows that rank's Pose.
//
// MUSIC: put a MusicManager in this scene with the results song in its Track slot.
// It fades out when you leave.
//
// SETUP: drag in the texts, ghosts and Curly's image. Two buttons: "Play Again" -> PlayAgain(),
// "Main Menu" -> BackToMenu(). Ranks go LOWEST FIRST.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Rank
{
    public string title;
    public int minScore;                 // you need at least this many points for this rank
    public Sprite pose;                  // Curly's pose for this rank
    [TextArea] public string curlyLine;  // family friendly (default)
    [TextArea] public string altLine;    // used when Family Friendly is OFF (leave empty to use curlyLine)
}

public class ResultsScreen : MonoBehaviour
{
    [Header("Score display (DSEG7)")]
    public TMP_Text scoreText;
    public TMP_Text scoreGhost;          // gray text behind the score
    public char scoreGhostChar = '8';    // "8" lights every segment in DSEG7 (use '~' for DSEG14)
    public int scoreDigits = 7;

    [Header("Rank display (DSEG14)")]
    public TMP_Text rankText;
    public TMP_Text rankGhost;           // gray text behind the rank
    public char rankGhostChar = '~';     // "~" lights every segment in DSEG14

    [Header("Curly")]
    public TMP_Text curlyText;
    public Image curlyImage;             // Curly on the results screen (hidden until the rank shows)

    [Header("Curly's voice (Undertale-style blips)")]
    public float charsPerSecond = 40f;
    public AudioClip blipSound;                  // the same blip as the booths
    public int blipEvery = 2;                    // blip every N letters
    [Range(0.5f, 2f)] public float voicePitch = 1.1f;
    [Range(0f, 0.3f)] public float voicePitchVariation = 0.05f;
    [Range(0f, 1f)] public float voiceVolume = 0.6f;

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
            curlyLine = "Now we're talking! You've got a good eye, friend. The ducks are mildly impressed, and that's hard to do.",
            altLine   = "Okay, okay, you can actually aim. I'm almost impressed. Almost."
        },
        new Rank {
            title = "DEADEYE", minScore = 45000,
            curlyLine = "Spectacular! Phenomenal! I'd tip my hat, if they'd given me one.",
            altLine   = "Damn, alright! I'd tip my hat, but they didn't budget for one."
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

        // start: score shows all zeros, rank and Curly are hidden
        SetLED(scoreText, scoreGhost, FormatScore(0), scoreGhostChar);
        SetLED(rankText, rankGhost, "", rankGhostChar);
        if (curlyText != null) curlyText.text = "";
        if (curlyImage != null) curlyImage.enabled = false;

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

        // show the rank and Curly's pose
        Rank r = GetRank(total);
        if (r == null) yield break;

        SetLED(rankText, rankGhost, r.title, rankGhostChar);
        if (SoundManager.Instance != null) SoundManager.Instance.Play(rankSound, 1f, 0f);

        if (curlyImage != null && r.pose != null)
        {
            curlyImage.sprite = r.pose;
            curlyImage.preserveAspect = true;
            curlyImage.enabled = true;
        }

        yield return new WaitForSeconds(0.4f);   // let the rank land before he talks

        // type out Curly's line with his voice
        bool useAlt = !GameSettings.FamilyFriendly && !string.IsNullOrEmpty(r.altLine);
        yield return TypeLine(useAlt ? r.altLine : r.curlyLine);
    }

    // Typewriter + Undertale-style blips: letters and numbers blip, spaces and punctuation stay silent
    IEnumerator TypeLine(string text)
    {
        if (curlyText == null || string.IsNullOrEmpty(text)) yield break;

        curlyText.text = text;
        curlyText.maxVisibleCharacters = 0;

        float typed = 0f;
        int visible = 0;
        int lettersSinceBlip = 0;

        while (visible < text.Length)
        {
            typed += charsPerSecond * Time.deltaTime;
            int now = Mathf.Min(Mathf.FloorToInt(typed), text.Length);

            while (visible < now)
            {
                char c = text[visible];
                visible++;

                if (char.IsLetterOrDigit(c))
                {
                    lettersSinceBlip++;
                    if (lettersSinceBlip >= blipEvery)
                    {
                        lettersSinceBlip = 0;
                        if (blipSound != null && SoundManager.Instance != null)
                            SoundManager.Instance.Play(blipSound, voiceVolume, voicePitchVariation, voicePitch);
                    }
                }
            }
            curlyText.maxVisibleCharacters = visible;
            yield return null;
        }

        curlyText.maxVisibleCharacters = 99999;
    }

    // Pads the score with zeros to a fixed number of digits (e.g. 7 -> "0004250").
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
        if (ghost != null) ghost.text = new string(ghostChar, led.Length);
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
        FadeOutMusic();
        SceneFader.FadeToScene(firstBoothScene);
    }

    public void BackToMenu()
    {
        if (leaving) return;
        leaving = true;
        if (ScoreManager.Instance != null) ScoreManager.Instance.ResetAll();
        FadeOutMusic();
        SceneFader.FadeToScene(mainMenuScene);
    }

    // results music fades out along with the screen
    void FadeOutMusic()
    {
        if (MusicManager.Instance != null) MusicManager.Instance.FadeOut(SceneFader.fadeTime);
    }
}