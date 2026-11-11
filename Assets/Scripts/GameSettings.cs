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
}