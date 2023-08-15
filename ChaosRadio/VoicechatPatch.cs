using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using System.Reflection.Emit;
using VoiceChat.Networking;

namespace Xname.ChaosRadio;

[HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
internal static class VoicechatPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        int index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Ldloc_0);

        var local = generator.DeclareLocal(typeof(VoicechattingEventArgs)).LocalIndex;
        var returnLabel = generator.DefineLabel();
        newInstructions[newInstructions.Count - 1].labels.Add(returnLabel);

        newInstructions.InsertRange(index, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Ldc_I4_1),
            new CodeInstruction(OpCodes.Newobj, AccessTools.GetDeclaredConstructors(typeof(VoicechattingEventArgs))[0]),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Stloc_S, local),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RadioHandler), nameof(RadioHandler.OnPlayerVoiceChatting))),
            new CodeInstruction(OpCodes.Ldloc_S, local),
            new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(VoicechattingEventArgs), nameof(VoicechattingEventArgs.IsAllowed))),
            new CodeInstruction(OpCodes.Brfalse_S, returnLabel),
        });

        foreach (var instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
}

internal sealed class VoicechattingEventArgs : System.EventArgs
{
    public VoicechattingEventArgs(NetworkConnection connection, VoiceMessage message, bool isAllowed = true)
    {
        Connection = connection;
        Message = message;
        IsAllowed = isAllowed;
    }

    public NetworkConnection Connection { get; }

    public VoiceMessage Message { get; }

    public bool IsAllowed { get; set; }
}
