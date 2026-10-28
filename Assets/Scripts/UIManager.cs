// UIMANAGER.CS  -  goes on an empty GameObject in each BOOTH scene (next to your Canvas).
//
// IN ONE SENTENCE: it makes a crosshair follow the mouse, shows your combo and multiplier,
// and shows a "RELOADING" message during the reload pause.
//
// (BoothManager already handles the timer and score text, so this script covers the rest.)
//
// SETUP:
//   - Canvas: set Render Mode to "Screen Space - Overlay".
//   - crosshair: a UI Image on the Canvas (your crosshair sprite). Turn OFF "Raycast Target" on it
//     so it never blocks anything.
//   - shooter: drag in the Main Camera (it has the Shooter script).
//   - comboText (optional): a TextMeshPro text. Shows e.g. "COMBO 7  x2". Hides itself at 0 combo.
//   - reloadObject (optional): any UI object (like a "RELOADING" label). Shown only while reloading.
//   - ammoText (optional): a TextMeshPro text showing shots left before the reload.

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
    public bool hideSystemCursor = true;

    void Start()
    {
        if (hideSystemCursor) Cursor.visible = false; // the crosshair replaces the normal cursor
    }

    // When this scene closes, give the player their normal cursor back
    void OnDisable()
    {
        Cursor.visible = true;
    }

    void Update()
    {
        // Crosshair: for an Overlay canvas, a UI object's position is measured in screen pixels,
        // which is exactly what the mouse position is - so we can copy it straight over.
        if (crosshair != null) crosshair.position = PointerPosition();

        // Combo and multiplier
        if (comboText != null)
        {
            ScoreManager sm = ScoreManager.Instance;
            if (sm != null && sm.combo > 0)
                comboText.text = "COMBO " + sm.combo + "  x" + sm.Multiplier;
            else
                comboText.text = "";
        }

        if (shooter != null)
        {
            if (reloadObject != null) reloadObject.SetActive(shooter.IsReloading);
            if (ammoText != null) ammoText.text = shooter.IsReloading ? "..." : shooter.ShotsLeft.ToString();
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
