using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectGlassCannon : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Glass Cannon";
    public override string TranslationName { get; set; } = "glass_cannon";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    // Controller handle -> damage multiplier
    public static Dictionary<nint, float> GlassCannonPlayers = new();

    public override void Initialize()
    {
        RawParameters.Add("damageMultiplier", "2");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if(!RawParameters.TryGetValue("damageMultiplier", out var multiplierStr) || !float.TryParse(multiplierStr, out var multiplier))
            return;

        GlassCannonPlayers[playerController.Handle] = multiplier;

        PrintDescription(playerController, TranslationName, multiplierStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        GlassCannonPlayers.Remove(playerController.Handle);
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }

    // The holder deals and takes more damage
    public static void OnPlayerTakeDamagePre(CCSPlayerPawn pawn, CTakeDamageInfo info)
    {
        if (GlassCannonPlayers.Count == 0)  return;

        var victim = pawn.Controller.Value;
        if (victim != null && GlassCannonPlayers.TryGetValue(victim.Handle, out var victimMultiplier))
            info.Damage *= victimMultiplier;

        var attacker = info.Attacker.Value;
        if (attacker == null || attacker.DesignerName != "player")  return;

        var attackerController = attacker.As<CCSPlayerPawn>().Controller.Value;
        if (attackerController != null && attackerController != victim &&
            GlassCannonPlayers.TryGetValue(attackerController.Handle, out var attackerMultiplier))
        {
            info.Damage *= attackerMultiplier;
        }
    }
}
