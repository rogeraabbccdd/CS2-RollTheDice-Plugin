using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectLessFOV : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Less FOV".__("effect_name_less_fov");
    public override string Description { get; set; } = "Your FOV is decreased to {mark}{0}".__("effect_description_less_fov");
    public override double Probability { get; set; } = 2;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("fovToSet", "35");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if(!RawParameters.TryGetValue("fovToSet", out var fovStr))
            return;

        if(!uint.TryParse(fovStr, out var fovInt))
            return;

        playerController.DesiredFOV = fovInt;
        playerController.RefreshUI();
        
        PrintDescription(playerController, "effect_description_less_fov", fovStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        playerController.DesiredFOV = 90;
        playerController.RefreshUI();
    }
}