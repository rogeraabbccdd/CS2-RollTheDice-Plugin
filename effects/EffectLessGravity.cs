using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectLessGravity : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Less Gravity";
    public override string TranslationName { get; set; } = "less_gravity";
    public override double Probability { get; set; } = 2;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("gravityScaleFactor", "0.5");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        if(!RawParameters.TryGetValue("gravityScaleFactor", out var gravityStr))
            return;

        if(!float.TryParse(gravityStr, out var gravityFloat))
            return;

        playerController!.PlayerPawn.Value.GravityScale *= gravityFloat;
        PrintDescription(playerController, TranslationName, ((1-gravityFloat)*100f)+"%");
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        playerController!.PlayerPawn.Value.GravityScale = 1;
    }
}