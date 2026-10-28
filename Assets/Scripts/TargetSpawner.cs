// TARGETSPAWNER.CS  -  put it on an empty GameObject near the booth (keep its Scale at 1,1,1).
//
// IN ONE SENTENCE: every second or so it creates a random target prefab at a random spawn
// point, and gets faster as the round goes on.
//
// SETUP:
//   - Prefabs: drag in your target prefabs (duck, can, bottle...).
//   - Spawn Points: make empty GameObjects where targets should appear. Each spawn point's
//     RED ARROW (its "right" direction) is the way targets will slide, so rotate it to aim the lane.
//   - Booth: drag in the BoothManager so spawning can speed up over the round (optional).
//
// All spawned targets become children of this object, so ClearAll() can wipe them at round end.

using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [Header("What to spawn and where")]
    public GameObject[] prefabs;      // targets it can spawn (picks one at random)
    public Transform[] spawnPoints;   // places they appear (picks one at random)

    [Header("Timing")]
    public float startInterval = 1.5f; // seconds between spawns at the start of the round
    public float endInterval = 0.7f;   // seconds between spawns at the end (smaller = faster)
    public bool isSpawning = true;     // BoothManager turns this on and off

    [Header("Movement")]
    public Vector2 speedRange = new Vector2(2f, 4f); // each target gets a random speed in this range

    [Header("Optional")]
    public BoothManager booth;         // lets spawning speed up as the round goes on

    float timer;

    void Start()
    {
        timer = startInterval;
    }

    void Update()
    {
        if (!isSpawning) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        SpawnOne();

        // Pick the next wait: slides from startInterval to endInterval as the round progresses (0 -> 1)
        float progress = (booth != null) ? booth.Progress : 0f;
        timer = Mathf.Lerp(startInterval, endInterval, progress);
    }

    void SpawnOne()
    {
        if (prefabs.Length == 0 || spawnPoints.Length == 0) return;

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Create it at the spawn point, keeping the prefab's own rotation, as a child of this object
        GameObject spawned = Instantiate(prefab, point.position, prefab.transform.rotation, transform);

        // If it slides, tell it which way and how fast
        LaneMover mover = spawned.GetComponent<LaneMover>();
        if (mover != null)
        {
            mover.direction = point.right;
            mover.speed = Random.Range(speedRange.x, speedRange.y);
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
