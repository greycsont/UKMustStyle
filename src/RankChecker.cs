


using HarmonyLib;

namespace Only;

public static class RankChecker
{
    public static bool IsRanked()
    {
        return AccessTools.Field(typeof(StyleHUD), "_rankIndex")?.GetValue(null) is int rankIndex && rankIndex > 4;
    }
}