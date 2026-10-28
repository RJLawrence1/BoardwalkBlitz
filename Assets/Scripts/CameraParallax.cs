// CAMERAPARALLAX.CS  -  goes on the Main Camera (right next to the Shooter script).
//
// IN ONE SENTENCE: the camera "leans" toward the mouse, like leaning over the booth counter.
// Mouse in the center = no lean. Mouse in the top-right corner = lean up and right.
// Mouse in the bottom-left corner = lean down and left.
//
// WHY IT LOOKS 3D: the camera SLIDES (it doesn't spin), so near layers (the counter) shift
// more than far layers (the backdrop). Your brain reads that difference as depth.
// This only works if your layers sit at different distances from the camera.
//
// DOES IT BREAK AIMING? No. The Shooter fires its ray from wherever the camera is,
// through the cursor, so you still hit what you point at.

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CameraParallax : MonoBehaviour
{
    [Header("Lean")]
    public bool enableLean = true;     // turn the whole effect off (nice accessibility option)
    public float leanAmount = 0.3f;    // how far the camera slides at the very edge of the screen
    public float smoothing = 6f;       // higher = snappier, lower = floatier

    [Header("Optional head tilt")]
    public bool useTilt = false;
    public float tiltDegrees = 1.5f;   // keep it tiny. If it tilts the wrong way, make this negative

    Vector3 startPosition;
    Quaternion startRotation;

    void Start()
    {
        // Remember where the camera started: this is the "perfect center" view
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    // LateUpdate runs after all the normal Update calls, so the camera moves last each frame
    void LateUpdate()
    {
        Vector2 lean = enableLean
            ? ScreenToLean(PointerPosition(), Screen.width, Screen.height)
            : new Vector2(0f, 0f);

        // Slide along the camera's ORIGINAL right and up directions
        Vector3 right = startRotation * Vector3.right;
        Vector3 up = startRotation * Vector3.up;
        Vector3 targetPosition = startPosition + right * (lean.x * leanAmount) + up * (lean.y * leanAmount);

        // Lerp = move a fraction of the way there each frame, which makes it smooth
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);

        if (useTilt)
        {
            // Roll the camera slightly, like tilting your head as you lean
            Quaternion targetRotation = startRotation * Quaternion.Euler(0f, 0f, -lean.x * tiltDegrees);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothing * Time.deltaTime);
        }
    }

    // Turns a pixel position into -1..+1 on each axis:
    //   center = (0, 0)   top-right = (1, 1)   bottom-left = (-1, -1)
    // Positions outside the window are clamped, so the lean never goes past the limit.
    public static Vector2 ScreenToLean(Vector2 pixel, float width, float height)
    {
        float x = Mathf.Clamp((pixel.x / width - 0.5f) * 2f, -1f, 1f);
        float y = Mathf.Clamp((pixel.y / height - 0.5f) * 2f, -1f, 1f);
        return new Vector2(x, y);
    }

    Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
#else
        return Input.mousePosition;
#endif
    }
}
