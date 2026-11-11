// DIALOGUEBOX.CS  -  put it on an empty GameObject in each booth scene.
//
// IN ONE SENTENCE: shows lines of dialogue one at a time in a box at the bottom of the screen,
// typing them out letter by letter. Click / Space / Enter finishes the line or goes to the next,
// and if nobody clicks, it moves on by itself after a reading delay (longer lines wait longer).

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
    public Sprite portrait;               // the speaker's face for this line
    [TextArea] public string text;        // family friendly (default)
    [TextArea] public string altText;     // used when Family Friendly is OFF (leave empty to use text)
}

public class DialogueBox : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;              // the dialogue box (starts OFF)
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text bodyText;
    public GameObject continueArrow;      // shows when a line is done (optional)

    [Header("Typing")]
    public float charsPerSecond = 40f;
    public AudioClip blipSound;           // little talking blip (optional)
    public int blipEvery = 2;

    [Header("Auto advance")]
    public bool autoAdvance = true;        // move to the next line on its own if nobody clicks
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

            // typewriter: show a few more letters each frame
            bodyText.text = text;
            bodyText.maxVisibleCharacters = 0;

            float typed = 0f;
            int visible = 0;
            bool skipped = false;

            while (visible < text.Length)
            {
                if (Pressed()) { skipped = true; break; }   // click = show the whole line now

                typed += charsPerSecond * Time.deltaTime;
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
                yield return null;
            }

            bodyText.maxVisibleCharacters = 99999;
            if (skipped) yield return null;   // so that click doesn't also skip the next line

            if (continueArrow != null) continueArrow.SetActive(true);

            // wait for a click, OR until the reading time runs out
            float readTime = Mathf.Clamp(text.Length * secondsPerLetter, minWait, maxWait);
            float waited = 0f;
            while (!Pressed())
            {
                waited += Time.deltaTime;
                if (autoAdvance && waited >= readTime) break;   // nobody clicked, move on
                yield return null;
            }
            yield return null;
        }

        if (panel != null) panel.SetActive(false);
        IsPlaying = false;
    }

    bool Pressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool key = Keyboard.current != null &&
                   (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame);
        return click || key;
#else
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
#endif
    }
}