using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;

namespace Preach.CS2.Plugins.RollTheDiceV2.Utilities;
public class Commands 
{
    RollTheDice _plugin;
    public Commands(RollTheDice instance)
    {
        _plugin = instance;
        CreateCommands();
    }

    private void ConfigCallback(CCSPlayerController? plyController, string configType, int args = 0, CommandInfo? info = null)
    {
        var isPlyValid = plyController!.IsValidPly();

        if(args != info?.ArgCount)
        {
            string errMsg = Log.GetLocalizedText("effect_config_failed_name");
            if(isPlyValid)
                plyController!.LogChat(errMsg, LogType.ERROR);
            else 
                Log.PrintServerConsole(errMsg, LogType.ERROR);
            
            return;
        }

        var msg = Log.GetLocalizedText("effect_config_changing");
        if(isPlyValid)
            plyController!.LogChat(msg, LogType.INFO);
        else 
            Log.PrintServerConsole(msg, LogType.INFO);

        switch(configType)
        {
            case "all":
                _plugin.ReloadConfig();
                break;
            case "effect":
                _plugin.EffectConfig!.UpdateConfig(info.ArgByIndex(1));
                break;
        }

        var successMessage = Log.GetLocalizedText("effect_config_changed");
        if(isPlyValid)
            plyController!.LogChat(successMessage, LogType.SUCCSS);
        else 
            Log.PrintServerConsole(successMessage, LogType.SUCCSS);
    }

    private void CreateCommands()
    {
        _plugin.AddCommand("dice", Log.GetLocalizedText("cmd_description_dice"), 
            [CommandHelper(0, "Rolling the dice my guy", CommandUsage.CLIENT_ONLY)] (ply, info) => {
                _plugin.DiceSystem!.CheckPlayerStatus(ply!);
            });

        _plugin.AddCommand("rtd_config_reload", Log.GetLocalizedText("cmd_description_reload_config"), 
            [RequiresPermissions("@css/root")] (ply, info) => {
                ConfigCallback(ply, "all", 1, info);
            });

        _plugin.AddCommand("rtd_config_set", Log.GetLocalizedText("cmd_description_effect_config"), 
            [RequiresPermissions("@css/root")] (ply, info) => {
                ConfigCallback(ply, "effect", 2, info);
            });

        _plugin.AddCommand("rtd_timer_effects_end", Log.GetLocalizedText("cmd_description_kill_effect_timers"), 
            [RequiresPermissions("@css/root"), CommandHelper(0, "", CommandUsage.SERVER_ONLY)] (ply, info) => {
                if(info.ArgCount != 3)
                    return;

                var playerID = info.ArgByIndex(1);
                var effectName = info.ArgByIndex(2);

                if(!int.TryParse(playerID, out var playerIDInt))
                    return;
                
                var getEffect = EffectBase.Effects.Where(effect => effect.Name.Contains(effectName)).First();
                var getPly = CounterStrikeSharp.API.Utilities.GetPlayerFromUserid(playerIDInt);

                if(getEffect == null || getPly == null || getEffect is not IEffectTimer effectTimer)
                    return;

                effectTimer.OnTimerEnd(getPly);
                effectTimer.Timers.Remove(getPly.Handle);
            });
    }
}