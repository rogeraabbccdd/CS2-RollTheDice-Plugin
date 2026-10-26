using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectHalfMoney : EffectBaseRegular
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Half Money";
    public override string TranslationName { get; set; } = "half_money";
    public override double Probability { get; set; } = 1;
    // Applies once, so rolling it again while it is active is fine
    public override bool Repeatable => true;
    public override bool ShowDescriptionOnRoll { get; set; } = false;

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.InGameMoneyServices is null)  return;

        var money = playerController.InGameMoneyServices.Account;
        Utilities.Helpers.SetMoney(playerController, money / 2);

        PrintDescription(playerController, TranslationName, playerController.InGameMoneyServices.Account.ToString());
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}
