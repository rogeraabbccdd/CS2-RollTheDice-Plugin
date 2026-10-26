using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectKnockback : EffectBaseEvent<EventPlayerHurt>, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Knockback";
    public override string TranslationName { get; set; } = "knockback";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("pushPerDamage", "12");
        RawParameters.Add("maxPush", "700");
        RawParameters.Add("lift", "180");
    }

    // The holder gets pushed away from enemies who hit them
    public override HookResult OnEvent(EventPlayerHurt @event, GameEventInfo eventInfo)
    {
        var attacker = @event.Attacker;
        var victim = @event.Userid;

        if (!IsActiveOn(victim) || attacker == null || !Utilities.Helpers.IsEnemy(victim!, attacker))
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
