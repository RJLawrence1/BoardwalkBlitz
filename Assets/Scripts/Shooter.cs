// SHOOTER.CS  -  goes on the Main Camera.
//
// IN ONE SENTENCE: when you click, it shoots an invisible line (a "raycast") from the camera
// through the mouse cursor. If that line touches a Target, it tells the Target "you got hit".
//
// ALSO DOES: a short reload pause every 100 shots, and the stun/blind effect when a thrown
// bottle hits you. BoothManager can switch shooting on/off with the canShoot checkbox.
//
// NOTE: the #if ENABLE_INPUT_SYSTEM lines are a compiler switch. They make the same script work
// whether your project uses Unity's new Input System or the old Input Manager.

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Shooter : MonoBehaviour
{
    [Header("Aiming")]
    public LayerMask hitLayers = ~0;   // which layers a shot can hit (~0 means everything)
    public float maxDistance = 100f;   // how far a shot reaches

    [Header("Reload")]
    public int shotsBeforeReload = 100; // shots before the forced reload pause
    public float reloadTime = 1f;       // how long the pause lasts (seconds)

    [Header("Stun / Blind")]
    public CanvasGroup blindOverlay;    // a full-screen UI Image with a CanvasGroup (alpha 0 at start)
    public float blindAlpha = 0.6f;     // how see-through the haze is

    [Header("Control")]
    public bool canShoot = true;        // BoothManager turns this off between rounds

    int shotsLeft;        // shots left before the next reload
    float reloadTimer;    // counts down while reloading
    float stunTimer;      // counts down while stunned

    // "Properties": read-only true/false answers other scripts (like the UI) can ask for
    public bool IsReloading => reloadTimer > 0f;
    public bool IsStunned => stunTimer > 0f;
    public int ShotsLeft => shotsLeft;   // how many shots until the next reload (the UI can show this)

    void Start()
    {
        ResetShooter();
    }

    // Puts everything back to a fresh start (called at the start of each round)
    public void ResetShooter()
    {
        shotsLeft = shotsBeforeReload;
        reloadTimer = 0f;
        stunTimer = 0f;
        if (blindOverlay != null) blindOverlay.alpha = 0f;
    }

    // Other scripts (like ThrownObject) call this when something hits the player
    public void Stun(float seconds)
    {
        stunTimer = seconds;
        if (blindOverlay != null) blindOverlay.alpha = blindAlpha;
    }

    void Update()
    {
        if (!canShoot) return;

        // Stunned: count down, clear the haze when done, and don't allow shooting
        if (IsStunned)
        {
            stunTimer -= Time.deltaTime;
            if (!IsStunned && blindOverlay != null) blindOverlay.alpha = 0f;
            return;
        }

        // Reloading: count down, refill when done, and don't allow shooting
        if (IsReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (!IsReloading) shotsLeft = shotsBeforeReload;
            return;
        }

        if (ClickedThisFrame())
        {
            Shoot();
            shotsLeft--;
            if (shotsLeft <= 0) reloadTimer = reloadTime; // out of shots: start the reload
        }
    }

    void Shoot()
    {
        // Make a ray (an invisible line) from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(PointerPosition());
        bool hitSomething = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitLayers))
        {
            // Did the line touch something with a Target script on it (or on its parent)?
            Target target = hit.collider.GetComponentInParent<Target>();
            if (target != null)
            {
                target.Hit();
                hitSomething = true;
            }
        }

        // A miss breaks your combo
        if (!hitSomething && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterMiss();
        }
    }

    // --- Small helpers so the rest of the script doesn't care which input system is used ---

    bool ClickedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current.position.ReadValue();
#else
        return Input.mousePosition;
#endif
    }
}
