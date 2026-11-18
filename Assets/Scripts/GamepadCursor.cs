// GAMEPADCURSOR.CS  -  no setup, don't put it on anything. It creates itself when the game starts.
//
// IN ONE SENTENCE: the controller's sticks move the real mouse cursor (so your PNG crosshair
// moves too), the trigger / A button "clicks" whatever UI is under it, and any script can make
// the controller rumble with GamepadCursor.Rumble(low, high, seconds).

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

public class GamepadCursor : MonoBehaviour
{
    public float speed = 900f;     // pixels per second at full tilt
    public float deadzone = 0.2f;  // ignore tiny stick drift

    static GamepadCursor instance;
    Coroutine rumbleRoutine;

    // Runs automatically when the game starts, in any scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        GameObject go = new GameObject("GamepadCursor");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<GamepadCursor>();
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = Gamepad.current;
        if (pad == null || Mouse.current == null) return;

        // MOVE: either stick works
        Vector2 stick = pad.leftStick.ReadValue() + pad.rightStick.ReadValue();
        if (stick.magnitude > deadzone)
        {
            stick = Vector2.ClampMagnitude(stick, 1f);
            stick *= stick.magnitude * stick.magnitude;   // even gentler at small tilts

            Vector2 pos = Mouse.current.position.ReadValue() + stick * speed * Time.unscaledDeltaTime;
            pos.x = Mathf.Clamp(pos.x, 0f, Screen.width - 1f);
            pos.y = Mathf.Clamp(pos.y, 0f, Screen.height - 1f);

            Mouse.current.WarpCursorPosition(pos);           // moves the real cursor on screen
            InputState.Change(Mouse.current.position, pos);  // makes scripts see the new spot this frame
        }

        // CLICK UI: menu buttons, toggles, the marquee
        if (GameInput.PadFirePressed()) ClickUI(Mouse.current.position.ReadValue());
#endif
    }

    // --- RUMBLE ---
    // low = the heavy, thumpy motor, high = the light, buzzy motor (both 0 to 1)
    public static void Rumble(float low, float high, float seconds)
    {
#if ENABLE_INPUT_SYSTEM
        if (instance == null || !GameSettings.Rumble || !GameInput.UsingPad) return;

        Gamepad pad = Gamepad.current;
        if (pad == null) return;

        // a new rumble replaces the old one, so a hit's thump overrides the fire tick
        if (instance.rumbleRoutine != null) instance.StopCoroutine(instance.rumbleRoutine);
        instance.rumbleRoutine = instance.StartCoroutine(instance.RumbleFor(pad, low, high, seconds));
#endif
    }

#if ENABLE_INPUT_SYSTEM
    IEnumerator RumbleFor(Gamepad pad, float low, float high, float seconds)
    {
        pad.SetMotorSpeeds(low, high);
        yield return new WaitForSecondsRealtime(seconds);
        pad.SetMotorSpeeds(0f, 0f);
        rumbleRoutine = null;
    }
#endif

    // never leave a controller buzzing if the game loses focus or closes
    void OnApplicationFocus(bool focused)
    {
        if (!focused) StopRumble();
    }

    void OnDisable()
    {
        StopRumble();
    }

    void StopRumble()
    {
#if ENABLE_INPUT_SYSTEM
        if (Gamepad.current != null) Gamepad.current.ResetHaptics();
#endif
    }

    // Finds the UI element under the cursor and clicks it, like a mouse click would
    void ClickUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return;

        PointerEventData data = new PointerEventData(EventSystem.current) { position = screenPos };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        if (results.Count == 0) return;

        ExecuteEvents.ExecuteHierarchy(results[0].gameObject, data, ExecuteEvents.pointerClickHandler);
    }
}