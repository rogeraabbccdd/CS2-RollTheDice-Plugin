
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
        // Check money before CanRoll, otherwise a roll is used up even if the player can't pay for it
        if(!plyController.IsValidPly() || !CheckTeamAndLifeState(plyController) || !HasEnoughMoney(plyController) || !CanRoll(plyController))
            return;

        var effect = GetEffectByRoll(plyController);
        if(effect == null)
        {
            plyController.LogChat(Log.GetLocalizedText("dice_no_effect_available"), LogType.INFO);
            return;
        }

        plyController.InGameMoneyServices!.Account -= _plugin.Config!.MoneyToRoll;
        plyController.RefreshUI();

        ApplyEffect(plyController, effect);
    }

    private bool HasEnoughMoney (CCSPlayerController plyController)
    {
        if (plyController.InGameMoneyServices is null)  return false;

        int money = plyController.InGameMoneyServices.Account;

        if(money < _plugin.Config!.MoneyToRoll)
        {
            plyController.LogChat(Log.GetLocalizedText("dice_not_enough_money", _plugin.Config.MoneyToRoll), LogType.INFO);

            return false;
        }

        return true;
    }

    private EffectBase? GetEffectByRoll(CCSPlayerController plyController)
    {
        var effectsList = EffectBase.Effects;

        if(effectsList == null)
        {
            Log.PrintServerConsole("No effects found", LogType.ERROR);
            return null;
        }

        var candidates = effectsList
                .Where(e => e.Enabled && e.Probability > 0 && CanBeRolled(plyController, e))
                .ToList();

        double roll = Random.Shared.NextDouble() * candidates.Sum(e => e.Probability);

        foreach(var effect in candidates)
        {
            roll -= effect.Probability;
            if(roll <= 0)
                return effect;
        }

        return candidates.LastOrDefault();
    }

    // Effects that are still active can't be rolled again, unless they are repeatable or will be removed first
    private bool CanBeRolled(CCSPlayerController plyController, EffectBase effect)
    {
        if(_plugin.Config!.RemoveLastEffect || effect.Repeatable)
            return true;

        return !plyController.GetEffects().Contains(effect);
    }

    private EffectBase? GetEffectByNumber(int number)
    {
        var effectsList = EffectBase.Effects;

        if(effectsList == null)
        {
            Log.PrintServerConsole("No effects found", LogType.ERROR);
            return null;
        }

        return effectsList
                .Where(e => e.RollNumber == number)
                .FirstOrDefault();
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

    public void RollAndApplyEffect(CCSPlayerController plyController, int number)
    {
        if(!plyController.IsValidPly())
            return;
        
        EffectBase? effect = number == -1 ? GetEffectByRoll(plyController) : GetEffectByNumber(number);

        if(effect == null)
            return;

        ApplyEffect(plyController, effect);
    }

    private void ApplyEffect(CCSPlayerController plyController, EffectBase effect)
    {
        var effectManager = RollTheDice.Instance!.EffectManager!;

        if(_plugin.Config.RemoveLastEffect)
            effectManager.RemoveOrResetPlyActiveEffects(plyController);

        effectManager.AddActiveEffect(plyController, effect);

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