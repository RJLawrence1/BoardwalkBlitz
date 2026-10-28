// POPUPTARGET.CS  -  put it on any target that should POP UP from behind cover instead of sliding
// (bandits behind barrels, ducks behind the counter, aliens from a hatch...).
// It works together with the Target script, so points, flip-down, etc. all still work.
//
// IN ONE SENTENCE: the target appears at the spawn point, but starts hidden below it,
// rises up, stays for a moment, drops back down, and then removes itself.
//
// HOW TO MIX MOVING AND POP-UP TARGETS (no extra code needed):
//   - Use one TargetSpawner for SLIDING targets (prefabs with LaneMover).
//   - Use a second TargetSpawner for POP-UP targets (prefabs with PopUpTarget).
//   - Put the pop-up spawn points where the target should be fully UP, and place a counter,
//     barrel, or wall in front of them so the hidden part is covered while it rises.
//   - Put both spawners in the BoothManager's "Spawners" list.

using UnityEngine;

public class PopUpTarget : MonoBehaviour
{
    public Vector3 hiddenOffset = new Vector3(0f, -1.5f, 0f); // how far down it starts (must be hidden behind cover)
    public float riseTime = 0.15f;                            // seconds to pop up
    public Vector2 stayTimeRange = new Vector2(1f, 2f);       // it stays up a random time in this range
    public float dropTime = 0.15f;                            // seconds to drop back down

    enum Phase { Rising, Staying, Dropping } // the three steps, in order
    Phase phase = Phase.Rising;

    float timer;
    float stayTime;
    Vector3 upPosition;
    Vector3 downPosition;

    // Awake runs the instant the target is created, so it never flashes at the "up" spot for a frame
    void Awake()
    {
        upPosition = transform.position;
        downPosition = upPosition + hiddenOffset;
        transform.position = downPosition;
        stayTime = Random.Range(stayTimeRange.x, stayTimeRange.y);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (phase == Phase.Rising)
        {
            // t goes from 0 to 1 as the rise finishes; Lerp slides between the two positions
            float t = Mathf.Clamp01(timer / riseTime);
            transform.position = Vector3.Lerp(downPosition, upPosition, t);
            if (t >= 1f)
            {
                phase = Phase.Staying;
                timer = 0f;
            }
        }
        else if (phase == Phase.Staying)
        {
            if (timer >= stayTime)
            {
                phase = Phase.Dropping;
                timer = 0f;
            }
        }
        else
        {
            float t = Mathf.Clamp01(timer / dropTime);
            transform.position = Vector3.Lerp(upPosition, downPosition, t);
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
