using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectHeavyArmor : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Heavy Armor";
    public override string TranslationName { get; set; } = "heavy_armor";
    public override double Probability { get; set; } = 1;
    // Applies once, so rolling it again while it is active is fine
    public override bool Repeatable => true;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("armor", "200");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        var pawn = playerController?.PlayerPawn.Value;
        if (pawn == null)  return;

        if(!RawParameters.TryGetValue("armor", out var armorStr) || !int.TryParse(armorStr, out var armor))
            return;

        pawn.ArmorValue = armor;
        CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");

        if (pawn.ItemServices != null)
        {
            new CCSPlayer_ItemServices(pawn.ItemServices.Handle).HasHelmet = true;
            CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_pItemServices");
        }

        PrintDescription(playerController, TranslationName, armorStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}
