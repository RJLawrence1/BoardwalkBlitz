// GAMEINPUT.CS  -  no setup, don't put it on anything.
// One place that answers "did the player fire this frame?" for mouse, touch AND controller,
// and remembers which one they're using (so rumble only happens on the controller).
// While the game is paused, firing is ignored (the pause menu buttons still work).
//
// WEB BUILDS: controller input is switched off (browsers can't move the cursor for aiming).
// TOUCH: a tap counts as a shot, using the "Pointer" (whatever was used last: mouse or finger).

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class GameInput
{
    // true = the player's last shot came from a controller
    public static bool UsingPad { get; private set; }

    // mouse click, screen tap, OR controller fire (never while paused)
    public static bool FirePressed()
    {
        if (PauseMenu.IsPaused) return false;

        bool pointer = PointerPressed();
        bool pad = PadFirePressed();

        if (pad) UsingPad = true;
        else if (pointer) UsingPad = false;

        return pointer || pad;
    }

    // left mouse click OR a finger tapping the screen
    public static bool PointerPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    // where the mouse is, or where the finger last touched
    public static Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    // right trigger or the bottom face button (A on Xbox, Cross on PlayStation, B on Switch)
    // always "no" in web builds
    public static bool PadFirePressed()
    {
#if ENABLE_INPUT_SYSTEM && !UNITY_WEBGL
        Gamepad pad = Gamepad.current;
        return pad != null && (pad.rightTrigger.wasPressedThisFrame || pad.buttonSouth.wasPressedThisFrame);
#else
        return false;
#endif
    }
}