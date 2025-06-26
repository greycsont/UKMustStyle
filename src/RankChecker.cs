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
            var rankIndex = global::StyleHUD.Instance.rankIndex;
            Plugin.Log.LogInfo($"Current rank index: {rankIndex}");
            return rankIndex > 4;
        }
        catch(Exception e)
        {
            Plugin.Log.LogError("Error checking rank: " + e.Message);
            return true;
        }

    }
}