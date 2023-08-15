using HarmonyLib;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace Xname.ChaosRadio;

internal sealed class Plugin
{
    [PluginConfig]
    public static Config Config;

    [PluginPriority(LoadPriority.Medium)]
    [PluginEntryPoint("Chaos Radio", "1.0.0", "Adds a custom Radio for Chaos Insurgency", "Xname")]
    public void Load()
    {
        _handler = new();
        _harmony.PatchAll();
        EventManager.RegisterEvents(_handler);
    }

    [PluginUnload]
    public void Unload()
    {
        _harmony.UnpatchAll();
        EventManager.UnregisterEvents(_handler);
        _handler = null;
    }

    private static readonly Harmony _harmony = new("xname.chaosradio.pl");
    private static RadioHandler _handler;
}