// MUSICMANAGER.CS  -  put it on an empty GameObject called "MusicManager" in each scene with music.
//
// IN ONE SENTENCE: loops this scene's song, fades it in when the scene starts, and follows
// the Music slider from the Options menu (even while you're dragging it).
//
// SETUP: drag the song into "Track". Each scene gets its own MusicManager with its own song,
// so the menu, duck booth and space booth can all sound different.

using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip track;                        // this scene's song
    [Range(0f, 1f)] public float trackVolume = 1f; // balance a song that's too loud/quiet compared to others
    public float fadeInTime = 1f;                  // seconds to fade the song in

    AudioSource source;
    float fade;   // goes 0 -> 1 during the fade-in

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = track;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;   // plain 2D sound
        source.volume = 0f;
    }

    void Start()
    {
        if (track != null) source.Play();
    }

    void Update()
    {
        // fade in, then just follow the Music slider
        fade = Mathf.MoveTowards(fade, 1f, Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeInTime));
        source.volume = trackVolume * GameSettings.MusicVolume * fade;
    }
}