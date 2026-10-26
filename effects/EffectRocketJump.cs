using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectRocketJump : EffectBaseEvent<EventWeaponFire>, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Rocket Jump";
    public override string TranslationName { get; set; } = "rocket_jump";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    private static readonly string[] IGNORED_WEAPONS = { "knife", "bayonet", "grenade", "flashbang", "molotov", "decoy", "taser", "c4", "healthshot" };
    private static readonly string[] SHOTGUNS = { "nova", "xm1014", "mag7", "sawedoff" };
    private static readonly string[] SNIPERS = { "awp", "ssg08", "scar20", "g3sg1" };

    public override void Initialize()
    {
        // Push per shot in units/s against the view direction
        RawParameters.Add("defaultForce", "50");
        RawParameters.Add("shotgunForce", "450");
        RawParameters.Add("sniperForce", "350");
        // Automatic weapons would fly forever without a limit
        RawParameters.Add("maxUpSpeed", "500");
    }

    // Every shot pushes the holder against the view direction, shooting down lifts them up
    public override HookResult OnEvent(EventWeaponFire @event, GameEventInfo eventInfo)
    {
        var shooter = @event.Userid;
        var weapon = @event.Weapon;

        if (!IsActiveOn(shooter) || string.IsNullOrEmpty(weapon) || IGNORED_WEAPONS.Any(weapon.Contains))
            return HookResult.Continue;

        var pawn = shooter!.PlayerPawn.Value;
        if (pawn == null || pawn.AbsVelocity == null)
            return HookResult.Continue;

        var forceKey = SHOTGUNS.Any(weapon.Contains) ? "shotgunForce" : SNIPERS.Any(weapon.Contains) ? "sniperForce" : "defaultForce";
        if (!RawParameters.TryGetValue(forceKey, out var forceStr) || !float.TryParse(forceStr, out var force))
            return HookResult.Continue;

        if (!RawParameters.TryGetValue("maxUpSpeed", out var maxUpStr) || !float.TryParse(maxUpStr, out var maxUpSpeed))
            return HookResult.Continue;

        // Forward vector of the view, positive pitch means looking down
        float pitch = pawn.EyeAngles.X * MathF.PI / 180f;
        float yaw = pawn.EyeAngles.Y * MathF.PI / 180f;
        float forwardX = MathF.Cos(pitch) * MathF.Cos(yaw);
        float forwardY = MathF.Cos(pitch) * MathF.Sin(yaw);
        float forwardZ = -MathF.Sin(pitch);

        var current = pawn.AbsVelocity;
        var velocity = new Vector(
            current.X - forwardX * force,
            current.Y - forwardY * force,
            Math.Min(maxUpSpeed, current.Z - forwardZ * force)
        );

        // Setting the velocity through Teleport lets the movement code take it over, also on the ground
        // https://github.com/Kandru/cs2-roll-the-dice/blob/0863bbaff25201bd73186f4435ac0f2e8737bdca/src/utils/Motion.cs
        Server.NextFrame(() =>
        {
            if (pawn.IsValid)
                pawn.Teleport(null, null, velocity);
        });

        return HookResult.Continue;
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}
