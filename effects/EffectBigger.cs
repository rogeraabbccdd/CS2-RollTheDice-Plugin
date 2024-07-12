using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectBigger : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Bigger";
    public override string TranslationName { get; set; } = "bigger";
    public override double Probability { get; set; } = 1;
    public Dictionary<string, string> RawParameters {get; set; } = new();
    public override bool ShowDescriptionOnRoll { get; set; } = false;

    public override void Initialize()
    {
        RawParameters.Add("scale", "1.2");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || 
            playerController.PlayerPawn.Value == null ||
            playerController.PlayerPawn.Value.Entity == null
        )  return;

        if(!RawParameters.TryGetValue("scale", out var scale))
            return;

        playerController.PlayerPawn.Value.Entity.EntityInstance.AddEntityIOEvent("SetScale", null, null, scale);

        PrintDescription(playerController, TranslationName, scale);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null || 
            playerController.PlayerPawn.Value == null ||
            playerController.PlayerPawn.Value.Entity == null
        )  return;

        playerController.PlayerPawn.Value.Entity.EntityInstance.AddEntityIOEvent("SetScale", null, null, "1");
    }
}