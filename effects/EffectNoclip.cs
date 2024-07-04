using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectNoclip : EffectBaseRegular, IEffectParameter, IEffectTimer
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Noclip";
    public override string TranslationName { get; set; } = "noclip";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters {get; set;} = new();
    public Dictionary<nint, CounterStrikeSharp.API.Modules.Timers.Timer> Timers { get; set; } = new();

    public override void Initialize()
    {
        RawParameters.Add("durationSeconds", "5");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;
        
        if(Timers.ContainsKey(playerController!.Handle))
        {
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText("effect_already_applied"));
            return;
        }

        if(!RawParameters.TryGetValue("durationSeconds", out var durationStr))
            return;

        if(!float.TryParse(durationStr, out var durationFl))
            return;

        playerController!.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_NOCLIP;
        Schema.SetSchemaValue(playerController!.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 8);
        playerController.RefreshUI();

        var timerRef = Timers;
        StartTimer(ref timerRef, playerController, durationFl);
        PrintDescription(playerController, TranslationName, durationStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if(Timers.TryGetValue(playerController!.Handle, out var timer))
        {
            timer.Kill();
            Timers.Remove(playerController!.Handle);
        }
    }

    public void OnTimerEnd(CCSPlayerController playerController)
    {
        if(playerController == null || playerController.PlayerPawn.Value == null ||
            !playerController!.IsValidPly() || !playerController!.IsAlive()
        )   return;

        playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "end")));
        playerController.PlayerPawn.Value.MoveType = MoveType_t.MOVETYPE_WALK;
        Schema.SetSchemaValue(playerController!.PlayerPawn.Value.Handle, "CBaseEntity", "m_nActualMoveType", 2);
		playerController.RefreshUI();
    }
}