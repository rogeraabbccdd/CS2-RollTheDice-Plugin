using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectLooseRandomWeapon : EffectBaseRegular
{
    public override bool Enabled {get; set; } = true;
    public override string PrettyName {get; set; } = "Loose Random Weapon";
    public override string TranslationName { get; set; } = "loose_random_weapon";
    public override double Probability { get; set; } = 3;
    public override bool ShowDescriptionOnRoll { get; set; } = false;

    public override void Initialize()
    {
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value is null)  return;

        var weaponServices = playerController!.PlayerPawn.Value.WeaponServices;

        if(weaponServices == null)
            return;

        var randomWeapons = weaponServices.MyWeapons
            .Where(x => 
                x.IsValid == true &&
                x.Value != null &&
                !x.Value.DesignerName.Contains("knife") &&
                !x.Value.DesignerName.Contains("bayonet") &&
                x.Value.DesignerName != "weapon_c4" &&
                !string.IsNullOrEmpty(x.Value.DesignerName)
            )
            .OrderBy(x => Guid.NewGuid());

        if(randomWeapons == null || randomWeapons.Count() == 0)
        {
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "failed")));
            return;
        }

        var weaponToRemove = randomWeapons.First();

        if(weaponToRemove == null || weaponToRemove.Value == null || string.IsNullOrEmpty(weaponToRemove!.Value.DesignerName))
        {
            playerController.LogChat(GetEffectPrefix() + Log.GetLocalizedText(Log.GetEffectLocale(TranslationName, "failed")));
            return;
        }

        playerController.RemoveItemByDesignerName(weaponToRemove.Value.DesignerName, false);
        playerController.ExecuteClientCommand("slot3");

        var weaponName = weaponToRemove.Value.DesignerName;
        PrintDescription(playerController, TranslationName, weaponName);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }
}