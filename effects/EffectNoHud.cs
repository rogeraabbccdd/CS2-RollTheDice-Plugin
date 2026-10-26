using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectNoHud : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "No HUD";
    public override string TranslationName { get; set; } = "no_hud";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    // Controller handle -> m_iHideHUD bits added by this effect, only these are cleared on remove
    public static Dictionary<nint, uint> NoHudPlayers = new();

    public override void Initialize()
    {
        // Bits of m_iHideHUD, values from the Source engine HIDEHUD flags:
        // 4 = all, 8 = health, 256 = crosshair, 4096 = radar. 256 was confirmed in game to hide the crosshair
        RawParameters.Add("hideHudFlags", "260");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        var pawn = playerController?.PlayerPawn.Value;
        if (pawn == null)  return;

        if(!RawParameters.TryGetValue("hideHudFlags", out var flagsStr) || !uint.TryParse(flagsStr, out var flags))
            return;

        // Don't clear bits that were already set by something else
        var addedFlags = flags & ~pawn.HideHUD;
        NoHudPlayers[playerController!.Handle] = addedFlags;

        pawn.HideHUD |= addedFlags;
        CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_iHideHUD");
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null || !NoHudPlayers.Remove(playerController.Handle, out var addedFlags))  return;

        var pawn = playerController.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid)  return;

        pawn.HideHUD &= ~addedFlags;
        CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_iHideHUD");
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}
