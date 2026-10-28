// SCOREMANAGER.CS  -  put it on an empty GameObject called "ScoreManager" in your first scene.
//
// IN ONE SENTENCE: the shared scoreboard. Any script can add points with
// ScoreManager.Instance.AddPoints(10).
//
// COMBOS: every 5 hits in a row raises the multiplier (x2, x3, x4 max). A miss, or shooting a
// "don't shoot" target, resets it. That means spamming clicks hurts your score.
//
// "static Instance" = one shared copy that any script can reach without dragging it in.
// DontDestroyOnLoad = the scoreboard survives when a new scene loads, so totals carry over.

using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score;       // this booth's score
    public int totalScore;  // all booths so far (used for the final results)
    public int combo;       // current streak of hits

    public int hitsPerLevel = 5;   // hits in a row needed to raise the multiplier
    public int maxMultiplier = 4;

    // 1 + (combo / 5), but never above maxMultiplier. Example: combo 12 -> 1 + 2 = x3
    public int Multiplier => Mathf.Min(1 + combo / hitsPerLevel, maxMultiplier);

    void Awake()
    {
        // If one already exists (from an earlier scene), destroy this duplicate
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
            combo = 0; // negative points (a don't-shoot hit) break the streak
            score = Mathf.Max(0, score + points); // score never goes below 0
        }
        Debug.Log("Score: " + score + "  Combo: " + combo + "  x" + Multiplier);
    }

    public void RegisterMiss()
    {
        combo = 0;
    }

    public void StartBooth()
    {
        score = 0;
        combo = 0;
    }

    public void EndBooth()
    {
        totalScore += score;
    }

    // Wipes everything for a brand-new run (used by "Play Again")
    public void ResetAll()
    {
        score = 0;
        totalScore = 0;
        combo = 0;
    }
}
