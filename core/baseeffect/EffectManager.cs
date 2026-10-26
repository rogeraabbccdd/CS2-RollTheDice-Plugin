using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Effects;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;

public class EffectManager
{
    private RollTheDice _plugin;
    // A player can have more than one effect when "Remove last effect on roll" is off
    public Dictionary<ulong, List<EffectBase>>? PlyActiveEffect = new();
    public EffectManager(RollTheDice plugin)
    {
        _plugin = plugin;
    }

    public void ResetState()
    {
        PlyActiveEffect?.Clear();
    }

    public void RemoveOrResetPlyActiveEffects(CCSPlayerController plyController)
    {
        if(!plyController.IsValidPly())
            return;

        if(!PlyActiveEffect!.Remove(plyController.SteamID, out var activeEffects))
            return;

        foreach(var activeEffect in activeEffects)
            activeEffect.OnRemove(plyController);
    }

    public void RemoveActiveEffect(CCSPlayerController plyController)
    {
        if(!plyController.IsValidPly())
            return;

        var plyID = plyController.SteamID;

        if(!PlyActiveEffect!.ContainsKey(plyID))
            return;

        PlyActiveEffect.Remove(plyID);
    }

    public void AddActiveEffect(CCSPlayerController plyController, EffectBase effect)
    {
        if(!plyController.IsValidPly())
            return;

        var plyID = plyController.SteamID;

        if(!PlyActiveEffect!.TryGetValue(plyID, out var activeEffects))
        {
            activeEffects = new List<EffectBase>();
            PlyActiveEffect.Add(plyID, activeEffects);
        }

        activeEffects.Add(effect);
    }

    // The new effect replaces active effects of the same conflict group, e.g. More Speed replaces Less Speed
    public void RemoveConflictingEffects(CCSPlayerController plyController, EffectBase effect)
    {
        if(effect.ConflictGroup == null || !PlyActiveEffect!.TryGetValue(plyController.SteamID, out var activeEffects))
            return;

        var conflicting = activeEffects
            .Where(e => e != effect && e.ConflictGroup == effect.ConflictGroup)
            .ToList();

        foreach(var activeEffect in conflicting)
        {
            activeEffect.OnRemove(plyController);
            activeEffects.Remove(activeEffect);
        }
    }

    public IReadOnlyList<EffectBase> GetActiveEffects(CCSPlayerController plyController)
    {
        if(!PlyActiveEffect!.TryGetValue(plyController.SteamID, out var activeEffects))
            return Array.Empty<EffectBase>();

        return activeEffects;
    }

    #region Events

    public HookResult HandlePlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? plyController = @event.Userid;
        if (plyController == null)  return HookResult.Continue;

        RemoveOrResetPlyActiveEffects(plyController);

        return HookResult.Continue;
    }

    public HookResult HandlePlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? plyController = @event.Userid;
        if (plyController == null)  return HookResult.Continue;

        RemoveOrResetPlyActiveEffects(plyController);

        return HookResult.Continue;
    }

    public HookResult HandleRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        CounterStrikeSharp.API.Utilities.GetPlayers().ForEach(plyController => 
        {
            RemoveOrResetPlyActiveEffects(plyController);
        });

        return HookResult.Continue;
    }

    public HookResult HandlePlayerHurt(EventPlayerHurt @event, GameEventInfo eventInfo)
    {
        var victim = @event.Userid;
        var attacker = @event.Attacker;

        foreach(var effect in attacker?.GetEventEffects<EventPlayerHurt>() ?? [])
            effect.OnEvent(@event, eventInfo);

        foreach(var effect in victim?.GetEventEffects<EventPlayerHurt>() ?? [])
            effect.OnEvent(@event, eventInfo);

        EffectMoreSpeed.OnPlayerHurt(victim);

        return HookResult.Continue;
    }

    public HookResult HandleRoundFreezeEnd (EventRoundFreezeEnd @event, GameEventInfo eventInfo)
    {
        foreach (var player in CounterStrikeSharp.API.Utilities.GetPlayers())
        {
            foreach(var effect in player.GetEffects().ToList())
                effect.OnRoundFreezeEnd(player);
        }
        return HookResult.Continue;
    }

    public void OnGameFrame ()
    {
        EffectThirdPerson.OnGameFrame();
        EffectDoubleJump.OnGameFrame();
    }

    public HookResult OnPlayerTakeDamagePre(CCSPlayerPawn pawn, CTakeDamageInfo info)
    {
        return EffectGodMode.OnPlayerTakeDamagePre(pawn, info);
    }
    #endregion
}