using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using Preach.CS2.Plugins.RollTheDiceV2.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Effects;

namespace Preach.CS2.Plugins.RollTheDiceV2.Utilities;

public static class Extensions
{
    public static bool IsValidPly(this CCSPlayerController plyController)
    {
        return plyController is { IsValid: true, IsBot: false, PlayerPawn: { IsValid: true }};
    }

    public static void LogChat(this CCSPlayerController plyController, string message, LogType? type = LogType.DEFAULT)
    {
        if(!plyController.IsValidPly())
            return;

        Log.PrintChat(plyController, message, type);
    }

    public static void LogCenter(this CCSPlayerController plyController, string message, LogType? type = LogType.DEFAULT)
    {
        if(!plyController.IsValidPly())
            return;

        Log.PrintCenter(plyController, message, type);
    }
    public static void LogCenterHtml(this CCSPlayerController plyController, string message, LogType? type = LogType.DEFAULT)
    {
        if(!plyController.IsValidPly())
            return;

        Log.PrintCenterHtml(plyController, message, type);
    }

    public static void AddEffect(this CCSPlayerController plyController, EffectBase effect)
    {
        if(!plyController.IsValidPly())
            return;

        RollTheDice.Instance!.EffectManager!.AddActiveEffect(plyController, effect);
    }

    public static void RemoveEffect(this CCSPlayerController plyController)
    {
        if(!plyController.IsValidPly())
            return;

        RollTheDice.Instance!.EffectManager!.RemoveActiveEffect(plyController);
    }

    public static EffectBase? GetEffect(this CCSPlayerController plyController)
    {
        if(!plyController.IsValidPly())
            return null;

        return RollTheDice.Instance!.EffectManager!.GetActiveEffect(plyController);
    }

    public static IEffectGameEvent<TEvent>? GetEventEffect<TEvent>(this CCSPlayerController plyController) where TEvent : GameEvent
    {
        if(!plyController.IsValidPly())
            return null;

        if(RollTheDice.Instance!.EffectManager!.GetActiveEffect(plyController) is IEffectGameEvent<TEvent> effect)
            return effect;

        return null;
    }

    public static void GiveWeapon(this CCSPlayerController? plyController, string weaponName = "weapon_knife")
    {
        if(!plyController!.IsValidPly())
            return;

        plyController!.GiveNamedItem(weaponName);
    }

    public static void RefreshUI(this CCSPlayerController plyController)
    {
        if(!plyController.IsValidPly() ||
            plyController is { PlayerPawn.Value: {WeaponServices: null, ItemServices: null} } ||
            plyController.PlayerPawn.Value == null
        )   return;
        
        var setStateChanged = CounterStrikeSharp.API.Utilities.SetStateChanged;
        setStateChanged(plyController, "CCSPlayerController", "m_pInGameMoneyServices");
        setStateChanged(plyController.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
        setStateChanged(plyController.PlayerPawn.Value, "CBaseModelEntity", "m_clrRender");
        setStateChanged(plyController.PlayerPawn.Value, "CBaseEntity", "m_MoveType");
        setStateChanged(plyController, "CBasePlayerController", "m_iDesiredFOV");
        setStateChanged(plyController.PlayerPawn!.Value!, "CBasePlayerPawn", "m_pCameraServices");
    }

    public static bool IsAlive(this CCSPlayerController plyController)
    {
        return plyController is { IsValid: true, PawnIsAlive: true };
    }
}