using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;
using CounterStrikeSharp.API.Modules.Timers;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API.Modules.Utils;
using Color = System.Drawing.Color;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectBeacon : EffectBaseRegular
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Beacon";
    public override string TranslationName { get; set; } = "beacon";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public static Dictionary<CCSPlayerController, Timer> BeaconTimers { get; set; } = [];

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if (!BeaconTimers.TryGetValue(playerController, out Timer? timer) || timer == null)
        {
            Beacon(playerController);

            BeaconTimers.Add(playerController, RollTheDice.Instance!.AddTimer(3.0f, () =>
            {
                Beacon(playerController);
            }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE));
        }
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if (BeaconTimers.TryGetValue(playerController, out Timer? timer) && timer != null)
        {
            timer.Kill();
            BeaconTimers[playerController].Kill();
            BeaconTimers.Remove(playerController);
        }
    }
    // https://github.com/schwarper/cs2-admin/blob/main/src/playerutils/playerutils.cs#L194
    public static void Beacon (CCSPlayerController playerController)
    {
        Vector? absOrigin = playerController.PlayerPawn.Value?.AbsOrigin;

        if (absOrigin == null)
        {
            return;
        }

        float step = (float)(2 * Math.PI) / lines;
        float angle = 0.0f;
        Color teamColor = playerController.TeamNum == 2 ? Color.Red : Color.Blue;

        List<CBeam> beams = [];

        for (int i = 0; i < lines; i++)
        {
            Vector start = CalculateCirclePoint(angle, initialRadius, absOrigin);
            angle += step;
            Vector end = CalculateCirclePoint(angle, initialRadius, absOrigin);

            CBeam? beam = CreateAndDrawBeam(start, end, teamColor, 1.0f, 2.0f);

            if (beam != null)
            {
                beams.Add(beam);
            }
        }

        float elapsed = 0.0f;

        RollTheDice.Instance!.AddTimer(0.1f, () =>
        {
            if (elapsed >= 0.9f)
            {
                return;
            }

            MoveBeams(beams, absOrigin, angle, step, radiusIncrement, elapsed);
            elapsed += 0.1f;
        }, TimerFlags.REPEAT);
    }
    private static Vector CalculateCirclePoint(float angle, float radius, Vector mid)
    {
        return new Vector(
            (float)(mid.X + (radius * Math.Cos(angle))),
            (float)(mid.Y + (radius * Math.Sin(angle))),
            mid.Z + 6.0f
        );
    }
    private static CBeam? CreateAndDrawBeam(Vector start, Vector end, Color color, float life, float width)
    {
        CBeam? beam = CounterStrikeSharp.API.Utilities.CreateEntityByName<CBeam>("beam");

        if (beam != null)
        {
            beam.Render = color;
            beam.Width = width;
            beam.Teleport(start, new QAngle(), new Vector());
            beam.EndPos.X = end.X;
            beam.EndPos.Y = end.Y;
            beam.EndPos.Z = end.Z;
            beam.DispatchSpawn();
            RollTheDice.Instance!.AddTimer(life, () => beam.Remove());
        }

        return beam;
    }

    private static void MoveBeams(List<CBeam> beams, Vector mid, float angle, float step, float radiusIncrement, float elapsed)
    {
        float radius = initialRadius + radiusIncrement * (elapsed / 0.1f);
        foreach (CBeam beam in beams)
        {
            Vector start = CalculateCirclePoint(angle, radius, mid);
            angle += step;
            Vector end = CalculateCirclePoint(angle, radius, mid);
            TeleportLaser(beam, start, end);
        }
    }

    private static void TeleportLaser(CBeam beam, Vector start, Vector end)
    {
        if (beam != null && beam.IsValid)
        {
            beam.Teleport(start, new QAngle(), new Vector());
            beam.EndPos.X = end.X;
            beam.EndPos.Y = end.Y;
            beam.EndPos.Z = end.Z;
            CounterStrikeSharp.API.Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
        }
    }

    private const int lines = 20;
    private const float radiusIncrement = 10.0f;
    private const float initialRadius = 20.0f;
}