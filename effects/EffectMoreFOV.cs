using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectMoreFOV : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "More FOV".__("effect_name_more_fov");
    public override string Description { get; set; } = "More FOV.".__("effect_description_more_fov");
    public override double Probability { get; set; }  = 2;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        playerController.DesiredFOV = 145;
        playerController.RefreshUI();
        
        PrintDescription(playerController, "effect_description_more_fov");
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        playerController.DesiredFOV = 90;
        playerController.RefreshUI();
    }
}