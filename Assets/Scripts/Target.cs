// TARGET.CS  -  goes on every shootable thing (ducks, cans, aliens, bottles...).
// It needs a Collider on the same object (Unity adds one for you if it's missing).
//
// IN ONE SENTENCE: when the Shooter says "you got hit", this gives (or takes) points and
// flips the target down.
//
// THE INSPECTOR SETTINGS MAKE DIFFERENT TARGET TYPES - no new scripts needed:
//   Normal target ..... points = 10
//   Bonus target ...... points = 50
//   Don't-shoot ....... tick "dontShoot" (you LOSE points and your combo breaks)
//   Tough target ...... hitsToKill = 2 (like a bandit with a steel plate)
//   Thrown bottle ..... untick "flipDown" so it just disappears
//
// "virtual" on Hit() means other scripts (like BossBandit) can swap in their own version.

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour
{
    [Header("Scoring")]
    public int points = 10;         // base points (use bigger numbers for bonus targets)
    public bool dontShoot = false;  // true = shooting it LOSES points and breaks your combo

    [Header("Toughness")]
    public int hitsToKill = 1;      // 2 or more = takes several hits

    [Header("Hit reaction")]
    public bool flipDown = true;    // false = just disappear (good for thrown bottles)
    public float flipTime = 0.25f;  // how long the flip takes (seconds)

    int hitsLeft;
    bool isDown;
    float flipProgress;             // goes from 0 to 1 while flipping
    Quaternion startRotation;
    Quaternion downRotation;

    void Awake()
    {
        hitsLeft = hitsToKill;
        startRotation = transform.rotation;
        // The "down" pose is the start pose tipped back 90 degrees (change -90 to 90 if it flips the wrong way)
        downRotation = startRotation * Quaternion.Euler(-90f, 0f, 0f);
    }

    void Update()
    {
        if (!isDown) return;

        // Slide smoothly from standing to flipped down, then remove the target
        flipProgress += Time.deltaTime / flipTime;
        transform.rotation = Quaternion.Slerp(startRotation, downRotation, flipProgress);

        if (flipProgress >= 1f) Destroy(gameObject);
    }

    public virtual void Hit()
    {
        if (isDown) return;           // already flipped, ignore extra shots

        hitsLeft--;
        if (hitsLeft > 0) return;     // tough target: still standing (later, play a "clang" sound here)

        // Don't-shoot targets turn the points negative
        int value = dontShoot ? -Mathf.Abs(points) : points;
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPoints(value);

        if (flipDown)
        {
            isDown = true;
            GetComponent<Collider>().enabled = false; // so you can't shoot it twice
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
