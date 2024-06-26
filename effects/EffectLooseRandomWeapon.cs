using CounterStrikeSharp.API.Core;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectLooseRandomWeapon : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled {get; set; } = true;
    public override string PrettyName {get; set; } = "Loose Random Weapon".__("effect_name_loose_random_weapon");
    public override string Description { get; set; } = "You have been disarmed, loosing a randomly choosen weapon: {mark}{0}".__("effect_description_loose_random_weapon");
    public override double Probability { get; set; }  = 3;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();

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
            .Where(x => x.Value != null && x.Value.DesignerName != "weapon_knife" && !string.IsNullOrEmpty(x.Value.DesignerName))
            .OrderBy(x => Guid.NewGuid());

        if(randomWeapons == null || randomWeapons.Count() == 0)
        {
            playerController.LogChat(GetEffectPrefix() + "No weapon found to remove");
            return;
        }

        var weaponToRemove = randomWeapons.First();

        if(weaponToRemove == null || weaponToRemove.Value == null || string.IsNullOrEmpty(weaponToRemove!.Value.DesignerName))
        {
            playerController.LogChat(GetEffectPrefix() + "No weapon found to remove");
            return;
        }

        var weaponName = weaponToRemove.Value.DesignerName;
        weaponToRemove.Value.Remove();

        PrintDescription(playerController, "effect_description_loose_random_weapon", weaponName);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}