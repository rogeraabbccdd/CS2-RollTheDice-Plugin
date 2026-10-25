using System.Drawing;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectInvisible : EffectBaseRegular, IEffectParameter, IEffectTimer
{
    public override bool Enabled {get; set; } = true;
    public override string PrettyName {get; set; } = "Invisible";
    public override string TranslationName {get; set; } = "invisible";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    public Dictionary<nint, CounterStrikeSharp.API.Modules.Timers.Timer> Timers {get; set; } = new();
    private bool StartTimerOnFreezeEnd = false;
    private float TimerDuration = 0.0f;
    public override void Initialize()
    {
        RawParameters.Add("durationSeconds", "10");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.Pawn.Value is null)  return;
        
        if(Timers.ContainsKey(playerController!.Handle))
        {
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText("effect_already_applied"));
            return;
        }

        if(!RawParameters.TryGetValue("durationSeconds", out var durationStr))
            return;

        if(!float.TryParse(durationStr, out var durationFloat))
            return;

        SetAlpha(playerController, 0);
        playerController.Pawn.Value.AcceptInput("DisableShadow");

        bool freezeTime = Utilities.Helpers.IsFreezeTime();
        if (freezeTime)
        {
            StartTimerOnFreezeEnd = true;
            TimerDuration = durationFloat;
        }
        else
        {
            StartEffectTimer(playerController, durationFloat);
        }
        PrintDescription(playerController, TranslationName, durationStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if(Timers.TryGetValue(playerController!.Handle, out var timer))
        {
            OnTimerEnd(playerController);

            timer.Kill();
            Timers.Remove(playerController!.Handle);
        }
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
        if (StartTimerOnFreezeEnd)
        {
            StartEffectTimer(playerController, TimerDuration);
            StartTimerOnFreezeEnd = false;
        }
    }

    public void StartEffectTimer (CCSPlayerController? playerController, float duration)
    {
        var timerRef = Timers;
        StartTimer(ref timerRef, playerController, duration);
    }

    public void OnTimerEnd(CCSPlayerController playerController)
    {
        if (playerController == null || playerController.Pawn.Value is null)  return;
        if(!playerController!.IsValidPly())
            return;

        SetAlpha(playerController, 255);
        playerController.Pawn.Value.AcceptInput("EnableShadow");
        
        playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "end")));
    }

    // Apply to the model and every carried weapon, otherwise the weapons are still visible
    private static void SetAlpha(CCSPlayerController playerController, int alpha)
    {
        var pawn = playerController.Pawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        pawn.Render = Color.FromArgb(alpha, 255, 255, 255);
        CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

        var weapons = pawn.WeaponServices?.MyWeapons;
        if (weapons == null) return;

        foreach (var handle in weapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid) continue;

            weapon.Render = Color.FromArgb(alpha, 255, 255, 255);
            CounterStrikeSharp.API.Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");
        }
    }
}