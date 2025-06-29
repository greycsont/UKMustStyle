using System;
using System.Reflection;
using HarmonyLib;

namespace MustStyle;

public static class RankChecker
{
    public static bool IsRanked()
    {
        try
        {
            return global::StyleHUD.Instance.rankIndex > 3;
        }
        catch(Exception e)
        {
            Plugin.Log.LogError("Error checking rank: " + e.Message);
            return true;
        }

    }
}