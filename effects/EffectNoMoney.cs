using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectNoMoney : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "No Money";
    public override string TranslationName { get; set; } = "no_money";
    public override double Probability { get; set; } = 1;
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

        playerController.InGameMoneyServices.Account = 0;
        playerController.RefreshUI();
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}