// GAMESETTINGS.CS  -  no setup, don't put it on anything.
// Saved settings any script can read, like GameSettings.FamilyFriendly.
// PlayerPrefs saves them on the player's computer, so they stick between play sessions.

using UnityEngine;

public static class GameSettings
{
    // ON by default (the 1 is the default value if nothing's been saved yet)
    public static bool FamilyFriendly
    {
        get { return PlayerPrefs.GetInt("FamilyFriendly", 1) == 1; }
        set { PlayerPrefs.SetInt("FamilyFriendly", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    // controller rumble, ON by default
    public static bool Rumble
    {
        get { return PlayerPrefs.GetInt("Rumble", 1) == 1; }
        set { PlayerPrefs.SetInt("Rumble", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    // camera leaning toward the cursor, ON by default
    public static bool CameraLean
    {
        get { return PlayerPrefs.GetInt("CameraLean", 1) == 1; }
        set { PlayerPrefs.SetInt("CameraLean", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    // --- VOLUMES (0 to 1) ---
    // kept in memory too, so sounds don't have to read the save file every time they play
    static float musicVolume = -1f;
    static float sfxVolume = -1f;

    public static float MusicVolume
    {
        get
        {
            if (musicVolume < 0f) musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            return musicVolume;
        }
        set
        {
            musicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.Save();
        }
    }

    public static float SfxVolume
    {
        get
        {
            if (sfxVolume < 0f) sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
            return sfxVolume;
        }
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
            PlayerPrefs.Save();
        }
    }

    // runs when the game starts: keeps the overall volume at full,
    // since music and sound effects now have their own sliders
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyOnStart()
    {
        AudioListener.volume = 1f;
    }
}