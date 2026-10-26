using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectHealthRegen : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Health Regen";
    public override string TranslationName { get; set; } = "health_regen";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    // Controller handle -> time when the player can heal next
    public static Dictionary<nint, float> NextHealTime = new();
    private static float DelaySeconds = 4f;
    private static int HealthPerSecond = 5;
    private static int MaxHealth = 100;

    public override void Initialize()
    {
        RawParameters.Add("delaySeconds", "4");
        RawParameters.Add("healthPerSecond", "5");
        RawParameters.Add("maxHealth", "100");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        if(!RawParameters.TryGetValue("delaySeconds", out var delayStr) || !float.TryParse(delayStr, out var delay))
            return;

        if(!RawParameters.TryGetValue("healthPerSecond", out var healthStr) || !int.TryParse(healthStr, out var health))
            return;

        if(!RawParameters.TryGetValue("maxHealth", out var maxStr) || !int.TryParse(maxStr, out var max))
            return;

        DelaySeconds = delay;
        HealthPerSecond = health;
        MaxHealth = max;
        NextHealTime[playerController.Handle] = Server.CurrentTime;

        PrintDescription(playerController, TranslationName, healthStr, delayStr);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        NextHealTime.Remove(playerController.Handle);
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }

    public static void OnPlayerHurt(CCSPlayerController? playerController)
    {
        if (playerController == null || !NextHealTime.ContainsKey(playerController.Handle))  return;

        NextHealTime[playerController.Handle] = Server.CurrentTime + DelaySeconds;
    }

    public static void OnGameFrame()
    {
        if (NextHealTime.Count == 0)  return;

        var now = Server.CurrentTime;
        foreach (var player in CounterStrikeSharp.API.Utilities.GetPlayers())
        {
            if (!NextHealTime.TryGetValue(player.Handle, out var nextHeal) || now < nextHeal)
                continue;

            NextHealTime[player.Handle] = now + 1f;

            var pawn = player.PlayerPawn.Value;
            if (pawn == null || !player.PawnIsAlive || pawn.Health >= MaxHealth)
                continue;

            pawn.Health = Math.Min(pawn.Health + HealthPerSecond, MaxHealth);
            CounterStrikeSharp.API.Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        }
    }
}
