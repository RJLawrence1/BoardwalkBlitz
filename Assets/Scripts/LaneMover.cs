// LANEMOVER.CS  -  put it on any target that should slide (ducks, cans, drinks on the bar...).
//
// Moves the object in a straight line, then removes it once it has traveled "travelDistance"
// (or after "lifetime" seconds, if travelDistance is 0).
//
// TargetSpawner sets the direction, speed and travel distance automatically.

using UnityEngine;

public class LaneMover : MonoBehaviour
{
    public Vector3 direction = Vector3.right; // which way to slide
    public float speed = 3f;                  // units per second
    public float travelDistance = 0f;         // delete after going this far (0 = use lifetime instead)
    public float lifetime = 6f;               // backup timer, only used when travelDistance is 0

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;   // remember where it spawned
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        if (travelDistance > 0f)
        {
            // gone far enough? remove it
            if (Vector3.Distance(startPos, transform.position) >= travelDistance)
                Destroy(gameObject);
        }
        else
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) Destroy(gameObject);
        }
    }
}