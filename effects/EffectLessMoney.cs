using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectLessMoney : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Less Money";
    public override string TranslationName { get; set; } = "less_money";
    public override double Probability { get; set; } = 3;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("moneySubtrahend", "1000");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || 
            playerController.PlayerPawn.Value is null ||
            playerController.InGameMoneyServices is null
        )  return;

        if(!RawParameters.TryGetValue("moneySubtrahend", out var moneyStr))
            return;

        if(!int.TryParse(moneyStr, out var moneyInt))
            return;

        var plyMoney = playerController.InGameMoneyServices.Account;
        playerController.InGameMoneyServices.Account = Math.Max(plyMoney - moneyInt, 0);
        playerController.RefreshUI();
        
        PrintDescription(playerController, TranslationName, moneyStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}