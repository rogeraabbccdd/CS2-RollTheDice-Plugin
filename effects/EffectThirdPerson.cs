using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectThirdPerson : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Third Person";
    public override string TranslationName { get; set; } = "thirdperson";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = true;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    
    public override void Initialize()
    {
        RawParameters.Add("smoothCamera", "false");
    }

    // point_camera has no model, so it won't render as an ERROR model behind the player
    // https://github.com/KKNecmi/ThirdPerson-Revamped/blob/master/ThirdPerson/ThirdPersonRevamped.cs
    public static Dictionary<CCSPlayerController, CPointCamera> ThirdPersonPool = new Dictionary<CCSPlayerController, CPointCamera>();
    public static Dictionary<CCSPlayerController, bool> SmoothCameraPool = new Dictionary<CCSPlayerController, bool>();

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value == null)  return;

        if (ThirdPersonPool.ContainsKey(playerController)) return;

        if(!RawParameters.TryGetValue("smoothCamera", out var smoothCameraStr))
            return;

        if(!bool.TryParse(smoothCameraStr, out var smoothCamera))
            return;

        var _camera = CounterStrikeSharp.API.Utilities.CreateEntityByName<CPointCamera>("point_camera");

        if (_camera == null || !_camera.IsValid) return;

        _camera.DispatchSpawn();
        _camera.Teleport(CalculatePositionInFront(playerController, -110, 90), playerController.PlayerPawn.Value.V_angle, new Vector());

        ThirdPersonPool.Add(playerController, _camera);
        SmoothCameraPool[playerController] = smoothCamera;

        Server.NextFrame(() =>
        {
            if (!_camera.IsValid || !playerController.IsValid || playerController.PlayerPawn.Value == null) return;

            playerController.PlayerPawn.Value.CameraServices!.ViewEntity.Raw = _camera.EntityHandle.Raw;
            CounterStrikeSharp.API.Utilities.SetStateChanged(playerController.PlayerPawn.Value, "CBasePlayerPawn", "m_pCameraServices");
        });
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if (!ThirdPersonPool.TryGetValue(playerController, out var _camera)) return;

        if (playerController.IsValid && playerController.PlayerPawn.Value?.CameraServices != null)
        {
            playerController.PlayerPawn.Value.CameraServices.ViewEntity.Raw = uint.MaxValue;
            RollTheDice.Instance!.AddTimer(0.3f, () =>
            {
                if (playerController.IsValid && playerController.PlayerPawn.Value != null)
                    CounterStrikeSharp.API.Utilities.SetStateChanged(playerController.PlayerPawn.Value, "CBasePlayerPawn", "m_pCameraServices");
            });
        }

        if (_camera.IsValid)
            _camera.Remove();

        ThirdPersonPool.Remove(playerController);
        SmoothCameraPool.Remove(playerController);
    }

    #region Game Events
    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
    
    public static void OnGameFrame ()
    {
        foreach (var player in ThirdPersonPool.Keys)
        {
            if (!player.IsValid || player.PlayerPawn.Value == null || !ThirdPersonPool[player].IsValid) continue;

            if (SmoothCameraPool.TryGetValue(player, out var smooth) && smooth)
                UpdateCameraSmooth(ThirdPersonPool[player], player);
            else
                UpdateCamera(ThirdPersonPool[player], player);
        }
    }
    #endregion

    #region Helper functions
    // https://github.com/UgurhanK/ThirdPerson-WIP/blob/main/ThirdPerson/EntityUtilities.cs#L74
    public static Vector CalculatePositionInFront(CCSPlayerController player, float offSetXY, float offSetZ = 0)
    {
        var pawn = player.PlayerPawn.Value;
        // Extract yaw angle from player's rotation QAngle
        float yawAngle = pawn!.EyeAngles!.Y;

        // Convert yaw angle from degrees to radians
        float yawAngleRadians = (float)(yawAngle * Math.PI / 180.0);

        // Calculate offsets in x and y directions
        float offsetX = offSetXY * (float)Math.Cos(yawAngleRadians);
        float offsetY = offSetXY * (float)Math.Sin(yawAngleRadians);

        // Calculate position in front of the player
        var positionInFront = new Vector
        {
            X = pawn!.AbsOrigin!.X + offsetX,
            Y = pawn!.AbsOrigin!.Y + offsetY,
            Z = pawn!.AbsOrigin!.Z + offSetZ
        };

        return positionInFront;
    }
    public static void UpdateCamera (CPointCamera _camera, CCSPlayerController target)
    {
        _camera.Teleport(CalculatePositionInFront(target, -110, 90), target.PlayerPawn.Value!.V_angle, new Vector());
    }
    public static void UpdateCameraSmooth(CPointCamera _camera, CCSPlayerController target)
    {
        // Move 35% of the remaining distance every tick
        const float lerpFactor = 0.35f;
        Vector current = _camera.AbsOrigin!;
        Vector targetPos = CalculatePositionInFront(target, -110, 90);
        Vector smoothed = new Vector
        {
            X = current.X + (targetPos.X - current.X) * lerpFactor,
            Y = current.Y + (targetPos.Y - current.Y) * lerpFactor,
            Z = current.Z + (targetPos.Z - current.Z) * lerpFactor
        };
        _camera.Teleport(smoothed, target.PlayerPawn.Value!.V_angle, new Vector());
    }

    #endregion
}