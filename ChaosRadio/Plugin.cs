using HarmonyLib;
using InventorySystem;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Respawning;

namespace Xname.ChaosRadio;

internal sealed class Plugin
{
    [PluginConfig]
    public static Config Config;

    [PluginPriority(LoadPriority.Medium)]
    [PluginEntryPoint("Chaos Radio", "1.0.0", "Adds a custom Radio for Chaos Insurgency", "Xname")]
    public void Load()
    {
        _harmony.PatchAll();

        EventManager.RegisterEvents(this);
        RespawnManager.ServerOnRespawned += RadioHandler.RespawnManager_ServerOnRespawned;
    }

    [PluginUnload]
    public void Unload()
    {
        _harmony.UnpatchAll();

        EventManager.UnregisterEvents(this);
        RespawnManager.ServerOnRespawned -= RadioHandler.RespawnManager_ServerOnRespawned;
    }

    private static readonly Harmony _harmony = new("xname.chaosradio.pl");

    [PluginEvent(ServerEventType.WaitingForPlayers)]
    private void OnWaitingForPlayers()
        => RadioHandler.ChaosRadios.Clear();

#if DEBUG
    [PluginEvent(ServerEventType.RoundStart)]
    private void OnRoundStart()
    {
        Timing.CallDelayed(2f, () =>
        {
            foreach (var player in Player.GetPlayers())
            {
                RadioHandler.ChaosRadios.Add(player.ReferenceHub.inventory.ServerAddItem(ItemType.Radio).ItemSerial);
            }
        });
    }
#endif
}