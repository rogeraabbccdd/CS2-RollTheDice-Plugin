using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectTax : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Tax";
    public override string TranslationName { get; set; } = "tax";
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

        var enemies = Utilities.Helpers.GetEnemies(playerController)
            .Where(p => p.InGameMoneyServices != null)
            .ToList();

        var tax = Math.Min(amount, playerController.InGameMoneyServices.Account);
        Utilities.Helpers.SetMoney(playerController, playerController.InGameMoneyServices.Account - tax);

        // Split the tax between enemies, the remainder is lost
        if (enemies.Count > 0)
        {
            var share = tax / enemies.Count;
            foreach (var enemy in enemies)
            {
                Utilities.Helpers.SetMoney(enemy, enemy.InGameMoneyServices!.Account + share);
                enemy.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "event"), playerController.PlayerName, share));
            }
        }

        PrintDescription(playerController, TranslationName, tax.ToString());
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}
