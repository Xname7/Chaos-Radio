namespace Xname.ChaosRadio;

internal sealed class Config
{
    public bool Debug { get; set; } = false;

    public bool ChaosRadioNormalRadio { get; set; } = false;

    public string RespawnMessage { get; set; } = "You have a <color=green>Chaos</color> radio in your inventory. Use this to communicate with other <color=green>Chaos</color>.";

    public string PickedUpMessage { get; set; } = "You have picked up a <color=green>Chaos</color> radio.";

    public string ItemSelectedMessage { get; set; } = "Remember; this is a <color=green>Chaos</color> radio.";
}