// GAMEINPUT.CS  -  no setup, don't put it on anything.
// One place that answers "did the player fire this frame?" for mouse AND controller,
// and remembers which one they're using (so rumble only happens on the controller).
// While the game is paused, firing is ignored (the pause menu buttons still work).

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class GameInput
{
    // true = the player's last shot came from a controller
    public static bool UsingPad { get; private set; }

    // mouse click OR controller fire (never while paused)
    public static bool FirePressed()
    {
        if (PauseMenu.IsPaused) return false;

        bool mouse = MousePressed();
        bool pad = PadFirePressed();

        if (pad) UsingPad = true;
        else if (mouse) UsingPad = false;

        return mouse || pad;
    }

    public static bool MousePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    // right trigger or the bottom face button (A on Xbox, Cross on PlayStation, B on Switch)
    public static bool PadFirePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        return pad != null && (pad.rightTrigger.wasPressedThisFrame || pad.buttonSouth.wasPressedThisFrame);
#else
        return false;
#endif
    }
}