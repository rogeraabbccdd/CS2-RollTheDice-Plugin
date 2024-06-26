
namespace Preach.CS2.Plugins.RollTheDiceV2.Utilities;
public static class Signature
{
    // https://github.com/biggestmannest/CS2StratRoulette/blob/master/CS2StratRoulette/Constants/Signatures.cs
    public const string GETMODELSIGNATURE_WINDOWS =
        @"\x40\x53\x48\x83\xEC\x20\x48\x8B\x41\x30\x48\x8B\xD9\x48\x8B\x48\x08\x48\x8B\x01\x2A\x2A\x2A\x48\x85";

    public const string GETMODELSIGNATURE_LINUX =
        @"\x55\x48\x89\xE5\x53\x48\x89\xFB\x48\x83\xEC\x08\x48\x8B\x47\x38";
}
