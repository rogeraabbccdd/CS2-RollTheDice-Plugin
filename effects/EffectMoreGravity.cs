using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectMoreGravity : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "More Gravity";
    public override string TranslationName { get; set; } = "more_gravity";
    public override double Probability { get; set; } = 2;
    public override string? ConflictGroup => "gravity";
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("gravityScaleFactor", "1.5");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        if(!RawParameters.TryGetValue("gravityScaleFactor", out var gravityStr))
            return;

        if(!float.TryParse(gravityStr, out var gravityFloat))
            return;

        // https://github.com/Kandru/cs2-roll-the-dice/blob/0863bbaff25201bd73186f4435ac0f2e8737bdca/src/dices/LowGravity.cs
        playerController!.PlayerPawn.Value.ActualGravityScale = gravityFloat;
        PrintDescription(playerController, TranslationName, ((gravityFloat-1)*100f)+"%");
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        playerController!.PlayerPawn.Value.ActualGravityScale = 1;
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}