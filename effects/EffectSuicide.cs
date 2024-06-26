
using System.ComponentModel;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectSuicide : EffectBaseRegular
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Suicide".__("effect_name_suicide");
    public override string Description { get; set; } = "Unlucky..".__("effect_description_suicide");
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;
        
        if(playerController!.PlayerPawn.Value.LifeState != 0)
            return;

        playerController.PlayerPawn.Value.CommitSuicide(true, false);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}