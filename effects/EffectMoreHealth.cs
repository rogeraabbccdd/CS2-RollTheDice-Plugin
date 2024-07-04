using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectMoreHealth : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "More Health";
    public override string TranslationName { get; set; } = "more_health";
    public override double Probability { get; set; } = 3;
    public Dictionary<string, string> RawParameters {get; set; } = new();
    public override bool ShowDescriptionOnRoll { get; set; } = false;

    public override void Initialize()
    {
        RawParameters.Add("healthSummand", "10");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        if(!RawParameters.TryGetValue("healthSummand", out var healthStr))
            return;

        if(!int.TryParse(healthStr, out var healthInt))
            return;

        playerController!.PlayerPawn.Value.Health += healthInt;

        playerController.RefreshUI();
        PrintDescription(playerController, TranslationName, healthStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}