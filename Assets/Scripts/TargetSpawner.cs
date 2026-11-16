// TARGETSPAWNER.CS  -  put it on an empty GameObject near the booth (keep its Scale at 1,1,1).
//
// TWO MODES:
//   Random (Use Waves OFF) ... spawns targets on a timer, speeding up over the round
//                              - Every Lane ON  = every lane spawns on its own timer, all at once
//                              - Every Lane OFF = one target at a time in a random lane
//   Waves  (Use Waves ON) .... plays the waves you set up in the Inspector, in order
//
// CHANCES: fill "Chances" in the same order as "Prefabs" (e.g. 65 and 35) to make some targets
// more common than others. Leave it empty for equal odds.
//
// AUTO LIMITS: when a target spawns, the spawner works out how much room it has
// (Vertical Room for up/down, and the gap to its neighbors for forward/back)
// and tells the target's LaneMover, so wiggling targets never leave their row or collide.

using System.Collections;
using UnityEngine;

[System.Serializable]
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
    public float[] chances;           // same order as Prefabs, e.g. 65 and 35 (don't have to add up to 100)
    public Transform[] spawnPoints;

    [Header("Mode")]
    public bool isSpawning = true;    // BoothManager turns this on and off
    public bool useWaves = false;

    [Header("Waves")]
    public Wave[] waves;
    public bool loopWaves = true;

    [Header("Random mode timing")]
    public float startInterval = 1.5f;
    public float endInterval = 0.7f;
    public Vector2 speedRange = new Vector2(2.5f, 2.5f); // keep both the same so nothing catches up
    public bool everyLane = true;
    public float laneJitter = 0.4f;

    [Header("Auto limits")]
    public float verticalRoom = 0.2f;   // most a target may move up/down without leaving its row
    public float minGap = 0.1f;         // breathing room kept between neighbors

    [Header("Optional")]
    public BoothManager booth;
    public float laneLength = 10f;      // how far targets travel before being removed

    float timer;
    bool wasSpawning;
    int waveIndex;
    float[] laneTimers;

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
            if (isSpawning && !wasSpawning) RestartWaves();
            wasSpawning = isSpawning;
            return;
        }

        // ---- RANDOM MODE ----
        if (!isSpawning) return;

        float progress = (booth != null) ? booth.Progress : 0f;
        float interval = Mathf.Lerp(startInterval, endInterval, progress);

        if (everyLane)
        {
            if (laneTimers == null || laneTimers.Length != spawnPoints.Length)
            {
                laneTimers = new float[spawnPoints.Length];
                for (int i = 0; i < laneTimers.Length; i++)
                    laneTimers[i] = Random.Range(0f, interval);
            }

            for (int i = 0; i < laneTimers.Length; i++)
            {
                laneTimers[i] -= Time.deltaTime;
                if (laneTimers[i] <= 0f)
                {
                    // endInterval = the closest targets can ever get, so sway is always safe
                    Spawn(PickPrefab(null), spawnPoints[i], speedRange, endInterval);
                    laneTimers[i] = interval + Random.Range(0f, laneJitter);
                }
            }
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        Spawn(PickPrefab(null), PickPoint(-1), speedRange, endInterval);
        timer = interval;
    }

    public void RestartWaves()
    {
        StopAllCoroutines();
        waveIndex = 0;
        if (waves.Length > 0) StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        while (true)
        {
            Wave w = waves[waveIndex];

            float waveSpeed = Random.Range(w.speedRange.x, w.speedRange.y);
            Vector2 speeds = new Vector2(waveSpeed, waveSpeed);

            for (int i = 0; i < w.count; i++)
            {
                if (w.allLanes)
                {
                    for (int lane = 0; lane < spawnPoints.Length; lane++)
                    {
                        if (!isSpawning) yield break;
                        Spawn(PickPrefab(w.prefabs), spawnPoints[lane], speeds, w.interval);
                        yield return new WaitForSeconds(w.interval / spawnPoints.Length);
                    }
                }
                else
                {
                    if (!isSpawning) yield break;
                    Spawn(PickPrefab(w.prefabs), PickPoint(w.spawnPointIndex), speeds, w.interval);
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
        bool usingMainList = (list == null || list.Length == 0);
        if (usingMainList) list = prefabs;
        if (list.Length == 0) return null;

        // use the Chances list if it matches the Prefabs list
        if (usingMainList && chances != null && chances.Length == list.Length)
        {
            float total = 0f;
            foreach (float c in chances) total += Mathf.Max(0f, c);

            if (total > 0f)
            {
                // roll a random number along the total, then see whose "slice" it landed in
                float roll = Random.value * total;
                for (int i = 0; i < list.Length; i++)
                {
                    roll -= Mathf.Max(0f, chances[i]);
                    if (roll <= 0f) return list[i];
                }
                return list[list.Length - 1];
            }
        }

        // no chances set: every prefab is equally likely
        return list[Random.Range(0, list.Length)];
    }

    Transform PickPoint(int index)
    {
        if (spawnPoints.Length == 0) return null;
        if (index >= 0 && index < spawnPoints.Length) return spawnPoints[index];
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    // "interval" = the shortest time between targets in this lane, used to work out the safe sway
    void Spawn(GameObject prefab, Transform point, Vector2 speeds, float interval)
    {
        if (prefab == null || point == null) return;

        GameObject spawned = Instantiate(prefab, point.position, prefab.transform.rotation, transform);

        LaneMover mover = spawned.GetComponent<LaneMover>();
        if (mover != null)
        {
            mover.direction = point.right;
            mover.speed = Random.Range(speeds.x, speeds.y);
            mover.travelDistance = laneLength;

            // AUTO LIMITS
            mover.maxHeight = verticalRoom;

            float width = TargetWidth(spawned);
            float spacing = mover.speed * interval;              // distance between neighbors in this lane
            float gap = spacing - width - minGap;                // empty space between them
            mover.maxSway = Mathf.Max(0f, gap / 2f);             // half each, so two neighbors can never meet
        }
    }

    // measures how wide the target is from left to right, using all its visible parts
    float TargetWidth(GameObject obj)
    {
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return 1f;

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b.size.x;
    }

    public void ClearAll()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Target>() != null) Destroy(child.gameObject);
        }
    }
}