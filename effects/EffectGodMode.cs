
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectGodMode : EffectBaseRegular, IEffectParameter, IEffectTimer
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "GodMode";
    public override string TranslationName { get; set; } = "godmode";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    public Dictionary<IntPtr, CounterStrikeSharp.API.Modules.Timers.Timer> Timers { get; set; } = new();
    // Players who rolled during freeze time, their timer starts on freeze end
    private Dictionary<nint, float> PendingTimers = new();
    // Controller handles of players who can't take damage
    public static HashSet<nint> GodModePlayers = new();
    public override void Initialize()
    {
        RawParameters.Add("durationSeconds", "10");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        if(Timers.ContainsKey(playerController!.Handle) || PendingTimers.ContainsKey(playerController.Handle))
        {
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText("effect_already_applied"));
            return;
        }

        if(!RawParameters.TryGetValue("durationSeconds", out var durationStr))
            return;

        if(!float.TryParse(durationStr, out var durationFl))
            return;

        GodModePlayers.Add(playerController.Handle);

        bool freezeTime = Utilities.Helpers.IsFreezeTime();
        if (freezeTime)
        {
            PendingTimers[playerController.Handle] = durationFl;
        }
        else
        {
            StartEffectTimer(playerController, durationFl);
        }

        PrintDescription(playerController, TranslationName, durationStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        bool active = PendingTimers.Remove(playerController.Handle);
        if(Timers.TryGetValue(playerController.Handle, out var timer))
        {
            timer.Kill();
            Timers.Remove(playerController.Handle);
            active = true;
        }

        if (active)
            OnTimerEnd(playerController);
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
        if (playerController != null && PendingTimers.Remove(playerController.Handle, out var duration))
            StartEffectTimer(playerController, duration);
    }
    public void StartEffectTimer (CCSPlayerController? playerController, float duration)
    {
        var timerRef = Timers;
        StartTimer(ref timerRef, playerController, duration);
    }
    public void OnTimerEnd(CCSPlayerController playerController)
    {
        if (playerController == null)  return;

        GodModePlayers.Remove(playerController.Handle);

        if(!playerController.IsValidPly() || !playerController.IsAlive())
            return;

        playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "end")));
    }

    // Returning HookResult.Handled prevents the damage
    public static HookResult OnPlayerTakeDamagePre(CCSPlayerPawn pawn, CTakeDamageInfo info)
    {
        if (GodModePlayers.Count == 0)  return HookResult.Continue;

        var controller = pawn.Controller.Value;
        if (controller == null || !GodModePlayers.Contains(controller.Handle))
            return HookResult.Continue;

        return HookResult.Handled;
    }

}
