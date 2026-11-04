// TARGETSPAWNER.CS  -  put it on an empty GameObject near the booth (keep its Scale at 1,1,1).
//
// TWO MODES:
//   Random (Use Waves OFF) ... spawns targets on a timer, speeding up over the round
//                              - Every Lane ON  = every lane spawns on its own timer, all at once
//                              - Every Lane OFF = one target at a time in a random lane
//   Waves  (Use Waves ON) .... plays the waves you set up in the Inspector, in order
//
// SETUP:
//   - Prefabs: your target prefabs (duck, bullseye...). Waves use these if a wave has none of its own.
//   - Spawn Points: empty GameObjects where targets appear. Each one's RED ARROW is the slide direction.
//   - Lane Length: how far targets travel before they're removed (distance between your spawn points).
//   - Booth: drag in the BoothManager (optional, makes random mode speed up over the round).

using System.Collections;
using UnityEngine;

[System.Serializable]   // makes this show up in the Inspector as a fold-out
public class Wave
{
    public string name = "Wave";
    public GameObject[] prefabs;                     // leave empty to use the spawner's main Prefabs list
    public int count = 5;                            // how many targets (per row if All Lanes is on)
    public float interval = 1f;                      // seconds between each target
    public Vector2 speedRange = new Vector2(2f, 4f); // one speed is picked from this for the whole wave
    public int spawnPointIndex = -1;                 // -1 = random lane, 0 = first spawn point, 1 = second...
    public bool allLanes = false;                    // true = spawn in EVERY row at once
    public float pauseAfter = 2f;                    // breather before the next wave
}

public class TargetSpawner : MonoBehaviour
{
    [Header("What to spawn and where")]
    public GameObject[] prefabs;
    public Transform[] spawnPoints;

    [Header("Mode")]
    public bool isSpawning = true;    // BoothManager turns this on and off
    public bool useWaves = false;     // tick to use the Waves list below

    [Header("Waves")]
    public Wave[] waves;
    public bool loopWaves = true;     // after the last wave, start again from wave 1

    [Header("Random mode timing")]
    public float startInterval = 1.5f;               // seconds between spawns at the start of the round
    public float endInterval = 0.7f;                 // seconds between spawns at the end (smaller = faster)
    public Vector2 speedRange = new Vector2(2.5f, 2.5f); // keep both the same so nothing catches up
    public bool everyLane = true;                    // true = every lane spawns on its own timer, all at once
    public float laneJitter = 0.4f;                  // random extra wait so lanes don't spawn in sync

    [Header("Optional")]
    public BoothManager booth;        // lets random mode speed up as the round goes on
    public float laneLength = 10f;    // how far targets travel before being removed

    float timer;
    bool wasSpawning;
    int waveIndex;
    float[] laneTimers;               // one countdown per lane (Every Lane mode)

    // Other scripts (like the UI) can read this to show "Wave 3"
    public int CurrentWave => waveIndex + 1;

    void Start()
    {
        timer = startInterval;
    }

    void Update()
    {
        // ---- WAVE MODE ----
        if (useWaves)
        {
            // The moment spawning gets switched on (start of a round), begin from wave 1
            if (isSpawning && !wasSpawning) RestartWaves();
            wasSpawning = isSpawning;
            return;
        }

        // ---- RANDOM MODE ----
        if (!isSpawning) return;

        // Slides from startInterval to endInterval as the round goes on (0 -> 1)
        float progress = (booth != null) ? booth.Progress : 0f;
        float interval = Mathf.Lerp(startInterval, endInterval, progress);

        if (everyLane)
        {
            // make one timer per lane the first time (staggered start so they don't all spawn together)
            if (laneTimers == null || laneTimers.Length != spawnPoints.Length)
            {
                laneTimers = new float[spawnPoints.Length];
                for (int i = 0; i < laneTimers.Length; i++)
                    laneTimers[i] = Random.Range(0f, interval);
            }

            // count down each lane separately and spawn when its timer runs out
            for (int i = 0; i < laneTimers.Length; i++)
            {
                laneTimers[i] -= Time.deltaTime;
                if (laneTimers[i] <= 0f)
                {
                    Spawn(PickPrefab(null), spawnPoints[i], speedRange);
                    laneTimers[i] = interval + Random.Range(0f, laneJitter);
                }
            }
            return;
        }

        // one target at a time in a random lane
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        Spawn(PickPrefab(null), PickPoint(-1), speedRange);
        timer = interval;
    }

    public void RestartWaves()
    {
        StopAllCoroutines();
        waveIndex = 0;
        if (waves.Length > 0) StartCoroutine(RunWaves());
    }

    // A "coroutine" can pause partway through with "yield", which makes timed sequences easy
    IEnumerator RunWaves()
    {
        while (true)
        {
            Wave w = waves[waveIndex];

            // pick ONE speed for the whole wave, so nothing in a row catches up to anything else
            float waveSpeed = Random.Range(w.speedRange.x, w.speedRange.y);
            Vector2 speeds = new Vector2(waveSpeed, waveSpeed);

            for (int i = 0; i < w.count; i++)
            {
                if (w.allLanes)
                {
                    // one target per row, spread out so the rows don't line up in columns
                    for (int lane = 0; lane < spawnPoints.Length; lane++)
                    {
                        if (!isSpawning) yield break;
                        Spawn(PickPrefab(w.prefabs), spawnPoints[lane], speeds);
                        yield return new WaitForSeconds(w.interval / spawnPoints.Length);
                    }
                }
                else
                {
                    if (!isSpawning) yield break;
                    Spawn(PickPrefab(w.prefabs), PickPoint(w.spawnPointIndex), speeds);
                    yield return new WaitForSeconds(w.interval);
                }
            }

            yield return new WaitForSeconds(w.pauseAfter);

            waveIndex++;
            if (waveIndex >= waves.Length)
            {
                if (loopWaves) waveIndex = 0;
                else yield break;
            }
        }
    }

    GameObject PickPrefab(GameObject[] list)
    {
        if (list == null || list.Length == 0) list = prefabs;   // fall back to the main list
        if (list.Length == 0) return null;
        return list[Random.Range(0, list.Length)];
    }

    Transform PickPoint(int index)
    {
        if (spawnPoints.Length == 0) return null;
        if (index >= 0 && index < spawnPoints.Length) return spawnPoints[index];
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    void Spawn(GameObject prefab, Transform point, Vector2 speeds)
    {
        if (prefab == null || point == null) return;

        // Create it at the spawn point, keeping the prefab's own rotation, as a child of this object
        GameObject spawned = Instantiate(prefab, point.position, prefab.transform.rotation, transform);

        // If it slides, tell it which way, how fast, and how far to go
        LaneMover mover = spawned.GetComponent<LaneMover>();
        if (mover != null)
        {
            mover.direction = point.right;
            mover.speed = Random.Range(speeds.x, speeds.y);
            mover.travelDistance = laneLength;
        }
    }

    // Removes every target this spawner made (BoothManager calls this when the round ends)
    public void ClearAll()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}