using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectLessFOV : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Less FOV".__("effect_name_less_fov");
    public override string Description { get; set; } = "Less FOV.".__("effect_description_less_fov");
    public override double Probability { get; set; } = 2;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        playerController.DesiredFOV = 35;
        playerController.RefreshUI();
        
        PrintDescription(playerController, "effect_description_less_fov");
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        playerController.DesiredFOV = 90;
        playerController.RefreshUI();
    }
}