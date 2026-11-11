// SCOREMANAGER.CS  -  put it on an empty GameObject called "ScoreManager" in your first scene.
//
// IN ONE SENTENCE: the shared scoreboard. Any script can add points with
// ScoreManager.Instance.AddPoints(10).
//
// COMBOS: every "hitsPerLevel" hits in a row raises the multiplier (up to maxMultiplier).
// A miss, or shooting a "don't shoot" target, resets it.
//
// ROUNDS: each finished booth adds its score to totalScore and bumps roundsPlayed,
// so it works for any number of booths in a row.
//
// "static Instance" = one shared copy that any script can reach without dragging it in.
// DontDestroyOnLoad = the scoreboard survives when a new scene loads, so totals carry over.

using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score;          // this booth's score
    public int totalScore;     // all booths so far (used for the final results)
    public int combo;          // current streak of hits
    public int roundsPlayed;   // how many booths have been finished this run

    public int hitsPerLevel = 2;   // hits in a row needed to raise the multiplier
    public int maxMultiplier = 4;

    // 1 + (combo / hitsPerLevel), but never above maxMultiplier
    public int Multiplier => Mathf.Min(1 + combo / hitsPerLevel, maxMultiplier);

    void Awake()
    {
        // If one already exists (from an earlier scene), destroy this duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // DontDestroyOnLoad only works on top-level objects, so make sure this one is top-level
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    public void AddPoints(int points)
    {
        if (points > 0)
        {
            combo++;
            score += points * Multiplier;
        }
        else
        {
            combo = 0;                             // negative points (a don't-shoot hit) break the streak
            score = Mathf.Max(0, score + points);  // score never goes below 0
        }
    }

    public void RegisterMiss()
    {
        combo = 0;
    }

    public void StartBooth()
    {
        score = 0;
        combo = 0;
        Debug.Log("Round " + (roundsPlayed + 1) + " started. Total so far: " + totalScore);
    }

    public void EndBooth()
    {
        totalScore += score;
        roundsPlayed++;
        Debug.Log("Round " + roundsPlayed + " ended. Round score: " + score + "  New total: " + totalScore);
    }

    // Wipes everything for a brand-new run (used by New Game and Play Again)
    public void ResetAll()
    {
        score = 0;
        totalScore = 0;
        combo = 0;
        roundsPlayed = 0;
        Debug.Log("Scores reset for a new run.");
    }
}