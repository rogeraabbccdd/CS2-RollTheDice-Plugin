using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectArmorBreak : EffectBaseRegular
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Armor Break";
    public override string TranslationName { get; set; } = "armor_break";
    public override double Probability { get; set; } = 1;
    // Applies once, so rolling it again while it is active is fine
    public override bool Repeatable => true;
    public override bool ShowDescriptionOnRoll { get; set; } = true;

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        var pawn = playerController?.PlayerPawn.Value;
        if (pawn == null)  return;

        pawn.ArmorValue = 0;
        CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_ArmorValue");

        if (pawn.ItemServices != null)
        {
            new CCSPlayer_ItemServices(pawn.ItemServices.Handle).HasHelmet = false;
            CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_pItemServices");
        }
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}
