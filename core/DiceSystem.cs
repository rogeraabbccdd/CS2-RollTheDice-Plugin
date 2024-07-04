
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using CounterStrikeSharp.API.Modules.Utils;

namespace Preach.CS2.Plugins.RollTheDiceV2.Core;
public class DiceSystem 
{
    private RollTheDice _plugin;
    public Dictionary<ulong, int>? PlyRollCounter;

    public DiceSystem(RollTheDice plugin)
    {
        _plugin = plugin;
        
        PlyRollCounter = new Dictionary<ulong, int>();
    }

    public void ResetState()
    {
        PlyRollCounter?.Clear();
    }

    private void RemoveOrResetPlyDiceCounter(CCSPlayerController plyController, bool isRemove)
    {
        if(!plyController.IsValidPly())
            return;

        var handle = plyController.SteamID;

        if(!PlyRollCounter!.ContainsKey(handle))
            return;

        if(isRemove)
            PlyRollCounter.Remove(handle);
        else 
            PlyRollCounter[handle] = _plugin.Config.RollsPerRound;
    }

    public bool CanRoll(CCSPlayerController plyController)
    {
        var plyID = plyController.SteamID;

        if(!PlyRollCounter!.ContainsKey(plyID))
            PlyRollCounter.Add(plyID, _plugin.Config!.RollsPerRound);

        int plyRollAmountLeft = --PlyRollCounter[plyID];

        if(plyRollAmountLeft < 0)
        {
            plyController.LogChat(Log.GetLocalizedText("dice_already_rolled"), LogType.INFO);

            return false;
        }

        if(plyRollAmountLeft > 0 && _plugin.Config!.UnicastRollAmount)
            plyController.LogChat(Log.GetLocalizedText("dice_rolls_left", plyRollAmountLeft));

        return true;
    }

    private bool CheckTeamAndLifeState(CCSPlayerController plyController)
    {

        if(!plyController.PawnIsAlive)
        {
            plyController.LogChat(Log.GetLocalizedText("dice_cant_roll_dead"), LogType.INFO);

            return false;
        }

        bool canCTRoll = _plugin.Config!.CTsCanRoll;
        bool canTRoll = _plugin.Config!.TsCanRoll;

        var teamName = "";
        switch(plyController.TeamNum)
        {
            case 1:
                teamName = "spec";
                break;
            case 2: 
                teamName = "t";
                break;
            case 3: 
                teamName = "ct";
                break;
            default:
                teamName = "unknown";
                break;
        }

        if(!canTRoll && plyController.TeamNum == 2 || !canCTRoll && plyController.TeamNum == 3)
        {
            plyController.LogChat(
                Log.GetLocalizedText("dice_wrong_team" + teamName, Log.GetLocalizedText("team_" + teamName))
            , LogType.INFO);

            return false;
        }

        return true;
    }

    public void CheckPlayerStatus(CCSPlayerController plyController)
    {
        if(!plyController.IsValidPly() || !CheckTeamAndLifeState(plyController) || !CanRoll(plyController) || !HasEnoughMoney(plyController))
            return;

        RollAndApplyEffect(plyController);
    }

    private bool HasEnoughMoney (CCSPlayerController plyController)
    {
        if (plyController.InGameMoneyServices is null)  return false;

        int money = plyController.InGameMoneyServices.Account;
        
        return money >= _plugin.Config!.MoneyToRoll;
    }

    private EffectBase? GetEffectByRoll()
    {
        var effectsList = EffectBase.Effects;

        if(effectsList == null)
        {
            Log.PrintServerConsole("No effects found", LogType.ERROR);
            return null;
        }

        double roll = Random.Shared.NextDouble() * EffectBase.TotalCumulativeProbability;

        return effectsList
                .Where(e => e.Enabled)
                .OrderBy(e => e.CumulativeProbability)
                .FirstOrDefault(e => roll <= e.CumulativeProbability);
    }

    private void BroadOrUnicastRollMessages(CCSPlayerController target, EffectBase effect)
    {
        bool localMessage = _plugin.Config!.UnicastOnRollMessage;
        bool broadcastMessage = _plugin.Config!.BroadcastOnRollMessage;
        bool broadcastMessageTerrorists = _plugin.Config!.BroadcastOnRollMessageTerrorists;
        bool broadcastMessageCounterTerrorists = _plugin.Config!.BroadcastOnRollMessageCounterTerrorists;

        var broadcastRollMessage = Log.GetLocalizedText("dice_rolled_broadcast", target.PlayerName, effect.RollNumber+"", Log.GetLocalizedText(Log.GetEffectLocale(effect.TranslationName, "name")));

        Log.PrintServerConsole(broadcastRollMessage, LogType.INFO);

        if(localMessage)
        {
            target.LogChat(Log.GetLocalizedText("dice_rolled_local", effect.RollNumber+"", Log.GetLocalizedText(Log.GetEffectLocale(effect.TranslationName, "name"))));
        }

        if(broadcastMessage || (broadcastMessageTerrorists && broadcastMessageCounterTerrorists))
        {
            Log.PrintChatAll(broadcastRollMessage, LogType.DEFAULT);
        }
        else if(broadcastMessageTerrorists || broadcastMessageCounterTerrorists)
        {
            CounterStrikeSharp.API.Utilities.GetPlayers().Where(_plyRollCounter => _plyRollCounter.IsValidPly()).ToList().ForEach(_plyController => 
            {
                bool broadcastTerroristPly = _plyController.TeamNum == 2 && broadcastMessageTerrorists;
                bool broadcastCounterTerroristPly = _plyController.TeamNum == 3 && broadcastMessageCounterTerrorists;
                
                if(broadcastTerroristPly || broadcastCounterTerroristPly)
                    _plyController.LogChat(broadcastRollMessage, LogType.DEFAULT);
            });
        }
    }

    public void RollAndApplyEffect(CCSPlayerController plyController)
    {
        if(!plyController.IsValidPly())
            return;

        EffectBase? effect = GetEffectByRoll()!;

        if(effect == null)
            return;

        var plyID = plyController.SteamID;

        if (plyController.InGameMoneyServices is null)  return;

        plyController.InGameMoneyServices.Account -= _plugin.Config!.MoneyToRoll;
        plyController.RefreshUI();

        var plyActiveEffect = RollTheDice.Instance!.EffectManager!.PlyActiveEffect;
        if(plyActiveEffect!.ContainsKey(plyID))
        {
            if(_plugin.Config.RemoveLastEffect)
                plyActiveEffect[plyID]!.OnRemove(plyController);

            plyActiveEffect[plyID] = effect;
        }
        else
            plyActiveEffect.Add(plyID, effect);

        BroadOrUnicastRollMessages(plyController, effect);

        if(effect is EffectBaseRegular regularEffect)
            regularEffect.OnApply(plyController);

        if(effect.ShowDescriptionOnRoll)
            plyController.LogChat(effect.GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(effect.TranslationName, "description")), LogType.DEFAULT);
    }

    #region Hooks
    public HookResult HandlePlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? plyController = @event.Userid;
        if (plyController == null)  return HookResult.Continue;

        RemoveOrResetPlyDiceCounter(plyController, true);

        return HookResult.Continue;
    }

    public HookResult HandlePlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if(!_plugin!.Config!.ResetOnDeath)
            return HookResult.Continue;

        CCSPlayerController? plyController = @event.Userid;
        if (plyController == null) return HookResult.Continue;

        RemoveOrResetPlyDiceCounter(plyController, false);

        return HookResult.Continue;
    }

    public HookResult HandleRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        if(_plugin.Config!.BroadcastPluginCommandInformation)

        foreach (CCSPlayerController player in CounterStrikeSharp.API.Utilities.GetPlayers())
        {
            var team = player.Team;
            if (
                (team == CsTeam.CounterTerrorist && _plugin.Config!.CTsCanRoll) ||
                (team == CsTeam.Terrorist && _plugin.Config!.TsCanRoll)
            ) {
                Log.PrintChat(player, Log.GetLocalizedText("dice_notify_round_start"), LogType.DEFAULT);
            }
        }

        if(!_plugin.Config!.ResetOnRoundStart)
            return HookResult.Continue;

        CounterStrikeSharp.API.Utilities.GetPlayers().ForEach(plyController => 
        {
            RemoveOrResetPlyDiceCounter(plyController, false);
        });


        return HookResult.Continue;
    }

    #endregion

}