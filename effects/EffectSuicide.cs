
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectSuicide : EffectBaseRegular
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Suicide";
    public override string TranslationName { get; set; } = "suicide";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    private bool SuicideOnFreezeEnd = false;

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;
        
        if(playerController!.PlayerPawn.Value.LifeState != 0)
            return;

        bool freezeTime = Utilities.Helpers.IsFreezeTime();

        if (freezeTime) SuicideOnFreezeEnd = true;
        else            playerController.PlayerPawn.Value.CommitSuicide(true, false);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
        if (SuicideOnFreezeEnd && playerController != null && playerController.PlayerPawn.Value != null)
        {
            playerController.PlayerPawn.Value.CommitSuicide(true, false);
        }
    }
}