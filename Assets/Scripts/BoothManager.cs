// BOOTHMANAGER.CS  -  one per booth scene. Put it on an empty GameObject called "BoothManager".
//
// IN ONE SENTENCE: runs one round from start to finish: countdown, then the timed round,
// then the "time's up" screen, then it loads the next scene.
//
// A "coroutine" (IEnumerator + yield return) is a function that can pause and pick up where
// it left off, which makes step-by-step sequences easy: 3... 2... 1... GO!... play... end.
//
// SETUP: drag the Main Camera (it has Shooter) into "Shooter", your TargetSpawner(s) into
// "Spawners", and optional TextMeshPro texts into the three text slots.
// Type the next scene's exact name into "Next Scene Name" (and add it to File > Build Settings).

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class BoothManager : MonoBehaviour
{
    [Header("Timing")]
    public int countdownSeconds = 3;   // the "3, 2, 1" before the round
    public float roundTime = 60f;      // length of the round (try 15 while testing)
    public float endDelay = 3f;        // how long the "TIME!" screen stays before moving on

    [Header("Hookups")]
    public Shooter shooter;            // the Shooter on your Main Camera
    public TargetSpawner[] spawners;   // every spawner in this booth
    public TMP_Text timerText;         // optional
    public TMP_Text scoreText;         // optional
    public TMP_Text messageText;       // optional (shows 3, 2, 1, GO!, TIME!)

    [Header("When the round ends")]
    public string endMessage = "TIME!";
    public string nextSceneName;       // leave empty to stay on this scene
    public UnityEvent onRoundEnd;      // optional: hook up a cutscene or sound here

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
        // 1) COUNTDOWN: nobody can shoot and nothing spawns yet
        SetSpawning(false);
        SetShooting(false);
        if (ScoreManager.Instance != null) ScoreManager.Instance.StartBooth();
        UpdateUI();

        for (int i = countdownSeconds; i > 0; i--)
        {
            ShowMessage(i.ToString());
            yield return new WaitForSeconds(1f);   // pause here for 1 second, then continue
        }
        ShowMessage("GO!");
        Invoke("ClearMessage", 1f);                // hide the GO! after a second

        // 2) PLAYING: the clock runs until it hits zero (or EndRoundEarly is called)
        if (shooter != null) shooter.ResetShooter();
        SetShooting(true);
        SetSpawning(true);
        isPlaying = true;

        while (timeLeft > 0f && !endEarly)
        {
            timeLeft -= Time.deltaTime;
            UpdateUI();
            yield return null;                     // wait one frame, then check again
        }

        // 3) ROUND OVER
        isPlaying = false;
        timeLeft = Mathf.Max(0f, timeLeft);
        SetShooting(false);
        SetSpawning(false);
        foreach (TargetSpawner s in spawners)
        {
            if (s != null) s.ClearAll();
        }
        if (ScoreManager.Instance != null) ScoreManager.Instance.EndBooth();
        UpdateUI();
        ShowMessage(endMessage);
        onRoundEnd.Invoke();

        yield return new WaitForSeconds(endDelay);
        if (!string.IsNullOrEmpty(nextSceneName)) SceneManager.LoadScene(nextSceneName);
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

    void UpdateUI()
    {
        if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        if (scoreText != null && ScoreManager.Instance != null)
        {
            string text = "Score: " + ScoreManager.Instance.score;
            if (ScoreManager.Instance.Multiplier > 1) text += "  x" + ScoreManager.Instance.Multiplier;
            scoreText.text = text;
        }
    }
}
