// UIMANAGER.CS  -  goes on an empty GameObject in each BOOTH scene (next to your Canvas).
//
// IN ONE SENTENCE: shows your combo and multiplier, your ammo, and a "RELOADING" message
// during the reload pause. (It can also move a UI crosshair, but we use the PNG cursor instead.)
//
// (BoothManager already handles the timer and score text, so this script covers the rest.)
//
// SETUP:
//   - Canvas: set Render Mode to "Screen Space - Overlay".
//   - crosshair: leave EMPTY (the PNG cursor from Project Settings does this job).
//   - hideSystemCursor: leave UNTICKED, or the PNG cursor disappears.
//   - shooter: drag in the Main Camera (it has the Shooter script).
//   - comboText (optional): a DSEG14 TextMeshPro text. Shows e.g. "COMBO 007 X3". Blank at 0 combo.
//   - reloadObject (optional): any UI object (like a "RELOADING" label). Shown only while reloading.
//   - ammoText (optional): a TextMeshPro text showing shots left before the reload.
//
// DSEG FONT TIP: "!" is a blank space that's the same width as a digit, so text lines up
// perfectly with the gray "ghost" text behind it. A normal space is skinnier.

using UnityEngine;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class UIManager : MonoBehaviour
{
    public RectTransform crosshair;
    public Shooter shooter;

    [Header("Optional")]
    public TMP_Text comboText;
    public GameObject reloadObject;
    public TMP_Text ammoText;
    public bool hideSystemCursor = false;   // CHANGED: off by default so the PNG cursor stays visible

    void Start()
    {
        if (hideSystemCursor) Cursor.visible = false;
    }

    // When this scene closes, give the player their normal cursor back
    void OnDisable()
    {
        Cursor.visible = true;
    }

    void Update()
    {
        // Crosshair (only if you use a UI crosshair instead of the PNG cursor)
        if (crosshair != null) crosshair.position = PointerPosition();

        // Combo and multiplier, LED style: "COMBO 007 X3"
        if (comboText != null)
        {
            ScoreManager sm = ScoreManager.Instance;
            if (sm != null && sm.combo > 0)
                comboText.text = "COMBO!" + sm.combo.ToString("000") + "!X" + sm.Multiplier;   // CHANGED
            else
                comboText.text = "";   // blank = only the gray ghost shows, like a display that's off
        }

        if (shooter != null)
        {
            if (reloadObject != null) reloadObject.SetActive(shooter.IsReloading);
            if (ammoText != null) ammoText.text = shooter.IsReloading ? "AMM0:--" : "AMM0:" + shooter.ShotsLeft.ToString("00");   // CHANGED: 3 digits
        }
    }

    Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }
}