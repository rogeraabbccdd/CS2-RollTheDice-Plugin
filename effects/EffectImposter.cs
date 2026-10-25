using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectImposter : EffectBaseRegular, IEffectParameter, IEffectTimer
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Imposter";
    public override string TranslationName { get; set; } = "imposter";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters {get; set;} = new();
    public Dictionary<nint, CounterStrikeSharp.API.Modules.Timers.Timer> Timers { get; set; } = new();
    private const string DEFAULT_T_MODEL = "agents/models/tm_phoenix/tm_phoenix.vmdl";
    private const string DEFAULT_CT_MODEL = "agents/models/ctm_sas/ctm_sas.vmdl";
    // Players who rolled during freeze time, their timer starts on freeze end
    private Dictionary<nint, float> PendingTimers = new();
    private bool ShouldTeleport = true;
    public static Dictionary<CCSPlayerController, string> PlayerModel = new Dictionary<CCSPlayerController, string>();
    public override void Initialize()
    {
        RawParameters.Add("durationSeconds", "5");
        RawParameters.Add("teleport", "true");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value == null)  return;

        if(Timers.ContainsKey(playerController!.Handle) || PendingTimers.ContainsKey(playerController.Handle))
        {
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText("effect_already_applied"));
            return;
        }

        if(!RawParameters.TryGetValue("durationSeconds", out var durationStr))
            return;

        if(!float.TryParse(durationStr, out var durationFl))
            return;

        if(!RawParameters.TryGetValue("teleport", out var teleportStr))
            return;

        if(!bool.TryParse(teleportStr, out var teleportBool))
            return;
        
        ShouldTeleport = teleportBool;

        string modelToSet = playerController.Team == CsTeam.CounterTerrorist ? DEFAULT_T_MODEL : DEFAULT_CT_MODEL;

        PlayerModel[playerController] = Utilities.Helpers.GetModel(playerController);

        Server.NextFrame(() =>
        {
            playerController.PlayerPawn.Value.SetModel(modelToSet);
        });

        bool freezeTime = Utilities.Helpers.IsFreezeTime();
        if (freezeTime)
        {
            PendingTimers[playerController.Handle] = durationFl;
        }
        else
        {
            if(ShouldTeleport)    Teleport(playerController);
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
        {
            if(ShouldTeleport)    Teleport(playerController);
            StartEffectTimer(playerController, duration);
        }
    }

    public void StartEffectTimer (CCSPlayerController? playerController, float duration)
    {
        var timerRef = Timers;
        StartTimer(ref timerRef, playerController, duration);
    }

    public void OnTimerEnd(CCSPlayerController playerController)
    {
        if(playerController == null)   return;

        if (!PlayerModel.Remove(playerController, out var model))
            return;

        playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "end")));

        Server.NextFrame(() =>
        {
            if (!playerController.IsValid || playerController.PlayerPawn.Value == null || string.IsNullOrEmpty(model))
                return;

            playerController.PlayerPawn.Value.SetModel(model);
        });
    }

    public void Teleport (CCSPlayerController playerController)
    {
        var team = playerController.Team;
        if (team != CsTeam.Terrorist && team != CsTeam.CounterTerrorist)
            return;

        var spawns = CounterStrikeSharp.API.Utilities.FindAllEntitiesByDesignerName<SpawnPoint>(
            team == CsTeam.Terrorist ? "info_player_counterterrorist" : "info_player_terrorist"
        );

        List<Vector> emptySpawn = new();

        foreach (var spawn in spawns)
        {
            if (spawn == null || spawn.AbsOrigin == null || spawn.AbsRotation == null)
                continue;

            bool conflit = false;
            foreach (CCSPlayerController player in CounterStrikeSharp.API.Utilities.GetPlayers())
            {
                if (
                    player.IsAlive() && 
                    player.Pawn.Value != null &&
                    player.Pawn.Value.AbsOrigin != null &&
                    Utilities.Helpers.CalculateDistance(player.Pawn.Value.AbsOrigin, spawn.AbsOrigin) < 60.0
                )
                {
                    conflit = true;
                    break;
                }
            }

            if (!conflit)
            {
                Vector vector = new ();
                vector[0] = spawn.AbsOrigin[0];
                vector[1] = spawn.AbsOrigin[1];
                vector[2] = spawn.AbsOrigin[2];
                emptySpawn.Add(vector);
            }
        }

        if (emptySpawn.Count == 0)
        {
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText("effect_imposter_no_spawn"));
        }
        else
        {
            Random random = new Random();
            int randomInt = random.Next(0, emptySpawn.Count);
            playerController.PlayerPawn.Value?.Teleport(emptySpawn[randomInt], null, null);
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText("effect_imposter_teleported"));
        }
    }
}
