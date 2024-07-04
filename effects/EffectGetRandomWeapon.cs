using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectGetRandomWeapon : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Get Random Weapon";
    public override string TranslationName { get; set; } = "get_random_weapon";
    public override double Probability { get; set; } = 3;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters {get; set; } = new();

    public override void Initialize()
    {
        RawParameters = new Dictionary<string, string>()
        {
            { "weapon_p250" , "1"  },
            { "weapon_mp9"  , "1"  },
            { "weapon_taser", "1" },
            { "weapon_nova" , "1"  },
            { "weapon_deagle", "1" },
            { "weapon_glock", "1" },
            { "weapon_usp_silencer", "1" },
            { "weapon_hkp2000", "1" },
            { "weapon_fiveseven", "1" },
            { "weapon_tec9", "1" },
            { "weapon_cz75a", "1" },
            { "weapon_revolver", "1" },
            { "weapon_famas", "1" },
            { "weapon_galilar", "1" },
            { "weapon_m4a1", "1" },
            { "weapon_m4a1_silencer", "1" },
            { "weapon_ak47", "1" },
            { "weapon_ssg08", "1" },
            { "weapon_sg556", "1" },
            { "weapon_aug", "1" },
            { "weapon_awp", "1" },
            { "weapon_g3sg1", "1" },
            { "weapon_scar20", "1" },
            { "weapon_mac10", "1" },
            { "weapon_mp7", "1" },
            { "weapon_mp5sd", "1" },
            { "weapon_ump45", "1" },
            { "weapon_p90", "1" },
            { "weapon_bizon", "1" },
            { "weapon_mag7", "1" },
            { "weapon_negev", "1" },
            { "weapon_sawedoff", "1" },
            { "weapon_xm1014", "1" },
            { "weapon_m249", "1" },
            { "weapon_healthshot", "0" },
            { "item_kevlar", "0" },
            { "item_assaultsuit", "0" },
            { "item_defuser", "0" }
        };
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if(RawParameters.Count == 0)
            return;

        if (playerController == null ||
            playerController.PlayerPawn.Value == null ||
            playerController.PlayerPawn.Value.ItemServices == null
        )   return;

        var randomEntry = RawParameters
            .Where(
                entry => 
                    entry.Value == "1" &&
                    (entry.Key != "item_defuser" || (entry.Key == "item_defuser" && playerController.Team == CsTeam.CounterTerrorist))
            )
            .OrderBy(_ => Guid.NewGuid())
            .FirstOrDefault();

        if(randomEntry.Key == null)
            return;

        if (randomEntry.Key == "item_defuser")
        {
            _ = new CCSPlayer_ItemServices(playerController.PlayerPawn.Value.ItemServices.Handle)
            {
                HasDefuser = true
            };
            
            CounterStrikeSharp.API.Utilities.SetStateChanged(playerController.PlayerPawn.Value, "CBasePlayerPawn", "m_pItemServices");
        }
        else    playerController!.GiveNamedItem(randomEntry.Key);
        PrintDescription(playerController, TranslationName, randomEntry.Key);
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
    }
}