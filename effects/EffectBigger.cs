using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectBigger : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Bigger";
    public override string TranslationName { get; set; } = "bigger";
    public override double Probability { get; set; } = 1;
    public Dictionary<string, string> RawParameters {get; set; } = new();
    public override bool ShowDescriptionOnRoll { get; set; } = false;

    public override void Initialize()
    {
        RawParameters.Add("scale", "1.2");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || 
            playerController.PlayerPawn.Value == null ||
            playerController.PlayerPawn.Value.Entity == null
        )  return;

        if(!RawParameters.TryGetValue("scale", out var scale))
            return;

        SetSize(playerController, scale);

        PrintDescription(playerController, TranslationName, scale);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null || 
            playerController.PlayerPawn.Value == null ||
            playerController.PlayerPawn.Value.Entity == null
        )  return;

        SetSize(playerController, "1");
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }

    // Set the skeleton scale as well as SetScale, same as Kandru's ChangePlayerSize
    // https://github.com/Kandru/cs2-roll-the-dice/blob/0863bbaff25201bd73186f4435ac0f2e8737bdca/src/dices/ChangePlayerSize.cs
    private static void SetSize(CCSPlayerController playerController, string scale)
    {
        var pawn = playerController.PlayerPawn.Value;
        if (pawn == null || !float.TryParse(scale, out var scaleFloat))  return;

        var skeleton = pawn.CBodyComponent?.SceneNode?.GetSkeletonInstance();
        if (skeleton != null)
            skeleton.Scale = scaleFloat;

        pawn.AcceptInput("SetScale", null, null, scale);
        CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CBaseEntity", "m_CBodyComponent");
    }
}
