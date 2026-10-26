using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectIceSkating : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Ice Skating";
    public override string TranslationName { get; set; } = "ice_skating";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    // Controller handle -> friction to keep on the player
    public static Dictionary<nint, float> SkatingPlayers = new();
    private const float NORMAL_FRICTION = 1.0f;

    public override void Initialize()
    {
        RawParameters.Add("friction", "0.1");
    }

    // Lowering the friction makes stopping and turning slide, setting the velocity does not work
    // https://github.com/Kandru/cs2-roll-the-dice/blob/0863bbaff25201bd73186f4435ac0f2e8737bdca/src/dices/IceSkating.cs
    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        if(!RawParameters.TryGetValue("friction", out var frictionStr) || !float.TryParse(frictionStr, out var friction))
            return;

        friction = Math.Clamp(friction, 0.01f, 1.0f);
        SkatingPlayers[playerController.Handle] = friction;
        SetFriction(playerController.PlayerPawn.Value, friction);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if (SkatingPlayers.Remove(playerController.Handle))
            SetFriction(playerController.PlayerPawn.Value, NORMAL_FRICTION);
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }

    public static void OnGameFrame()
    {
        if (SkatingPlayers.Count == 0)  return;

        foreach (var player in CounterStrikeSharp.API.Utilities.GetPlayers())
        {
            if (SkatingPlayers.TryGetValue(player.Handle, out var friction))
                SetFriction(player.PlayerPawn.Value, friction);
        }
    }

    private static void SetFriction(CCSPlayerPawn? pawn, float friction)
    {
        if (pawn == null || !pawn.IsValid || pawn.Friction == friction)  return;

        pawn.Friction = friction;
        CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CBaseEntity", "m_flFriction");
    }
}
