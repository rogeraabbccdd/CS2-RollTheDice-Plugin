using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Preach.CS2.Plugins.RollTheDiceV2.Core.BaseEffect;
using Preach.CS2.Plugins.RollTheDiceV2.Utilities;

namespace Preach.CS2.Plugins.RollTheDiceV2.Effects;

public class EffectChickenLeader : EffectBaseRegular, IEffectParameter
{
    public override bool Enabled { get; set; } = true;
    public override string PrettyName { get; set; } = "Chicken Leader";
    public override string TranslationName { get; set; } = "chicken_leader";
    public override double Probability { get; set; } = 1;
    public override bool ShowDescriptionOnRoll { get; set; } = false;
    public Dictionary<string, string> RawParameters { get; set; } = new();
    // Controller handle -> spawned chickens, removed with the effect
    public static Dictionary<nint, List<CChicken>> Chickens = new();

    public override void Initialize()
    {
        RawParameters.Add("amount", "4");
        RawParameters.Add("scale", "1.2");
    }

    public override void OnApply(CCSPlayerController? playerController)
    {
        if (playerController == null || playerController.PlayerPawn.Value?.AbsOrigin == null)  return;

        if(!RawParameters.TryGetValue("amount", out var amountStr) || !int.TryParse(amountStr, out var amount))
            return;

        if(!RawParameters.TryGetValue("scale", out var scaleStr) || !float.TryParse(scaleStr, out var scale))
            return;

        var origin = playerController.PlayerPawn.Value.AbsOrigin;
        var chickens = new List<CChicken>();

        for (int i = 0; i < amount; i++)
        {
            var chicken = CounterStrikeSharp.API.Utilities.CreateEntityByName<CChicken>("chicken");
            if (chicken == null)  continue;

            // Spread the chickens in a circle around the player
            var angle = 2 * Math.PI * i / amount;
            var position = new Vector(
                origin.X + (float)(60 * Math.Cos(angle)),
                origin.Y + (float)(60 * Math.Sin(angle)),
                origin.Z
            );

            chicken.DispatchSpawn();
            chicken.Teleport(position, null, null);

            if (chicken.CBodyComponent?.SceneNode != null)
                chicken.CBodyComponent.SceneNode.Scale = scale;

            // Chickens follow their leader, same as pressing USE on a chicken
            Schema.SetSchemaValue(chicken.Handle, "CChicken", "m_leader", playerController.Pawn.Raw);

            chickens.Add(chicken);
        }

        RemoveChickens(playerController.Handle);
        Chickens[playerController.Handle] = chickens;

        PrintDescription(playerController, TranslationName, chickens.Count.ToString());
    }

    public override void OnRemove(CCSPlayerController? playerController)
    {
        if (playerController == null)  return;

        RemoveChickens(playerController.Handle);
    }

    public override void OnRoundFreezeEnd(CCSPlayerController? playerController)
    {
    }

    private static void RemoveChickens(nint handle)
    {
        if (!Chickens.Remove(handle, out var chickens))  return;

        foreach (var chicken in chickens)
        {
            if (chicken.IsValid)
                chicken.Remove();
        }
    }
}
