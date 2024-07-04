
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;

public abstract class EffectBaseEvent<TGameEvent> : EffectBase, IEffectGameEvent<TGameEvent> where TGameEvent : GameEvent
{
    public abstract HookResult OnEvent(TGameEvent @event, GameEventInfo eventInfo);

    public virtual void PrintMessageOnEvent(CCSPlayerController? playerController, string effectName, params object[] args)
    {
        playerController!.LogCenterHtml(GetEffectPrefix() + "<br>" + Log.GetLocalizedText(Log.GetEffectLocale(effectName, "event"), args));
    }
}