using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectMirrorDamage : EffectBaseEvent<EventPlayerHurt>, IEffectParameter
{
    private bool _allowTeamDamage = false;
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Mirror Damage";
    public override string TranslationName { get; set; } = "mirror_damage";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("reflectedDamageScaleFactor", "0.5");

        var friendlyFire = ConVar.Find("mp_friendlyfire");
        if(friendlyFire is null)
            return;

        _allowTeamDamage = friendlyFire.GetPrimitiveValue<bool>();
    }

    public override HookResult OnEvent(EventPlayerHurt @event, GameEventInfo eventInfo)
    {
        if (@event.Attacker == null || @event.Userid == null || @event.Attacker.PlayerPawn.Value == null) return HookResult.Continue;

        CCSPlayerController attackerController = @event.Attacker;
        CCSPlayerController victimController = @event.Userid;

        // check if the attacker is a valid player and the victim is a valid player or bot
        if(!attackerController.IsValidPly() || victimController is {IsValid: false, PlayerPawn.IsValid: false})
            return HookResult.Continue;

        // check if the attacker is the victim or if they are on the same team
        if(attackerController == victimController || (!_allowTeamDamage && attackerController.TeamNum == victimController.TeamNum))
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
}