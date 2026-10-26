using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectMoreSpeed : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "More Speed";
    public override string TranslationName { get; set; } = "more_speed";
    public override double Probability { get; set; } = 2;
    public override string? ConflictGroup => "speed";
    public Dictionary<string, string> RawParameters {get; set; } = new();
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    // VelocityModifier is reset when the player gets hit, so the speed has to be applied again
    public static Dictionary<nint, float> SpeedPlayers = new();

    public override void Initialize()
    {
        RawParameters.Add("speedScaleFactor", "2.0");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        if(!RawParameters.TryGetValue("speedScaleFactor", out var speedStr))
            return;

        if(!float.TryParse(speedStr, out var speedF))
            return;

        SpeedPlayers[playerController.Handle] = speedF;
        SetSpeed(playerController, speedF);
        PrintDescription(playerController, TranslationName, ((speedF-1) * 100)+"%");
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if (SpeedPlayers.Remove(playerController.Handle))
            SetSpeed(playerController, 1.0f);
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }

    public static void OnPlayerHurt(CCSPlayerController? playerController)
    {
        if (playerController == null || !SpeedPlayers.ContainsKey(playerController.Handle))  return;

        // Delay 3 frames so the game has finished changing VelocityModifier
        // https://github.com/Kandru/cs2-roll-the-dice/blob/0863bbaff25201bd73186f4435ac0f2e8737bdca/src/dices/IncreaseSpeed.cs
        Server.NextFrame(() => Server.NextFrame(() => Server.NextFrame(() =>
        {
            if (!playerController.IsValid || !SpeedPlayers.TryGetValue(playerController.Handle, out var speed))
                return;

            SetSpeed(playerController, speed);
        })));
    }

    private static void SetSpeed(CCSPlayerController playerController, float speed)
    {
        var pawn = playerController.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid)  return;

        pawn.VelocityModifier = speed;
        CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_flVelocityModifier");
    }
}
