using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class JetpackUser
{
    public float Fuel { get; set; }
    // Ground height of the last take-off, the ceiling is measured from here
    public float GroundZ { get; set; }
    public int ShownSegments { get; set; } = -1;
}

public class EffectJetpack : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Jetpack";
    public override string TranslationName { get; set; } = "jetpack";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    public static Dictionary<nint, JetpackUser> JetpackUsers = new();
    private static float FuelSeconds = 3f;
    private static float Thrust = 1600f;
    private static float MaxUpSpeed = 300f;
    private static float RechargePerSecond = 1f;
    private static float Ceiling = 300f;
    // Fewer segments means fewer redraws while flying
    private const int FUEL_BAR_SEGMENTS = 5;

    public override void Initialize()
    {
        RawParameters.Add("fuelSeconds", "3");
        // Upward acceleration in units/s², gravity is 800
        RawParameters.Add("thrust", "1600");
        RawParameters.Add("maxUpSpeed", "300");
        RawParameters.Add("rechargePerSecond", "1");
        // Max height above the take-off point, so players can't reach places they shouldn't
        RawParameters.Add("ceiling", "300");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value?.AbsOrigin == null)  return;

        if (!TryGetFloat("fuelSeconds", out var fuelSeconds) || !TryGetFloat("thrust", out var thrust) ||
            !TryGetFloat("maxUpSpeed", out var maxUpSpeed) || !TryGetFloat("rechargePerSecond", out var recharge) ||
            !TryGetFloat("ceiling", out var ceiling))
            return;

        FuelSeconds = fuelSeconds;
        Thrust = thrust;
        MaxUpSpeed = maxUpSpeed;
        RechargePerSecond = recharge;
        Ceiling = ceiling;

        JetpackUsers[playerController.Handle] = new JetpackUser
        {
            Fuel = FuelSeconds,
            GroundZ = playerController.PlayerPawn.Value.AbsOrigin.Z
        };
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        JetpackUsers.Remove(playerController.Handle);
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }

    private bool TryGetFloat(string key, out float value)
    {
        value = 0;
        return RawParameters.TryGetValue(key, out var str) && float.TryParse(str, out value);
    }

    // Holding jump in the air pushes up while there is fuel, the fuel refills on the ground
    public static void OnGameFrame()
    {
        if (JetpackUsers.Count == 0)  return;

        var dt = Server.TickInterval;

        foreach (var player in CounterStrikeSharp.API.Utilities.GetPlayers())
        {
            if (!JetpackUsers.TryGetValue(player.Handle, out var user))
                continue;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null || pawn.AbsOrigin == null || !player.PawnIsAlive)
                continue;

            bool onGround = ((PlayerFlags)pawn.Flags & PlayerFlags.FL_ONGROUND) != 0;
            bool jumpHeld = (player.Buttons & PlayerButtons.Jump) != 0;

            if (onGround)
            {
                user.GroundZ = pawn.AbsOrigin.Z;
                user.Fuel = Math.Min(FuelSeconds, user.Fuel + RechargePerSecond * dt);
            }
            else if (jumpHeld && user.Fuel > 0)
            {
                user.Fuel = Math.Max(0, user.Fuel - dt);

                // Above the ceiling the thrust only holds the height
                if (pawn.AbsOrigin.Z - user.GroundZ >= Ceiling)
                    pawn.AbsVelocity.Z = Math.Max(pawn.AbsVelocity.Z, 0);
                else
                    pawn.AbsVelocity.Z = Math.Min(MaxUpSpeed, pawn.AbsVelocity.Z + Thrust * dt);
            }

            ShowFuel(player, user);
        }
    }

    // Every new center message plays the fade in again, so only redraw when the bar changes
    private static void ShowFuel(CCSPlayerController player, JetpackUser user)
    {
        if (!player.IsValidPly())  return;

        int filled = (int)Math.Ceiling(user.Fuel / Math.Max(0.01f, FuelSeconds) * FUEL_BAR_SEGMENTS);
        if (filled == user.ShownSegments)  return;

        // Nothing to show when the tank was full before and still is
        bool wasFull = user.ShownSegments == -1 || user.ShownSegments == FUEL_BAR_SEGMENTS;
        user.ShownSegments = filled;
        if (wasFull && filled == FUEL_BAR_SEGMENTS)  return;

        var bar = new string('■', filled) + new string('□', FUEL_BAR_SEGMENTS - filled);
        player.PrintToCenter($"{Log.GetLocalizedText("effect_event_jetpack")} {bar}");
    }
}
