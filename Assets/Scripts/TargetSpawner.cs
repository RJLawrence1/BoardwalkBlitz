// TARGETSPAWNER.CS  -  put it on an empty GameObject near the booth (keep its Scale at 1,1,1).
//
// TWO MODES:
//   Random (Use Waves OFF) ... spawns a random target every second or so, speeding up over the round
//   Waves  (Use Waves ON) .... plays the waves you set up in the Inspector, in order
//
// SETUP:
//   - Prefabs: your target prefabs (duck, can, bottle...). Waves use these if a wave has none of its own.
//   - Spawn Points: empty GameObjects where targets appear. Each one's RED ARROW is the slide direction.
//   - Booth: drag in the BoothManager (optional, used by Random mode to speed up).

using System.Collections;
using UnityEngine;

[System.Serializable]   // makes this show up in the Inspector as a fold-out
public class Wave
{
    public string name = "Wave";
    public GameObject[] prefabs;                     // leave empty to use the spawner's main Prefabs list
    public int count = 5;                            // how many targets in this wave
    public float interval = 1f;                      // seconds between each target
    public Vector2 speedRange = new Vector2(2f, 4f); // random speed for each target
    public int spawnPointIndex = -1;                 // -1 = random lane, 0 = first spawn point, 1 = second...
    public bool allLanes = false;   // true = spawn in EVERY row, count = targets per row
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
    public float startInterval = 1.5f;
    public float endInterval = 0.7f;
    public Vector2 speedRange = new Vector2(2f, 4f);

    [Header("Optional")]
    public float laneLength = 10f;   // how far targets travel before being removed
    public BoothManager booth;

    float timer;
    bool wasSpawning;
    int waveIndex;

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

        // ---- RANDOM MODE (same as before) ----
        if (!isSpawning) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        Spawn(PickPrefab(null), PickPoint(-1), speedRange);

        float progress = (booth != null) ? booth.Progress : 0f;
        timer = Mathf.Lerp(startInterval, endInterval, progress);
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

        GameObject spawned = Instantiate(prefab, point.position, prefab.transform.rotation, transform);

        LaneMover mover = spawned.GetComponent<LaneMover>();
        if (mover != null)
        {
            mover.direction = point.right;
            mover.speed = Random.Range(speeds.x, speeds.y);
            mover.travelDistance = laneLength;   // ← add this line
        }
    }

    public void ClearAll()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}