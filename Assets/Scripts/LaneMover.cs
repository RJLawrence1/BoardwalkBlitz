// LANEMOVER.CS  -  put it on any target that should slide (ducks, aliens, cans, drinks...).
//
// IN ONE SENTENCE: moves the target along its lane using the Path you pick in the Inspector,
// then removes it once it has traveled "travelDistance" (or after "lifetime" seconds).
//
// PATHS:
//   Straight ... plain slide (ducks, bullseyes)
//   Wave ....... smooth up-and-down bob
//   Zigzag ..... sharp up-and-down
//   Step ....... little jumps with pauses, like old arcade invaders
//   Sway ....... drifts slightly forward and back along the lane
//   Wander ..... a little of both: gentle up/down AND forward/back, slightly random
//
// AUTO LIMITS: the spawner fills in Max Height and Max Sway so targets never leave their row
// or bump into each other. Your Height / Sway Amount are used, but never more than those limits.

using UnityEngine;

public class LaneMover : MonoBehaviour
{
    public enum PathType { Straight, Wave, Zigzag, Step, Sway, Wander }

    [Header("Lane (set by the spawner)")]
    public Vector3 direction = Vector3.right;
    public float speed = 3f;
    public float travelDistance = 0f;   // delete after going this far (0 = use lifetime instead)
    public float lifetime = 6f;         // backup timer, only used when travelDistance is 0

    [Header("Path")]
    public PathType path = PathType.Straight;
    public float height = 0.15f;        // up/down amount (Wave, Zigzag, Wander)
    public float frequency = 1.5f;      // up/down speed
    public float swayAmount = 0.3f;     // forward/back amount (Sway, Wander)
    public float swaySpeed = 1.2f;      // forward/back speed
    public float stepTime = 0.4f;       // Step: seconds between jumps
    public bool randomStart = true;     // so targets don't all move in perfect sync

    [Header("Auto limits (filled in by the spawner)")]
    public float maxHeight = 999f;      // most it may go up/down without leaving its row
    public float maxSway = 999f;        // most it may drift without reaching a neighbor

    Vector3 startPos;
    Vector3 lanePos;       // position along the lane, ignoring the wiggle
    float phase;
    float noiseSeed;
    float stepTimer;
    float age;

    void Start()
    {
        startPos = transform.position;
        lanePos = startPos;
        phase = randomStart ? Random.Range(0f, 10f) : 0f;
        noiseSeed = Random.Range(0f, 100f);
    }

    void Update()
    {
        age += Time.deltaTime;
        Vector3 dir = direction.normalized;

        // 1) MOVE ALONG THE LANE
        if (path == PathType.Step)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepTime)
            {
                stepTimer = 0f;
                lanePos += dir * speed * stepTime;   // jump forward, same average speed as sliding
            }
        }
        else
        {
            lanePos += dir * speed * Time.deltaTime;
        }

        // 2) WIGGLE (never more than the auto limits)
        float h = Mathf.Min(height, maxHeight);
        float s = Mathf.Min(swayAmount, maxSway);
        float t = age * frequency + phase;

        float up = 0f;     // up/down
        float side = 0f;   // forward/back along the lane

        switch (path)
        {
            case PathType.Wave:
                up = Mathf.Sin(t * Mathf.PI) * h;
                break;

            case PathType.Zigzag:
                up = (Mathf.PingPong(t * 2f, 2f) - 1f) * h;
                break;

            case PathType.Sway:
                side = Mathf.Sin(age * swaySpeed + phase) * s;
                break;

            case PathType.Wander:
                // Perlin noise = smooth randomness, so it drifts naturally instead of in a perfect pattern
                up = (Mathf.PerlinNoise(noiseSeed, age * frequency) * 2f - 1f) * h;
                side = (Mathf.PerlinNoise(noiseSeed + 50f, age * swaySpeed) * 2f - 1f) * s;
                break;
        }

        transform.position = lanePos + Vector3.up * up + dir * side;

        // 3) REMOVE IT AT THE END OF THE LANE (based on the lane position, not the wiggle)
        if (travelDistance > 0f)
        {
            if (Vector3.Distance(startPos, lanePos) >= travelDistance) Destroy(gameObject);
        }
        else
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) Destroy(gameObject);
        }
    }
}