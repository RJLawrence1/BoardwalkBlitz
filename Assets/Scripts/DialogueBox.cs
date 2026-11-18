// DIALOGUEBOX.CS  -  put it on an empty GameObject in each booth scene.
//
// IN ONE SENTENCE: shows lines of dialogue one at a time in a box at the bottom of the screen,
// typing them out letter by letter, and swaps Curly's pose in the booth for each line.
// Click / Space / Enter / controller trigger or A finishes the line or goes to the next, and if
// nobody presses anything, it moves on by itself. Nothing can be skipped while the game is paused.
//
// POSES (per line):
//   Pose ........ what Curly looks like for this line (empty = keep whatever he had)
//   Rest Pose ... optional mouth-closed version: he flaps between Pose and Rest Pose while the
//                 text types (looks like talking), then holds Rest Pose when the line is done.
//                 Only pair two poses that face the same way (like Present + Idle).

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[System.Serializable]
public class DialogueLine
{
    public string speaker = "CURLY";
    public Sprite pose;                   // Curly's pose for this line (empty = keep the current one)
    public Sprite restPose;               // optional mouth-closed pose for the talking flap
    [TextArea] public string text;        // family friendly (default)
    [TextArea] public string altText;     // used when Family Friendly is OFF (leave empty to use text)
    public Sprite portrait;               // optional face in the dialogue box (you can ignore this)
}

public class DialogueBox : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;              // the dialogue box (starts OFF)
    public Image portraitImage;           // optional
    public TMP_Text nameText;
    public TMP_Text bodyText;
    public GameObject continueArrow;      // shows when a line is done (optional)

    [Header("Speaker in the booth")]
    public SpriteRenderer speakerSprite;  // Curly's Sprite Renderer in the scene
    public float flapTime = 0.12f;        // seconds between mouth open/closed while talking

    [Header("Typing")]
    public float charsPerSecond = 40f;
    public AudioClip blipSound;           // little talking blip (optional)
    public int blipEvery = 2;

    [Header("Auto advance")]
    public bool autoAdvance = true;        // move to the next line on its own if nobody presses anything
    public float minWait = 5f;             // shortest wait (short lines)
    public float maxWait = 10f;            // longest wait (long lines)
    public float secondsPerLetter = 0.08f; // longer lines get more reading time

    public bool IsPlaying { get; private set; }

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    // BoothManager waits on this: "yield return dialogue.Play(lines);"
    public IEnumerator Play(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0) yield break;

        IsPlaying = true;
        if (panel != null) panel.SetActive(true);

        foreach (DialogueLine line in lines)
        {
            bool useAlt = !GameSettings.FamilyFriendly && !string.IsNullOrEmpty(line.altText);
            string text = useAlt ? line.altText : line.text;

            if (nameText != null) nameText.text = line.speaker;
            if (portraitImage != null)
            {
                portraitImage.enabled = (line.portrait != null);
                portraitImage.sprite = line.portrait;
            }
            if (continueArrow != null) continueArrow.SetActive(false);

            // set Curly's pose for this line
            SetPose(line.pose);
            bool flap = (line.pose != null && line.restPose != null);
            bool mouthOpen = true;
            float flapTimer = 0f;

            // typewriter: show a few more letters each frame
            bodyText.text = text;
            bodyText.maxVisibleCharacters = 0;

            float typed = 0f;
            int visible = 0;
            bool skipped = false;

            while (visible < text.Length)
            {
                if (Pressed()) { skipped = true; break; }   // press = show the whole line now

                typed += charsPerSecond * Time.deltaTime;   // freezes while paused
                int now = Mathf.Min(Mathf.FloorToInt(typed), text.Length);

                if (now > visible)
                {
                    visible = now;
                    bodyText.maxVisibleCharacters = visible;

                    if (blipSound != null && visible % blipEvery == 0 && text[visible - 1] != ' ')
                    {
                        if (SoundManager.Instance != null) SoundManager.Instance.Play(blipSound, 0.6f, 0.15f);
                    }
                }

                // TALKING FLAP: swap mouth open/closed while the text types
                if (flap)
                {
                    flapTimer += Time.deltaTime;
                    if (flapTimer >= flapTime)
                    {
                        flapTimer = 0f;
                        mouthOpen = !mouthOpen;
                        SetPose(mouthOpen ? line.pose : line.restPose);
                    }
                }

                yield return null;
            }

            bodyText.maxVisibleCharacters = 99999;

            // done talking: hold the mouth-closed pose (or keep the single pose)
            if (flap) SetPose(line.restPose);

            if (skipped) yield return null;   // so that press doesn't also skip the next line

            if (continueArrow != null) continueArrow.SetActive(true);

            // wait for a press, OR until the reading time runs out
            float readTime = Mathf.Clamp(text.Length * secondsPerLetter, minWait, maxWait);
            float waited = 0f;
            while (!Pressed())
            {
                waited += Time.deltaTime;   // freezes while paused
                if (autoAdvance && waited >= readTime) break;   // nobody pressed anything, move on
                yield return null;
            }
            yield return null;
        }

        if (panel != null) panel.SetActive(false);
        IsPlaying = false;
    }

    // swaps Curly's sprite (does nothing if the slot is empty)
    void SetPose(Sprite pose)
    {
        if (speakerSprite != null && pose != null) speakerSprite.sprite = pose;
    }

    // mouse click, Space, Enter, or controller trigger / A (never while paused)
    bool Pressed()
    {
        if (PauseMenu.IsPaused) return false;   // no skipping Curly's lines while paused

#if ENABLE_INPUT_SYSTEM
        bool key = Keyboard.current != null &&
                   (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame);
        return GameInput.FirePressed() || key;
#else
        return GameInput.FirePressed() || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
#endif
    }
}