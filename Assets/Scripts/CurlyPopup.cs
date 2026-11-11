// CURLYPOPUP.CS  -  put it on Curly (a sprite standing in the booth).
//
// IN ONE SENTENCE: pops Curly up from behind a shelf with a bouncy spring, bobs him a little
// while he talks, and ducks him back down out of sight.
//
// SETUP: place Curly in the editor exactly where he should be when he's UP (talking).
// The script hides him "Hidden Drop" units lower when the game starts.
// Put him between two rows, so the front shelf covers him when he's down.

using System.Collections;
using UnityEngine;

public class CurlyPopup : MonoBehaviour
{
    [Header("Movement")]
    public float hiddenDrop = 2f;     // how far down he hides (enough to be fully behind the shelf)
    public float popTime = 0.35f;     // how long popping up takes
    public float duckTime = 0.25f;    // how long ducking down takes

    [Header("While talking")]
    public bool bob = true;
    public float bobAmount = 0.05f;   // little up/down bounce while he talks
    public float bobSpeed = 6f;

    [Header("Sounds")]
    public AudioClip popSound;        // a "boing" or whoosh (optional)
    public AudioClip duckSound;       // optional

    Vector3 upPos;
    Vector3 downPos;
    bool isUp;
    bool moving;

    void Awake()
    {
        upPos = transform.localPosition;              // where you placed him = his "up" spot
        downPos = upPos + Vector3.down * hiddenDrop;  // hidden spot below
        transform.localPosition = downPos;            // start hidden
    }

    void Update()
    {
        // bob gently while he's up and not moving
        if (isUp && !moving && bob)
        {
            transform.localPosition = upPos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmount);
        }
    }

    // BoothManager waits on these: "yield return curly.PopUp();"
    public IEnumerator PopUp()
    {
        PlaySound(popSound);
        moving = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / popTime;
            // "ease out back" = shoots up, overshoots a little, settles: a springy pop
            transform.localPosition = Vector3.LerpUnclamped(downPos, upPos, EaseOutBack(Mathf.Clamp01(t)));
            yield return null;
        }

        transform.localPosition = upPos;
        moving = false;
        isUp = true;
    }

    public IEnumerator Duck()
    {
        PlaySound(duckSound);
        isUp = false;
        moving = true;

        Vector3 start = transform.localPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duckTime;
            float p = Mathf.Clamp01(t);
            // "ease in" = starts slow, drops fast, like ducking out of sight
            transform.localPosition = Vector3.Lerp(start, downPos, p * p);
            yield return null;
        }

        transform.localPosition = downPos;
        moving = false;
    }

    // the springy curve: goes a bit past 1 then settles back to 1
    float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && SoundManager.Instance != null) SoundManager.Instance.Play(clip);
    }
}