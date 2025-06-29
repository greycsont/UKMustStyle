using System;
using System.Reflection;
using HarmonyLib;

namespace Only;

public static class RankChecker
{
    public static bool IsRanked()
    {
        try
        {
            return global::StyleHUD.Instance.rankIndex > 4;
        }
        catch(Exception e)
        {
            Plugin.Log.LogError("Error checking rank: " + e.Message);
            return true;
        }

    }
}