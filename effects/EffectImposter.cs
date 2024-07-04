using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectImposter : EffectBaseRegular
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Imposter";
    public override string TranslationName { get; set; } = "imposter";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters {get; set;} = new();
    public Dictionary<nint, CounterStrikeSharp.API.Modules.Timers.Timer> Timers { get; set; } = new();
    private const string DEFAULT_T_MODEL = "characters/models/tm_phoenix/tm_phoenix.vmdl";
    private const string DEFAULT_CT_MODEL = "characters/models/ctm_sas/ctm_sas.vmdl";

    public static Dictionary<CCSPlayerController, string> PlayerModel = new Dictionary<CCSPlayerController, string>();
    public override void Initialize()
    {
        RawParameters.Add("durationSeconds", "30");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value == null)  return;

        if(Timers.ContainsKey(playerController!.Handle))
        {
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText("effect_already_applied"));
            return;
        }

        if(!RawParameters.TryGetValue("durationSeconds", out var durationStr))
            return;

        if(!float.TryParse(durationStr, out var durationFl))
            return;

        string modelToSet = playerController.Team == CounterStrikeSharp.API.Modules.Utils.CsTeam.CounterTerrorist ? DEFAULT_T_MODEL : DEFAULT_CT_MODEL;

        PlayerModel.Add(playerController, Utilities.Helpers.GetModel(playerController.PlayerPawn.Value.Handle));

        CounterStrikeSharp.API.Server.NextFrame(() =>
        {
            playerController.PlayerPawn.Value.SetModel(modelToSet);
        });

        var timerRef = Timers;

        RollTheDice.Instance!.AddTimer(durationFl, () =>
        {
            OnTimerEnd(playerController);
        });

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
        if(playerController == null || playerController.PlayerPawn.Value == null)   return;

        playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "end")));

        CounterStrikeSharp.API.Server.NextFrame(() =>
        {
            if (PlayerModel[playerController] != null)
            {
                playerController.PlayerPawn.Value.SetModel(PlayerModel[playerController]);
                PlayerModel.Remove(playerController);
            }
        });
    }
}