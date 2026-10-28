// CUTSCENECONTROLLER.CS  -  goes on an empty GameObject in a CUTSCENE scene
// (the intro), or on each ENDING object that GameManager switches on.
//
// IN ONE SENTENCE: it plays a Timeline cutscene when it appears, lets the player skip it,
// and when it's over, loads the next scene (and/or fires an event).
//
// SETUP:
//   - director: the GameObject that has your Timeline (a "Playable Director" component).
//   - nextSceneName: the scene to load afterwards, e.g. "Booth_Ducks" for the intro,
//     or "MainMenu" for an ending. Leave it empty to just stop (the ending buttons take over).
//   - allowSkip: lets the player click or press Space to skip.
//   - No Timeline yet? Leave "director" empty and set fallbackSeconds - it will wait that long.
//     That way you can test the game flow before the cutscene is made.
//   - Add every scene to File > Build Settings, or LoadScene won't find it.
//
// FOR ENDINGS: put this script on each ending object (Rookie / Sharpshooter / Legend).
// GameManager switches only the matching one on, and it starts playing right away.

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CutsceneController : MonoBehaviour
{
    public PlayableDirector director;
    public string nextSceneName = "";
    public bool allowSkip = true;
    public float fallbackSeconds = 3f;   // only used when there's no director
    public UnityEvent onFinished;

    bool finished;

    // OnEnable runs every time this object is switched on, so an ending plays as soon as it appears
    void OnEnable()
    {
        finished = false;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        if (director != null)
        {
            director.Play();
            yield return null; // wait one frame so the director has really started

            // Keep waiting while the cutscene is still playing
            while (!finished && director.state == PlayState.Playing)
            {
                yield return null;
            }
        }
        else
        {
            float timer = 0f;
            while (!finished && timer < fallbackSeconds)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        Finish();
    }

    void Update()
    {
        if (allowSkip && !finished && SkipPressed())
        {
            finished = true; // the coroutine sees this and wraps up
            if (director != null) director.Stop();
        }
    }

    // Runs once, when the cutscene ends (naturally or skipped)
    void Finish()
    {
        finished = true;
        onFinished.Invoke();
        if (!string.IsNullOrEmpty(nextSceneName)) SceneManager.LoadScene(nextSceneName);
    }

    bool SkipPressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool key = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        return click || key;
#else
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
#endif
    }
}
