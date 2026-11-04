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
    public int hitsToKill = 1;

    [Header("Sound")]
    public AudioClip[] hitSounds;       // every sound in this list plays on a hit

    [Header("Hit reaction")]
    public bool stayUp = false;
    public float spinTime = 0.3f;
    public bool flipDown = true;
    public float flipTime = 0.25f;

    int hitsLeft;
    bool isDown;
    float flipProgress;
    float spinLeft;
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
        if (spinLeft > 0f)
        {
            float step = Mathf.Min(360f / spinTime * Time.deltaTime, spinLeft);
            transform.Rotate(0f, step, 0f, Space.World);
            spinLeft -= step;
        }

        if (!isDown) return;
        flipProgress += Time.deltaTime / flipTime;
        transform.rotation = Quaternion.Slerp(startRotation, downRotation, flipProgress);
        if (flipProgress >= 1f) Destroy(gameObject);
    }

    public virtual void Hit()
    {
        if (isDown) return;

        if (SoundManager.Instance != null)
        {
            foreach (AudioClip clip in hitSounds)
                SoundManager.Instance.Play(clip);
        }

        if (stayUp)
        {
            GivePoints();
            spinLeft = Mathf.Min(spinLeft + 360f, 720f);
            return;
        }

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