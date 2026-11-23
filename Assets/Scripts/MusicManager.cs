// MUSICMANAGER.CS  -  put it on an empty GameObject called "MusicManager" in each scene with music.
//
// IN ONE SENTENCE: plays this scene's music, fades songs in and out, and follows the Music
// slider from the Options menu.
//
// TWO WAYS TO USE IT:
//   Simple scenes (menu, results): drag a song into "Track" and it loops from the start.
//   Booths: leave "Track" empty. BoothManager tells it what to play and when
//           (pre-round, round, post-round) using Play() and FadeOut().

using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;           // lets other scripts find it easily

    public AudioClip track;                        // optional: plays automatically when the scene starts
    [Range(0f, 1f)] public float trackVolume = 1f; // balance songs that are too loud/quiet
    public float fadeInTime = 1f;                  // seconds to fade the starting track in

    AudioSource source;
    float fade;            // 0 = silent, 1 = full volume (multiplied with the Music slider)
    Coroutine fading;

    void Awake()
    {
        Instance = this;

        source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;   // plain 2D sound
        source.volume = 0f;
    }

    void Start()
    {
        if (track != null) Play(track, fadeInTime);
    }

    void Update()
    {
        // always follow the Music slider, times the current fade
        source.volume = trackVolume * GameSettings.MusicVolume * fade;
    }

    // Starts a song. fadeTime 0 = starts instantly at full volume.
    public void Play(AudioClip clip, float fadeTime = 1f, bool loop = true)
    {
        if (clip == null) return;
        if (fading != null) StopCoroutine(fading);

        source.clip = clip;
        source.loop = loop;
        source.Play();

        if (fadeTime <= 0f)
        {
            fade = 1f;
        }
        else
        {
            fade = 0f;
            fading = StartCoroutine(FadeTo(1f, fadeTime, false));
        }
    }

    // Fades the current song out, then stops it.
    public void FadeOut(float fadeTime = 1f)
    {
        if (fading != null) StopCoroutine(fading);
        fading = StartCoroutine(FadeTo(0f, fadeTime, true));
    }

    IEnumerator FadeTo(float target, float time, bool stopWhenDone)
    {
        float start = fade;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, time);
            fade = Mathf.Lerp(start, target, Mathf.Clamp01(t));
            yield return null;
        }
        fade = target;
        if (stopWhenDone) source.Stop();
        fading = null;
    }
}