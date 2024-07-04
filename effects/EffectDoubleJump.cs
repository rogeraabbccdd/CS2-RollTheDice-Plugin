using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class DoubleJumpUser
{
    public PlayerButtons PrevButtons { get; set; }
    public PlayerFlags PrevFlags { get; set; }
    public int JumpsCount { get; set; }
}

public class EffectDoubleJump : EffectBaseRegular
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Double Jump";
    public override string TranslationName { get; set; } = "double_jump";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public static Dictionary<CCSPlayerController, DoubleJumpUser> DoubleJumpUsers = new Dictionary<CCSPlayerController, DoubleJumpUser>();
    

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        var doubleJumpUser = new DoubleJumpUser();
        DoubleJumpUsers.Add(playerController, doubleJumpUser);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;
        DoubleJumpUsers.Remove(playerController);
    }

    public static void OnGameFrame ()
    {
        foreach (var player in CounterStrikeSharp.API.Utilities.GetPlayers().Where(player => player.IsValid))
        {
            OnDoubleJump(player);
        }
    }
    // https://github.com/fidarit/cs2-DoubleJump
    public static void OnDoubleJump (CCSPlayerController player)
    {
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn == null)
            return;

        if (!DoubleJumpUsers.TryGetValue(player, out var userInfo))
        {
            return;
        }

        var currentFlags = (PlayerFlags)playerPawn.Flags;
        var currentButtons = player.Buttons;

        var wasGrounded = (userInfo.PrevFlags & PlayerFlags.FL_ONGROUND) != 0;
        var isGrounded = (currentFlags & PlayerFlags.FL_ONGROUND) != 0;

        var jumpWasPressed = (userInfo.PrevButtons & PlayerButtons.Jump) != 0;
        var jumpIsPressed = (currentButtons & PlayerButtons.Jump) != 0;

        if (isGrounded)
            userInfo.JumpsCount = 0;
        else if (userInfo.JumpsCount < 1)
            userInfo.JumpsCount = 1;

        if (!jumpWasPressed && jumpIsPressed
            && !wasGrounded && !isGrounded
            && userInfo.JumpsCount < 2)
        {
            userInfo.JumpsCount++;

            if (playerPawn.AbsVelocity.Z < 0)
                playerPawn.AbsVelocity.Z = 250f;
        }

        userInfo.PrevFlags = currentFlags;
        userInfo.PrevButtons = currentButtons;
    }
}