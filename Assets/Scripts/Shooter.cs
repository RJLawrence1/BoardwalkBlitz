// SHOOTER.CS  -  goes on the Main Camera.
//
// IN ONE SENTENCE: when you click, it shoots an invisible line (a "raycast") from the camera
// through the mouse cursor. If that line touches a Target, it tells the Target "you got hit".
//
// ALSO DOES: a short reload pause every 100 shots, and the stun/blind effect when a thrown
// bottle hits you. BoothManager can switch shooting on/off with the canShoot checkbox.

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Shooter : MonoBehaviour
{
    [Header("Aiming")]
    public LayerMask hitLayers = ~0;
    public float maxDistance = 100f;

    [Header("Reload")]
    public int shotsBeforeReload = 100;
    public float reloadTime = 1f;

    [Header("Stun / Blind")]
    public CanvasGroup blindOverlay;
    public float blindAlpha = 0.6f;

    [Header("Sounds")]                  // NEW
    public AudioClip shootSound;        // NEW - plays every shot
    public AudioClip reloadSound;       // NEW - plays when the forced reload starts

    [Header("Control")]
    public bool canShoot = true;

    int shotsLeft;
    float reloadTimer;
    float stunTimer;

    public bool IsReloading => reloadTimer > 0f;
    public bool IsStunned => stunTimer > 0f;
    public int ShotsLeft => shotsLeft;

    void Start()
    {
        ResetShooter();
    }

    public void ResetShooter()
    {
        shotsLeft = shotsBeforeReload;
        reloadTimer = 0f;
        stunTimer = 0f;
        if (blindOverlay != null) blindOverlay.alpha = 0f;
    }

    public void Stun(float seconds)
    {
        stunTimer = seconds;
        if (blindOverlay != null) blindOverlay.alpha = blindAlpha;
    }

    void Update()
    {
        if (!canShoot) return;

        if (IsStunned)
        {
            stunTimer -= Time.deltaTime;
            if (!IsStunned && blindOverlay != null) blindOverlay.alpha = 0f;
            return;
        }

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
            if (shotsLeft <= 0)
            {
                reloadTimer = reloadTime;
                if (SoundManager.Instance != null) SoundManager.Instance.Play(reloadSound, 1f, 0f); // NEW
            }
        }
    }

    void Shoot()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.Play(shootSound); // NEW

        Ray ray = Camera.main.ScreenPointToRay(PointerPosition());
        bool hitSomething = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitLayers))
        {
            Target target = hit.collider.GetComponentInParent<Target>();
            if (target != null)
            {
                target.Hit();
                hitSomething = true;
            }
        }

        if (!hitSomething && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RegisterMiss();
        }
    }

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