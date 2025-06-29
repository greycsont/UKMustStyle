using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using System;
using System.Reflection;
using HarmonyLib;


namespace StyleGoRound;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("ULTRAKILL.exe")]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony;
    internal static ManualLogSource Log;
    private void Awake()
    {
        Log = base.Logger;
        LoadMainModule();
        LoadOptionalModule();

        PatchHarmony(); 
        Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void LoadMainModule()
    {
        
    }
    private void LoadOptionalModule()
    {

    }
    private void PatchHarmony()
    {
        harmony = new Harmony(PluginInfo.PLUGIN_GUID+".harmony");
        harmony.PatchAll();
    }

}
