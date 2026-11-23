// DIALOGUEBOX.CS  -  put it on an empty GameObject in each booth scene.
//
// IN ONE SENTENCE: shows lines of dialogue one at a time in a box at the bottom of the screen,
// typing them out letter by letter with an Undertale-style voice blip. Click / Space / Enter /
// controller trigger or A finishes the line or goes to the next, and if nobody presses anything,
// it moves on by itself. Nothing can be skipped while the game is paused.
//
// VOICE: "Blip Sound" plays on letters and numbers only (spaces and punctuation stay silent,
// which gives the natural pauses). "Voice Pitch" sets how high or low the character sounds.
//
// CURLY: this script doesn't need to know about him. BoothManager passes in his picture
// (a Sprite Renderer) when he talks, and this swaps it to each line's pose.
//
// POSES (per line):
//   Pose ........ what Curly looks like for this line (empty = keep whatever he had)
//   Rest Pose ... optional mouth-closed version: he flaps between Pose and Rest Pose while the
//                 text types (looks like talking), then holds Rest Pose when the line is done.

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

    [Header("Talking")]
    public float flapTime = 0.12f;        // seconds between mouth open/closed while talking

    [Header("Typing")]
    public float charsPerSecond = 40f;

    [Header("Voice (Undertale-style blips)")]
    public AudioClip blipSound;                  // a tiny short blip
    public int blipEvery = 2;                    // blip every N letters (1 = every letter, 2 = calmer)
    [Range(0.5f, 2f)] public float voicePitch = 1.1f;     // higher = brighter voice, lower = deeper
    [Range(0f, 0.3f)] public float voicePitchVariation = 0.05f; // tiny wobble so it isn't robotic
    [Range(0f, 1f)] public float voiceVolume = 0.6f;

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

    // BoothManager calls this: "yield return dialogue.Play(lines, curlysPicture);"
    // speakerSprite is optional: leave it out and the lines play with no pose swaps.
    public IEnumerator Play(DialogueLine[] lines, SpriteRenderer speakerSprite = null)
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

            // set the speaker's pose for this line
            SetPose(speakerSprite, line.pose);
            bool flap = (speakerSprite != null && line.pose != null && line.restPose != null);
            bool mouthOpen = true;
            float flapTimer = 0f;

            // typewriter: show a few more letters each frame
            bodyText.text = text;
            bodyText.maxVisibleCharacters = 0;

            float typed = 0f;
            int visible = 0;
            int lettersSinceBlip = 0;
            bool skipped = false;

            while (visible < text.Length)
            {
                if (Pressed()) { skipped = true; break; }   // press = show the whole line now

                typed += charsPerSecond * Time.deltaTime;   // freezes while paused
                int now = Mathf.Min(Mathf.FloorToInt(typed), text.Length);

                // go through each newly shown letter (a frame can show more than one)
                while (visible < now)
                {
                    char c = text[visible];
                    visible++;

                    // VOICE: only letters and numbers blip, so spaces and punctuation are natural pauses
                    if (char.IsLetterOrDigit(c))
                    {
                        lettersSinceBlip++;
                        if (lettersSinceBlip >= blipEvery)
                        {
                            lettersSinceBlip = 0;
                            PlayBlip();
                        }
                    }
                }
                bodyText.maxVisibleCharacters = visible;

                // TALKING FLAP: swap mouth open/closed while the text types
                if (flap)
                {
                    flapTimer += Time.deltaTime;
                    if (flapTimer >= flapTime)
                    {
                        flapTimer = 0f;
                        mouthOpen = !mouthOpen;
                        SetPose(speakerSprite, mouthOpen ? line.pose : line.restPose);
                    }
                }

                yield return null;
            }

            bodyText.maxVisibleCharacters = 99999;

            // done talking: hold the mouth-closed pose (or keep the single pose)
            if (flap) SetPose(speakerSprite, line.restPose);

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

    void PlayBlip()
    {
        if (blipSound != null && SoundManager.Instance != null)
            SoundManager.Instance.Play(blipSound, voiceVolume, voicePitchVariation, voicePitch);
    }

    // swaps the speaker's picture (does nothing if there's no speaker or no pose)
    void SetPose(SpriteRenderer speakerSprite, Sprite pose)
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