using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectNothing : EffectBaseRegular
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Nothing";
    public override string TranslationName { get; set; } = "nothing";
    public override double Probability { get; set; } = 3;
    public override bool ShowDescriptionOnRoll { get; set; } = true;

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}