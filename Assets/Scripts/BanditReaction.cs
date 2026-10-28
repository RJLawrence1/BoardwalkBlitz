// BANDITREACTION.CS  -  goes on the bandit. Plays the "clutch chest, lick hand, ketchup packets?" gag.
//
// IN ONE SENTENCE: steps through a list of "beats". Each beat shows one picture (pose) of the
// bandit, optionally a line of dialogue and a sound, and holds for a set time.
//
// SETUP: in the Inspector fill the "Beats" list (one entry per pose). Hook "Play" up to
// BossBandit's "On Splatted" event, and hook this script's "On Finished" event to
// BoothManager.EndRoundEarly so the stage ends after the joke.
//
// [System.Serializable] on Beat lets Unity show the little Beat class as a form in the Inspector.

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class BanditReaction : MonoBehaviour
{
    [System.Serializable]
    public class Beat
    {
        public Sprite pose;      // which picture of the bandit to show
        public float duration;   // how long to hold it (seconds)
        public string line;      // dialogue shown during this beat (leave empty for none)
        public AudioClip sound;  // optional sound effect
    }

    public SpriteRenderer banditRenderer; // the bandit's sprite
    public AudioSource audioSource;       // plays the sound effects
    public TMP_Text dialogueText;         // where the lines appear
    public Beat[] beats;                  // the sequence, in order
    public UnityEvent onFinished;         // runs after the last beat

    // Hook this up to BossBandit's "On Splatted" event
    public void Play()
    {
        StartCoroutine(PlayBeats());
    }

    IEnumerator PlayBeats()
    {
        foreach (Beat beat in beats)
        {
            if (banditRenderer != null && beat.pose != null) banditRenderer.sprite = beat.pose;
            if (dialogueText != null) dialogueText.text = beat.line;
            if (audioSource != null && beat.sound != null) audioSource.PlayOneShot(beat.sound);

            yield return new WaitForSeconds(beat.duration); // hold this beat, then move to the next
        }

        if (dialogueText != null) dialogueText.text = "";
        onFinished.Invoke();
    }
}
