using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectHeartbeatSensor : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Heartbeat Sensor";
    public override string TranslationName { get; set; } = "heartbeat_sensor";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    // Controller handle -> time of the next display update
    public static Dictionary<nint, float> NextUpdateTime = new();
    private static Dictionary<nint, string> LastText = new();
    private static Dictionary<nint, float> LastSentTime = new();
    private static float Range = 2000f;
    private const float UPDATE_INTERVAL = 0.25f;
    // Send the same text again after this time, so the message doesn't fade out
    private const float KEEP_ALIVE_INTERVAL = 3f;
    private const float UNITS_PER_METER = 39.37f;

    public override void Initialize()
    {
        RawParameters.Add("range", "2000");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if(!RawParameters.TryGetValue("range", out var rangeStr) || !float.TryParse(rangeStr, out var range))
            return;

        Range = range;
        NextUpdateTime[playerController.Handle] = Server.CurrentTime;
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        NextUpdateTime.Remove(playerController.Handle);
        LastText.Remove(playerController.Handle);
        LastSentTime.Remove(playerController.Handle);
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }

    public static void OnGameFrame()
    {
        if (NextUpdateTime.Count == 0)  return;

        var now = Server.CurrentTime;
        foreach (var player in CounterStrikeSharp.API.Utilities.GetPlayers())
        {
            if (!NextUpdateTime.TryGetValue(player.Handle, out var nextUpdate) || now < nextUpdate)
                continue;

            NextUpdateTime[player.Handle] = now + UPDATE_INTERVAL;

            if (!player.IsValidPly() || !player.PawnIsAlive)
                continue;

            ShowHeartbeat(player, now);
        }
    }

    // Only the distance to the nearest enemy is shown, not the direction
    private static void ShowHeartbeat(CCSPlayerController player, float now)
    {
        var origin = player.PlayerPawn.Value?.AbsOrigin;
        if (origin == null)  return;

        float nearest = float.MaxValue;
        foreach (var enemy in Utilities.Helpers.GetEnemies(player))
        {
            var enemyOrigin = enemy.PlayerPawn.Value?.AbsOrigin;
            if (!enemy.PawnIsAlive || enemyOrigin == null)
                continue;

            nearest = Math.Min(nearest, Utilities.Helpers.CalculateDistance(origin, enemyOrigin));
        }

        string text;
        if (nearest > Range)
        {
            text = Log.GetLocalizedText("effect_none_heartbeat_sensor");
        }
        else
        {
            // The distance is rounded so the text doesn't change every tick
            var meters = (int)Math.Round(nearest / UNITS_PER_METER / 5) * 5;
            text = Log.GetLocalizedText("effect_event_heartbeat_sensor", meters);
        }

        // Every new center message plays the fade in again, so only send it when the text changes
        LastText.TryGetValue(player.Handle, out var last);
        LastSentTime.TryGetValue(player.Handle, out var lastSent);
        if (text == last && now - lastSent < KEEP_ALIVE_INTERVAL)
            return;

        LastText[player.Handle] = text;
        LastSentTime[player.Handle] = now;
        player.PrintToCenter(text);
    }
}
