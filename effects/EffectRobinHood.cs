using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectRobinHood : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Robin Hood";
    public override string TranslationName { get; set; } = "robin_hood";
    public override double Probability { get; set; } = 1;
    // Applies once, so rolling it again while it is active is fine
    public override bool Repeatable => true;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("amount", "2000");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.InGameMoneyServices is null)  return;

        if(!RawParameters.TryGetValue("amount", out var amountStr) || !int.TryParse(amountStr, out var amount))
            return;

        var target = Utilities.Helpers.GetEnemies(playerController)
            .Where(p => p.InGameMoneyServices != null && p.InGameMoneyServices.Account > 0)
            .OrderBy(_ => Guid.NewGuid())
            .FirstOrDefault();

        if (target == null)
        {
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "failed")));
            return;
        }

        var stolen = Math.Min(amount, target.InGameMoneyServices!.Account);
        Utilities.Helpers.SetMoney(target, target.InGameMoneyServices.Account - stolen);
        Utilities.Helpers.SetMoney(playerController, playerController.InGameMoneyServices.Account + stolen);

        PrintDescription(playerController, TranslationName, stolen.ToString(), target.PlayerName);
        target.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "victim"), playerController.PlayerName, stolen));
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}
