
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
    public static List<CCSPlayerController> players = [];

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;
        
        if(playerController!.PlayerPawn.Value.LifeState != 0)
            return;

        bool freezeTime = CounterStrikeSharp.API.Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules?.FreezePeriod ?? false;

        if (freezeTime) players.Add(playerController);
        else            playerController.PlayerPawn.Value.CommitSuicide(true, false);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public static void OnRoundFreezeEnd()
    {
        foreach (var player in players)
        {
            if (player.PlayerPawn.Value == null)  continue;
            player.PlayerPawn.Value.CommitSuicide(true, false);
        }
        players.Clear();
    }
}