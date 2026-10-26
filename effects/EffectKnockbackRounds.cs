using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectKnockbackRounds : EffectBaseEvent<EventPlayerHurt>, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Knockback Rounds";
    public override string TranslationName { get; set; } = "knockback_rounds";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("pushPerDamage", "12");
        RawParameters.Add("maxPush", "700");
        RawParameters.Add("lift", "180");
    }

    // The holder's hits push enemies away from the holder
    public override HookResult OnEvent(EventPlayerHurt @event, GameEventInfo eventInfo)
    {
        var attacker = @event.Attacker;
        var victim = @event.Userid;

        if (!IsActiveOn(attacker) || victim == null || !Utilities.Helpers.IsEnemy(attacker!, victim))
            return HookResult.Continue;

        if (!Knockback.TryGetParameters(RawParameters, out var pushPerDamage, out var maxPush, out var lift))
            return HookResult.Continue;

        Knockback.Push(attacker!, victim!, @event.DmgHealth, pushPerDamage, maxPush, lift);

        return HookResult.Continue;
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}

internal static class Knockback
{
    public static bool TryGetParameters(Dictionary<string, string> parameters, out float pushPerDamage, out float maxPush, out float lift)
    {
        pushPerDamage = maxPush = lift = 0;

        return parameters.TryGetValue("pushPerDamage", out var pushStr) && float.TryParse(pushStr, out pushPerDamage) &&
            parameters.TryGetValue("maxPush", out var maxStr) && float.TryParse(maxStr, out maxPush) &&
            parameters.TryGetValue("lift", out var liftStr) && float.TryParse(liftStr, out lift);
    }

    // Push the victim away from the attacker, harder with more damage
    public static void Push(CCSPlayerController attacker, CCSPlayerController victim, int damage, float pushPerDamage, float maxPush, float lift)
    {
        var victimPawn = victim.PlayerPawn.Value;
        var from = attacker.PlayerPawn.Value?.AbsOrigin;
        var to = victimPawn?.AbsOrigin;

        if (victimPawn == null || from == null || to == null || victimPawn.Health <= 0)
            return;

        float dx = to.X - from.X;
        float dy = to.Y - from.Y;
        float length = MathF.Sqrt(dx * dx + dy * dy);
        if (length < 1f)  return;

        float push = Math.Min(maxPush, damage * pushPerDamage);
        var current = victimPawn.AbsVelocity;
        var velocity = new Vector(
            current.X + dx / length * push,
            current.Y + dy / length * push,
            Math.Max(current.Z, lift)
        );

        // Setting the velocity through Teleport lets the movement code take it over, also on the ground
        // https://github.com/Kandru/cs2-roll-the-dice/blob/0863bbaff25201bd73186f4435ac0f2e8737bdca/src/utils/Motion.cs
        Server.NextFrame(() =>
        {
            if (victimPawn.IsValid)
                victimPawn.Teleport(null, null, velocity);
        });
    }
}
