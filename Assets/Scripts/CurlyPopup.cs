// CURLYPOPUP.CS  -  put it on Curly (the object with his Sprite Renderer).
//
// IN ONE SENTENCE: hides Curly below the shelf when the game starts, slides him up into view
// when he talks, bobs him a little while he's up, and ducks him back down quickly.
//
// SETUP: place Curly in the editor exactly where he should be when he's UP (talking).
// The script hides him "Hidden Drop" units lower when the game starts.
// Put him between two rows, so the shelf in front covers him when he's down.

using System.Collections;
using UnityEngine;

public class CurlyPopup : MonoBehaviour
{
    [Header("Movement")]
    public float hiddenDrop = 1f;     // how far down he hides (enough to be fully behind the shelf)
    public float popTime = 0.5f;      // how long sliding up takes (bigger = slower, smoother)
    public float duckTime = 0.15f;    // how long ducking down takes (small = quick)
    public bool bounce = false;       // true = springy overshoot at the top, false = smooth slide

    [Header("While talking")]
    public bool bob = true;
    public float bobAmount = 0.02f;   // little up/down bounce while he talks
    public float bobSpeed = 4f;

    [Header("Sounds")]
    public AudioClip popSound;        // a whoosh or "boing" as he comes up (optional)
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

    // BoothManager waits on these: "yield return popup.PopUp();"
    public IEnumerator PopUp()
    {
        PlaySound(popSound);
        moving = true;

        Vector3 start = transform.localPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / popTime;
            float p = Mathf.Clamp01(t);
            float e = bounce ? EaseOutBack(p) : EaseOutCubic(p);
            transform.localPosition = Vector3.LerpUnclamped(start, upPos, e);
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
            // "ease in": starts slow then drops fast, like ducking out of sight
            transform.localPosition = Vector3.Lerp(start, downPos, p * p);
            yield return null;
        }

        transform.localPosition = downPos;
        moving = false;
    }

    // smooth slide: fast at first, gently settles at the top (no overshoot)
    float EaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    // springy pop: goes a bit past the top, then settles back
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