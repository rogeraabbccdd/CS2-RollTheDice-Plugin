using System.ComponentModel.Design;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Core;

public class EventSystem
{
    private RollTheDice _plugin;

    public EventSystem(RollTheDice plugin)
    {
        _plugin = plugin;
        Initialize();
    }

    public void Initialize()
    {
        _plugin.RegisterEventHandler<EventPlayerDisconnect>(HandlePlayerDisconnect, HookMode.Pre);
        _plugin.RegisterEventHandler<EventPlayerDeath>(HandlePlayerDeath);
        _plugin.RegisterEventHandler<EventRoundStart>(HandleRoundStart);
        _plugin.RegisterEventHandler<EventRoundFreezeEnd>(HandleRoundFreezeEnd);
        _plugin.RegisterEventHandler<EventPlayerHurt>(HandlePlayerHurt);
        _plugin.RegisterListener<Listeners.OnTick>(OnGameFrame);
    }

    public HookResult HandlePlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo eventInfo)
    {
        _plugin.EffectManager!.HandlePlayerDisconnect(@event, eventInfo);
        _plugin.DiceSystem!.HandlePlayerDisconnect(@event, eventInfo);

        return HookResult.Continue;
    }

    public HookResult HandlePlayerDeath(EventPlayerDeath @event, GameEventInfo eventInfo)
    {
        _plugin.EffectManager!.HandlePlayerDeath(@event, eventInfo);
        _plugin.DiceSystem!.HandlePlayerDeath(@event, eventInfo);

        return HookResult.Continue;
    }

    public HookResult HandleRoundStart(EventRoundStart @event, GameEventInfo eventInfo)
    {
        _plugin.EffectManager!.HandleRoundStart(@event, eventInfo);
        _plugin.DiceSystem!.HandleRoundStart(@event, eventInfo);

        return HookResult.Continue;
    }

    public HookResult HandleRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo eventInfo)
    {
        _plugin.EffectManager!.HandleRoundFreezeEnd(@event, eventInfo);

        return HookResult.Continue;
    }

    public HookResult HandlePlayerHurt(EventPlayerHurt @event, GameEventInfo eventInfo)
    {
        _plugin.EffectManager!.HandlePlayerHurt(@event, eventInfo);

        return HookResult.Continue;
    }

    public void OnGameFrame()
    {
        _plugin.EffectManager!.OnGameFrame();
    }
}