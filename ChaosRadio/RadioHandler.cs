using InventorySystem;
using PlayerRoles.Voice;
using Respawning;
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

        var speakerChaosRadio = HasChaosRadio(message.Speaker);
        foreach (ReferenceHub hub in ReferenceHub.AllHubs)
        {
            if (hub.roleManager.CurrentRole is not IVoiceRole voiceRole)
                continue;

            VoiceChatChannel channel = voiceRole.VoiceModule.ValidateReceive(message.Speaker, VoiceChatChannel.Radio);
            if (channel is VoiceChatChannel.None)
                continue;

            var recieverChaosRadio = HasChaosRadio(hub);
            if (speakerChaosRadio && recieverChaosRadio)
            {
                hub.connectionToClient.Send(message);
                continue;
            }

            if (!speakerChaosRadio && !recieverChaosRadio)
            {
                hub.connectionToClient.Send(message);
            }
        }

        ev.IsAllowed = false;
    }

    public static void RespawnManager_ServerOnRespawned(SpawnableTeamType team, List<ReferenceHub> players)
    {
        if (team is not SpawnableTeamType.ChaosInsurgency)
            return;

        players.ForEach(x => ChaosRadios.Add(x.inventory.ServerAddItem(ItemType.Radio).ItemSerial));
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
}
