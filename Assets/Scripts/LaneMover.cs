// LANEMOVER.CS  -  put it on any target that should slide (ducks, cans, drinks on the bar...).
//
// IN ONE SENTENCE: moves the object in a straight line every frame, then removes it
// after a few seconds (so targets that slid off-screen, or glasses that "fell off the bar",
// don't pile up).
//
// TargetSpawner sets the direction and speed automatically when it spawns the target.

using UnityEngine;

public class LaneMover : MonoBehaviour
{
    public Vector3 direction = Vector3.right; // which way to slide
    public float speed = 3f;                  // units per second
    public float lifetime = 6f;               // seconds until it's removed

    void Update()
    {
        // Time.deltaTime = seconds since the last frame, so movement is smooth on any computer
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }
}
