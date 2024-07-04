using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using System.Drawing;

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

    public static Dictionary<CCSPlayerController, CDynamicProp> ThirdPersonPool = new Dictionary<CCSPlayerController, CDynamicProp>();
    public static Dictionary<CCSPlayerController, CPhysicsPropMultiplayer> ThirdPersonPoolSmooth = new Dictionary<CCSPlayerController, CPhysicsPropMultiplayer>();

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if(!RawParameters.TryGetValue("smoothCamera", out var smoothCameraStr))
            return;

        if(!bool.TryParse(smoothCameraStr, out var smoothCamera))
            return;
        
        if (smoothCamera)
        {
            var _cameraProp = CounterStrikeSharp.API.Utilities.CreateEntityByName<CPhysicsPropMultiplayer>("prop_physics_multiplayer");

            if (_cameraProp == null || !_cameraProp.IsValid) return;

            _cameraProp.DispatchSpawn();

            _cameraProp.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NEVER;
            _cameraProp.Collision.SolidFlags = 12;
            _cameraProp.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;

            _cameraProp.DispatchSpawn();
            _cameraProp.Render = Color.FromArgb(0, 255, 255, 255);

            playerController.PlayerPawn.Value!.CameraServices!.ViewEntity.Raw = _cameraProp.EntityHandle.Raw;
            CounterStrikeSharp.API.Utilities.SetStateChanged(playerController.PlayerPawn.Value, "CBasePlayerPawn", "m_pCameraServices");

            _cameraProp.Teleport(CalculatePositionInFront(playerController, -110, 90), playerController.PlayerPawn.Value.V_angle, new Vector());

            ThirdPersonPoolSmooth.Add(playerController, _cameraProp);
        }
        else
        {
            CDynamicProp? _cameraProp = CounterStrikeSharp.API.Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

            if (_cameraProp == null) return;

            _cameraProp.DispatchSpawn();
            _cameraProp.Render = Color.FromArgb(0, 255, 255, 255);

            CounterStrikeSharp.API.Utilities.SetStateChanged(_cameraProp, "CBaseModelEntity", "m_clrRender");
    
            _cameraProp.Teleport(CalculatePositionInFront(playerController, -110, 90), playerController.PlayerPawn.Value!.V_angle, new Vector());

            playerController.PlayerPawn!.Value!.CameraServices!.ViewEntity.Raw = _cameraProp.EntityHandle.Raw;

            CounterStrikeSharp.API.Utilities.SetStateChanged(playerController.PlayerPawn!.Value!, "CBasePlayerPawn", "m_pCameraServices");

            ThirdPersonPool.Add(playerController, _cameraProp);

            RollTheDice.Instance!.AddTimer(0.5f, () =>
            {
                _cameraProp.Teleport(CalculatePositionInFront(playerController, -110, 90), playerController.PlayerPawn.Value.V_angle, new Vector());
            });
        }
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        if(!RawParameters.TryGetValue("smoothCamera", out var smoothCameraStr))
            return;

        if(!bool.TryParse(smoothCameraStr, out var smoothCamera))
            return;
        
        if (smoothCamera)
        {
            playerController!.PlayerPawn!.Value!.CameraServices!.ViewEntity.Raw = uint.MaxValue;
            RollTheDice.Instance!.AddTimer(0.3f, () => 
                CounterStrikeSharp.API.Utilities.SetStateChanged(playerController.PlayerPawn!.Value!, "CBasePlayerPawn", "m_pCameraServices")
            );
            if (ThirdPersonPoolSmooth[playerController] != null && ThirdPersonPoolSmooth[playerController].IsValid)
                ThirdPersonPoolSmooth[playerController].Remove();
        }
        else
        {
            playerController!.PlayerPawn!.Value!.CameraServices!.ViewEntity.Raw = uint.MaxValue;
            RollTheDice.Instance!.AddTimer(0.3f, () => 
                CounterStrikeSharp.API.Utilities.SetStateChanged(playerController.PlayerPawn!.Value!, "CBasePlayerPawn", "m_pCameraServices")
            );

            if (ThirdPersonPool[playerController] != null && ThirdPersonPool[playerController].IsValid) 
                ThirdPersonPool[playerController].Remove();
        }

        ThirdPersonPool.Remove(playerController);
        ThirdPersonPoolSmooth.Remove(playerController);
    }

    #region Game Events
    public static void OnGameFrame ()
    {
        foreach (var player in ThirdPersonPool.Keys)
        {
            UpdateCamera(ThirdPersonPool[player], player);
        }

        foreach (var player in ThirdPersonPoolSmooth.Keys)
        {
            UpdateCameraSmooth(ThirdPersonPoolSmooth[player], player);
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
    public static Vector CalculateVelocity(Vector positionA, Vector positionB, float timeDuration)
    {
        // Step 1: Determine direction from A to B
        Vector directionVector = positionB - positionA;

        // Step 2: Calculate distance between A and B
        float distance = directionVector.Length();

        // Step 3: Choose a desired time duration for the movement
        // Ensure that timeDuration is not zero to avoid division by zero
        if (timeDuration == 0)
        {
            timeDuration = 1;
        }

        // Step 4: Calculate velocity magnitude based on distance and time
        float velocityMagnitude = distance / timeDuration;

        // Step 5: Normalize direction vector
        if (distance != 0)
        {
            directionVector /= distance;
        }

        // Step 6: Scale direction vector by velocity magnitude to get velocity vector
        Vector velocityVector = directionVector * velocityMagnitude;

        return velocityVector;
    }
    public static void UpdateCamera (CDynamicProp _cameraProp, CCSPlayerController target)
    {
        _cameraProp.Teleport(CalculatePositionInFront(target, -110, 90), target.PlayerPawn.Value!.V_angle, new Vector());
    }
    public static void UpdateCameraSmooth(CPhysicsPropMultiplayer _cameraProp, CCSPlayerController target)
    {
        Vector velocity = CalculateVelocity(_cameraProp.AbsOrigin!, CalculatePositionInFront(target, -110, 90), 0.1f);
        _cameraProp.Teleport(_cameraProp.AbsOrigin!, target.PlayerPawn.Value!.V_angle, velocity);
    }

    #endregion
}