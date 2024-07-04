
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectLessSpeed : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Less Speed";
    public override string TranslationName { get; set; } = "less_speed";
    public override double Probability { get; set; } = 2;
    public Dictionary<string, string> RawParameters {get; set; } = new();
    public override bool ShowDescriptionOnRoll { get; set; } = false;

    public override void Initialize()
    {
        RawParameters.Add("speedScaleFactor", "0.5");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        if(!RawParameters.TryGetValue("speedScaleFactor", out var speedStr))
            return;

        if(!float.TryParse(speedStr, out var speedF))
            return;

        //playerController!.PlayerPawn.Value.VelocityModifier = 0.1f;
        playerController!.PlayerPawn.Value.MovementServices!.Maxspeed *= speedF;
        PrintDescription(playerController, TranslationName, ((1-speedF) * 100)+"%");
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}