using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectMirrorDamage : EffectBaseEvent<EventPlayerHurt>, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Mirror Damage";
    public override string TranslationName { get; set; } = "mirror_damage";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("reflectedDamageScaleFactor", "0.5");
    }

    public override HookResult OnEvent(EventPlayerHurt @event, GameEventInfo eventInfo)
    {
        if (@event.Attacker == null || @event.Userid == null || @event.Attacker.PlayerPawn.Value == null) return HookResult.Continue;

        CCSPlayerController attackerController = @event.Attacker;
        CCSPlayerController victimController = @event.Userid;

        // check if the attacker is a valid player and the victim is a valid player or bot
        if(!attackerController.IsValidPly() || victimController is {IsValid: false, PlayerPawn.IsValid: false})
            return HookResult.Continue;

        // OnEvent is also called when the victim has this effect, only the attacker should be affected
        if(!IsActiveOn(attackerController))
            return HookResult.Continue;

        // Damage to teammates is only reflected when friendly fire is on, read every time since it can be changed
        bool friendlyFire = ConVar.Find("mp_friendlyfire")?.GetPrimitiveValue<bool>() ?? false;
        if(attackerController == victimController || (!friendlyFire && !Utilities.Helpers.IsEnemy(attackerController, victimController)))
            return HookResult.Continue;

        if(!RawParameters.TryGetValue("reflectedDamageScaleFactor", out string? healthScaledStr))
            return HookResult.Continue;

        string victimName = victimController.PlayerName;
        int damageAmount = Utilities.Helpers.GetDamageInRangePlyHealth(@event, float.Parse(healthScaledStr));

        // Health less than 1 crashes the server
        var attackerHealth = attackerController.PlayerPawn.Value.Health;
        attackerController.PlayerPawn.Value.Health = Math.Max(attackerHealth - damageAmount, 1);
        attackerController.RefreshUI();
        PrintMessageOnEvent(attackerController, TranslationName, damageAmount);

        return HookResult.Continue;
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}