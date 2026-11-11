// BOOTHMANAGER.CS  -  one per booth scene. Put it on an empty GameObject called "BoothManager".
//
// IN ONE SENTENCE: runs one round from start to finish: "ROUND #", countdown, the timed round,
// "TIME!", this round's score, the running total, then it loads the next scene.
//
// WORKS FOR ANY NUMBER OF ROUNDS: each booth adds its score to the running total and bumps the
// round counter in ScoreManager, so you can chain as many booth scenes as you like.
//
// SETUP: drag the Main Camera (it has Shooter) into "Shooter", your TargetSpawner(s) into
// "Spawners", and the TextMeshPro texts into the text slots.
// Use DSEG14 on Message Text so words like "ROUND" and "TIME!" display properly.
// Type the next scene's exact name into "Next Scene Name" (and add it to the Scene List).

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class BoothManager : MonoBehaviour
{
    [Header("Timing")]
    public int countdownSeconds = 3;   // the "3, 2, 1" before the round
    public float roundTime = 60f;      // length of the round (try 15 while testing)
    public float messageTime = 1.5f;   // how long "ROUND #", "TIME!" and the score screens stay up

    [Header("Hookups")]
    public Shooter shooter;            // the Shooter on your Main Camera
    public TargetSpawner[] spawners;   // every spawner in this booth
    public TMP_Text timerText;         // optional
    public TMP_Text scoreText;         // optional
    public TMP_Text messageText;       // optional (shows ROUND #, 3, 2, 1, GO!, TIME!, scores)

    [Header("Score display")]
    public bool showRunningTotal = true;   // true = scoreboard keeps climbing across rounds

    [Header("When the round ends")]
    public string endMessage = "TIME!";
    public bool clearTargetsAtEnd = false;   // false = leftover targets finish sliding off on their own
    public string nextSceneName;             // leave empty to stay on this scene
    public UnityEvent onRoundEnd;            // optional: hook up a cutscene or sound here

    [Header("Countdown sounds")]
    public AudioClip[] countdownSounds;      // Element 0 = "3", Element 1 = "2", Element 2 = "1"
    public AudioClip goSound;                // plays on "GO!"
    public AudioClip timeUpSound;            // plays on "TIME!"

    float timeLeft;
    bool isPlaying;
    bool endEarly;

    public bool IsPlaying => isPlaying;

    // 0 at the start of the round, 1 at the end (the spawner uses this to speed up)
    public float Progress => Mathf.Clamp01(1f - timeLeft / roundTime);

    void Start()
    {
        timeLeft = roundTime;
        StartCoroutine(RunRound());
    }

    // Another script (like the boss fight) can call this to finish the round before the timer runs out
    public void EndRoundEarly()
    {
        endEarly = true;
    }

    IEnumerator RunRound()
    {
        ScoreManager sm = ScoreManager.Instance;

        // 0) SETUP: nobody can shoot and nothing spawns yet
        SetSpawning(false);
        SetShooting(false);
        if (sm != null) sm.StartBooth();
        UpdateUI();

        // 1) "ROUND #" intro (counts up forever: round 1, 2, 3...)
        int roundNumber = (sm != null) ? sm.roundsPlayed + 1 : 1;
        ShowMessage("ROUND " + roundNumber);
        yield return new WaitForSeconds(messageTime);

        // 2) COUNTDOWN
        for (int i = countdownSeconds; i > 0; i--)
        {
            ShowMessage(i.ToString());

            int index = countdownSeconds - i;   // first number uses Element 0, and so on
            if (index < countdownSounds.Length) PlaySound(countdownSounds[index]);

            yield return new WaitForSeconds(1f);
        }
        ShowMessage("GO!");
        PlaySound(goSound);
        Invoke("ClearMessage", 1f);

        // 3) PLAYING: the clock runs until it hits zero (or EndRoundEarly is called)
        if (shooter != null) shooter.ResetShooter();
        SetShooting(true);
        SetSpawning(true);
        isPlaying = true;

        while (timeLeft > 0f && !endEarly)
        {
            timeLeft -= Time.deltaTime;
            UpdateUI();
            yield return null;
        }

        // 4) ROUND OVER: stop spawning and shooting
        isPlaying = false;
        timeLeft = Mathf.Max(0f, timeLeft);
        SetShooting(false);
        SetSpawning(false);

        if (clearTargetsAtEnd)
        {
            foreach (TargetSpawner s in spawners)
            {
                if (s != null) s.ClearAll();
            }
        }

        // grab this round's score, then add it to the running total
        int roundScore = (sm != null) ? sm.score : 0;
        if (sm != null) sm.EndBooth();
        UpdateUI();

        // "TIME!"
        ShowMessage(endMessage);
        PlaySound(timeUpSound);
        onRoundEnd.Invoke();
        yield return new WaitForSeconds(messageTime);

        // this round's score
        ShowMessage("ROUND\n" + roundScore.ToString("0000000"));
        yield return new WaitForSeconds(messageTime);

        // running total so far
        int total = (sm != null) ? sm.totalScore : roundScore;
        ShowMessage("TOTAL\n" + total.ToString("0000000"));
        yield return new WaitForSeconds(messageTime);

        // 5) NEXT SCENE
        if (!string.IsNullOrEmpty(nextSceneName)) SceneFader.FadeToScene(nextSceneName);
    }

    void SetSpawning(bool on)
    {
        foreach (TargetSpawner s in spawners)
        {
            if (s != null) s.isSpawning = on;
        }
    }

    void SetShooting(bool on)
    {
        if (shooter != null) shooter.canShoot = on;
    }

    void ShowMessage(string text)
    {
        if (messageText != null) messageText.text = text;
    }

    void ClearMessage()
    {
        ShowMessage("");
    }

    void PlaySound(AudioClip clip)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.Play(clip, 1f, 0f); // 0 = no pitch wobble
    }

    void UpdateUI()
    {
        if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString("00");

        ScoreManager sm = ScoreManager.Instance;
        if (scoreText != null && sm != null)
        {
            // running total = everything from earlier rounds + this round so far
            // (after EndBooth runs, "score" is already inside totalScore, so don't add it twice)
            int shown = showRunningTotal
                ? (isPlaying ? sm.totalScore + sm.score : sm.totalScore)
                : sm.score;
            scoreText.text = shown.ToString("0000000");
        }
    }
}