// THROWNOBJECT.CS  -  goes on the bottles and glasses the bandit throws.
// The same object also needs: a Target script (untick "flipDown") and a Collider.
//
// IN ONE SENTENCE: flies straight at the camera. If you shoot it first, the Target script
// removes it. If it reaches you, it stuns you (Shooter.Stun) and disappears.
//
// Requires the camera to be tagged "MainCamera" and to have the Shooter script on it.

using UnityEngine;

[RequireComponent(typeof(Target))]
public class ThrownObject : MonoBehaviour
{
    public float speed = 6f;          // how fast it flies at you
    public float stunTime = 1f;       // how long it stuns you (seconds)
    public float hitDistance = 1.5f;  // how close to the camera counts as "it hit you"

    Transform cam;
    Shooter shooter;

    void Start()
    {
        cam = Camera.main.transform;
        shooter = cam.GetComponent<Shooter>();
    }

    void Update()
    {
        // Move a little closer to the camera each frame
        transform.position = Vector3.MoveTowards(transform.position, cam.position, speed * Time.deltaTime);

        // Close enough = it hit you
        if (Vector3.Distance(transform.position, cam.position) <= hitDistance)
        {
            if (shooter != null) shooter.Stun(stunTime);
            Destroy(gameObject);
        }
    }
}
