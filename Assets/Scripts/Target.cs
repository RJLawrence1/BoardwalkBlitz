// TARGET.CS  -  goes on every shootable thing (ducks, cans, aliens, bottles...).
// It needs a Collider on the same object (Unity adds one for you if it's missing).
//
// TARGET TYPES (all Inspector settings):
//   Gallery target .... tick "stayUp" - never goes away, every hit = points + a spin
//   Normal target ..... points = 10
//   Bonus target ...... points = 50
//   Don't-shoot ....... tick "dontShoot" (you LOSE points and your combo breaks)
//   Tough target ...... hitsToKill = 2 (like a bandit with a steel plate)
//   Thrown bottle ..... untick "flipDown" so it just disappears

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour
{
    [Header("Scoring")]
    public int points = 10;
    public bool dontShoot = false;

    [Header("Toughness")]
    public int hitsToKill = 1;      // ignored when stayUp is on

    [Header("Hit reaction")]
    public bool stayUp = false;     // true = can be hit forever, spins on every hit
    public float spinTime = 0.3f;   // how long one full spin takes
    public bool flipDown = true;    // only used when stayUp is OFF
    public float flipTime = 0.25f;

    int hitsLeft;
    bool isDown;
    float flipProgress;
    float spinLeft;                 // how many degrees of spinning are still left to do
    Quaternion startRotation;
    Quaternion downRotation;

    void Awake()
    {
        hitsLeft = hitsToKill;
        startRotation = transform.rotation;
        downRotation = startRotation * Quaternion.Euler(-90f, 0f, 0f);
    }

    void Update()
    {
        // SPIN: turn a little each frame until the spin is used up
        if (spinLeft > 0f)
        {
            float step = Mathf.Min(360f / spinTime * Time.deltaTime, spinLeft);
            transform.Rotate(0f, step, 0f, Space.World);
            spinLeft -= step;
        }

        // FLIP DOWN (only for targets that don't stay up)
        if (!isDown) return;
        flipProgress += Time.deltaTime / flipTime;
        transform.rotation = Quaternion.Slerp(startRotation, downRotation, flipProgress);
        if (flipProgress >= 1f) Destroy(gameObject);
    }

    public virtual void Hit()
    {
        if (isDown) return;

        // GALLERY TARGET: score every single hit and spin
        if (stayUp)
        {
            GivePoints();
            spinLeft = Mathf.Min(spinLeft + 360f, 720f); // hits stack spins, max 2 queued
            return;
        }

        // EVERYTHING ELSE: works exactly like before
        hitsLeft--;
        if (hitsLeft > 0) return;

        GivePoints();

        if (flipDown)
        {
            isDown = true;
            GetComponent<Collider>().enabled = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void GivePoints()
    {
        int value = dontShoot ? -Mathf.Abs(points) : points;
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPoints(value);
    }
}