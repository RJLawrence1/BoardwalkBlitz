// BOSSBANDIT.CS  -  goes on the robbery-stage bandit (needs a Collider on his chest).
//
// IN ONE SENTENCE: the bandit has three moods. THROWING (he throws bottles, and you can't
// hurt him), DISTRACTED (a short window where a hit works), and SPLATTED (you got him).
//
// HOW IT'S BUILT: "BossBandit : Target" means it IS a Target with extra behavior (inheritance).
// The Shooter finds a Target and calls Hit(), and here Hit() is replaced with "only works
// during the distracted window". That's what "override" means.
//
// SETUP: drag the BoothManager into "Booth", the bottle-throwing TargetSpawner into
// "Throw Spawner", the ketchup splat sprite into "Splat Sprite", a "?" sprite into
// "Distracted Icon". In "On Splatted" add BanditReaction.Play.
// Set "points" (from Target) to the bonus for the splat, like 500.

using UnityEngine;
using UnityEngine.Events;

public class BossBandit : Target
{
    public enum State { Throwing, Distracted, Splatted } // "enum" = a fixed list of choices
    public State state = State.Throwing;

    [Header("Timing")]
    public float throwPhaseTime = 20f; // seconds of throwing before he gets distracted
    public float distractedTime = 2f;  // how long the window stays open

    [Header("Hookups")]
    public BoothManager booth;         // so the timers only run while the round is playing
    public TargetSpawner throwSpawner; // the thing that makes him throw (paused while distracted)
    public GameObject splatSprite;     // the ketchup stain on his chest (starts hidden)
    public GameObject distractedIcon;  // a "?" showing the window is open (starts hidden)
    public UnityEvent onSplatted;      // play the reaction, end the stage, etc.

    float timer;

    void Start()
    {
        timer = throwPhaseTime;
        if (splatSprite != null) splatSprite.SetActive(false);
        if (distractedIcon != null) distractedIcon.SetActive(false);
    }

    void Update()
    {
        if (state == State.Splatted) return;
        if (booth != null && !booth.IsPlaying) return; // wait for the round to actually start

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (state == State.Throwing)
        {
            // Open the window: stop throwing, show the "?"
            state = State.Distracted;
            timer = distractedTime;
            if (distractedIcon != null) distractedIcon.SetActive(true);
            SetThrowing(false);
        }
        else
        {
            // The player missed the window: back to throwing
            state = State.Throwing;
            timer = throwPhaseTime;
            if (distractedIcon != null) distractedIcon.SetActive(false);
            SetThrowing(true);
        }
    }

    public override void Hit()
    {
        // Only counts during the distracted window
        if (state != State.Distracted) return;

        state = State.Splatted;
        if (distractedIcon != null) distractedIcon.SetActive(false);
        if (splatSprite != null) splatSprite.SetActive(true);

        if (throwSpawner != null)
        {
            throwSpawner.isSpawning = false;
            throwSpawner.ClearAll(); // remove any bottles still in the air
        }

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPoints(points); // splat bonus

        onSplatted.Invoke();
    }

    void SetThrowing(bool on)
    {
        if (throwSpawner != null) throwSpawner.isSpawning = on;
    }
}
