// BOOTHMANAGER.CS  -  one per booth scene. Put it on an empty GameObject called "BoothManager".
//
// IN ONE SENTENCE: runs one round: Curly pops up and does his intro, ducks down, "ROUND #",
// countdown, the timed round, "TIME!", round score, running total, Curly pops up for his outro,
// then the next scene. Works for any number of booths in a row.

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class BoothManager : MonoBehaviour
{
    [Header("Timing")]
    public int countdownSeconds = 3;
    public float roundTime = 60f;      // try 15 while testing
    public float messageTime = 1.5f;   // how long ROUND #, TIME! and score screens stay up

    [Header("Hookups")]
    public Shooter shooter;
    public TargetSpawner[] spawners;
    public TMP_Text timerText;
    public TMP_Text scoreText;
    public TMP_Text messageText;       // use DSEG14 so words display properly

    [Header("Dialogue")]
    public DialogueBox dialogue;       // optional
    public CurlyPopup curly;           // Curly in the booth (optional)
    public DialogueLine[] introLines;  // Curly before the round
    public DialogueLine[] outroLines;  // Curly after the scores
    public bool duckAfterOutro = false;// true = Curly ducks before the scene fades

    [Header("Score display")]
    public bool showRunningTotal = true;

    [Header("When the round ends")]
    public string endMessage = "TIME!";
    public bool clearTargetsAtEnd = false;
    public string nextSceneName;
    public UnityEvent onRoundEnd;

    [Header("Countdown sounds")]
    public AudioClip[] countdownSounds;   // Element 0 = "3", 1 = "2", 2 = "1"
    public AudioClip goSound;
    public AudioClip timeUpSound;

    float timeLeft;
    bool isPlaying;
    bool endEarly;

    public bool IsPlaying => isPlaying;
    public float Progress => Mathf.Clamp01(1f - timeLeft / roundTime);   // 0 at start, 1 at end

    void Start()
    {
        timeLeft = roundTime;
        StartCoroutine(RunRound());
    }

    public void EndRoundEarly()
    {
        endEarly = true;
    }

    IEnumerator RunRound()
    {
        ScoreManager sm = ScoreManager.Instance;

        // SETUP
        SetSpawning(false);
        SetShooting(false);
        if (sm != null) sm.StartBooth();
        UpdateUI();
        yield return new WaitForSeconds(0.5f);   // let the fade-in finish

        // CURLY'S INTRO: pop up, talk, duck down
        if (curly != null) yield return curly.PopUp();
        if (dialogue != null) yield return dialogue.Play(introLines);
        if (curly != null) yield return curly.Duck();

        // ROUND #
        int roundNumber = (sm != null) ? sm.roundsPlayed + 1 : 1;
        ShowMessage("ROUND " + roundNumber);
        yield return new WaitForSeconds(messageTime);

        // COUNTDOWN
        for (int i = countdownSeconds; i > 0; i--)
        {
            ShowMessage(i.ToString());
            int index = countdownSeconds - i;
            if (index < countdownSounds.Length) PlaySound(countdownSounds[index]);
            yield return new WaitForSeconds(1f);
        }
        ShowMessage("GO!");
        PlaySound(goSound);
        Invoke("ClearMessage", 1f);

        // PLAYING
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

        // ROUND OVER
        isPlaying = false;
        timeLeft = Mathf.Max(0f, timeLeft);
        SetShooting(false);
        SetSpawning(false);

        if (clearTargetsAtEnd)
        {
            foreach (TargetSpawner s in spawners)
                if (s != null) s.ClearAll();
        }

        int roundScore = (sm != null) ? sm.score : 0;
        if (sm != null) sm.EndBooth();
        UpdateUI();

        ShowMessage(endMessage);
        PlaySound(timeUpSound);
        onRoundEnd.Invoke();
        yield return new WaitForSeconds(messageTime);

        ShowMessage("ROUND\n" + roundScore.ToString("0000000"));
        yield return new WaitForSeconds(messageTime);

        int total = (sm != null) ? sm.totalScore : roundScore;
        ShowMessage("TOTAL\n" + total.ToString("0000000"));
        yield return new WaitForSeconds(messageTime);
        ClearMessage();

        // CURLY'S OUTRO: pop up and talk
        if (curly != null) yield return curly.PopUp();
        if (dialogue != null) yield return dialogue.Play(outroLines);
        if (curly != null && duckAfterOutro) yield return curly.Duck();

        // NEXT SCENE
        if (!string.IsNullOrEmpty(nextSceneName)) SceneFader.FadeToScene(nextSceneName);
    }

    void SetSpawning(bool on)
    {
        foreach (TargetSpawner s in spawners)
            if (s != null) s.isSpawning = on;
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
        if (SoundManager.Instance != null) SoundManager.Instance.Play(clip, 1f, 0f);
    }

    void UpdateUI()
    {
        if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString("00");

        ScoreManager sm = ScoreManager.Instance;
        if (scoreText != null && sm != null)
        {
            // after EndBooth, "score" is already inside totalScore, so don't add it twice
            int shown = showRunningTotal
                ? (isPlaying ? sm.totalScore + sm.score : sm.totalScore)
                : sm.score;
            scoreText.text = shown.ToString("0000000");
        }
    }
}