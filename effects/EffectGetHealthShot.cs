using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectGetHealthShot : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Healthshot";
    public override string TranslationName { get; set; } = "healthshot";
    public override double Probability { get; set; } = 2;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || 
            playerController.PlayerPawn.Value is null ||
            playerController.InGameMoneyServices is null
        )  return;

        playerController!.GiveNamedItem("weapon_healthshot");
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}