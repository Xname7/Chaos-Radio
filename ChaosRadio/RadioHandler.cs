using InventorySystem;
using MEC;
using PlayerRoles;
using PlayerRoles.Voice;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System.Collections.Generic;
using System.Linq;
using VoiceChat;

namespace Xname.ChaosRadio;

internal sealed class RadioHandler
{
    public static readonly HashSet<ushort> ChaosRadios = new();

    public static void OnPlayerVoiceChatting(VoicechattingEventArgs ev)
    {
        var message = ev.Message;
        if (message.Channel is not VoiceChatChannel.Radio)
            return;

        var speakerChaosRadio = Plugin.Config.ChaosRadioNormalRadio || HasChaosRadio(message.Speaker);
        foreach (ReferenceHub hub in ReferenceHub.AllHubs)
        {
            if (hub.roleManager.CurrentRole is not IVoiceRole voiceRole)
                continue;

            var channel = voiceRole.VoiceModule.ValidateReceive(message.Speaker, VoiceChatChannel.Radio);
            if (channel is VoiceChatChannel.None)
                continue;

            var recieverChaosRadio = Plugin.Config.ChaosRadioNormalRadio || HasChaosRadio(hub);
            if (speakerChaosRadio && recieverChaosRadio)
            {
                hub.connectionToClient.Send(message);
                continue;
            }

            if (!speakerChaosRadio && !recieverChaosRadio)
                hub.connectionToClient.Send(message);
        }

        ev.IsAllowed = false;
    }

    private static bool HasChaosRadio(ReferenceHub player)
    {
        var radio = player.inventory.UserInventory.Items.FirstOrDefault(x => x.Value.ItemTypeId is ItemType.Radio);
        if (radio.Value is null)
            return false;

        if (!ChaosRadios.Contains(radio.Key))
            return false;

        return true;
    }

    [PluginEvent(ServerEventType.WaitingForPlayers)]
    private void OnWaitingForPlayers()
        => ChaosRadios.Clear();

    [PluginEvent(ServerEventType.PlayerChangeRole)]
    private void OnPlayerChangeRole(PlayerChangeRoleEvent ev)
    {
        if (ev.ChangeReason is not RoleChangeReason.Respawn)
            return;

        if (ev.NewRole.GetTeam() is not Team.ChaosInsurgency)
            return;

        Timing.CallDelayed(1f, () =>
        {
            ChaosRadios.Add(ev.Player.AddItem(ItemType.Radio).ItemSerial);
            ev.Player.ReceiveHint(Plugin.Config.RespawnMessage, 10);
        });
    }

    [PluginEvent(ServerEventType.PlayerSearchedPickup)]
    private void OnPlayerSearchedPickup(PlayerSearchedPickupEvent ev)
    {
        if (!ChaosRadios.Contains(ev.Item.Info.Serial))
            return;

        ev.Player.ReceiveHint(Plugin.Config.PickedUpMessage, 5);
    }

    [PluginEvent(ServerEventType.PlayerChangeItem)]
    private void OnPlayerChangeItem(PlayerChangeItemEvent ev)
    {
        if (!ChaosRadios.Contains(ev.NewItem))
            return;

        ev.Player.ReceiveHint(Plugin.Config.ItemSelectedMessage, 5);
    }

#if DEBUG
    [PluginEvent(ServerEventType.RoundStart)]
    private void OnRoundStart()
    {
        Timing.CallDelayed(2f, () =>
        {
            foreach (var player in Player.GetPlayers())
                ChaosRadios.Add(player.ReferenceHub.inventory.ServerAddItem(ItemType.Radio).ItemSerial);
        });
    }
#endif
}
