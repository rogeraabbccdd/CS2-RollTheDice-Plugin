
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;

public abstract class EffectBaseRegular : EffectBase, IEffectRegular
{
    public abstract void OnApply(CCSPlayerController? playerController);

    public virtual void PrintDescription(CCSPlayerController? playerController, string effectName, params string[] args)
    {
        playerController!.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(effectName, "description"), args));
    }
}