// SOUNDMANAGER.CS  -  put it on one empty GameObject in the scene.
//
// IN ONE SENTENCE: any script can call SoundManager.Instance.Play(clip) to play a sound,
// with a little random pitch so repeated sounds don't get annoying.
//
// It keeps a few AudioSources and takes turns using them, so fast shots don't cut each other off.

using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;   // lets any script find this one easily

    public int voices = 8;                 // how many sounds can overlap at once

    AudioSource[] sources;
    int next;

    void Awake()
    {
        Instance = this;

        sources = new AudioSource[voices];
        for (int i = 0; i < voices; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].spatialBlend = 0f;   // 0 = plain 2D sound, same volume everywhere
        }
    }

    public void Play(AudioClip clip, float volume = 1f, float pitchVariation = 0.1f)
    {
        if (clip == null) return;

        AudioSource s = sources[next];
        next = (next + 1) % sources.Length;   // move to the next source for next time

        s.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        s.PlayOneShot(clip, volume);
    }
}