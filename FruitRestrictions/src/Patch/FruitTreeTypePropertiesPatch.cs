using HarmonyLib;
using System.Runtime.CompilerServices;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace FruitRestrictions.src.Patch
{
    [HarmonyPatch(typeof(BlockFruitTreeBranch), "OnLoaded")]
    public class FruitTreeTypePropertiesPatch
    {
        static void Postfix(BlockFruitTreeBranch __instance, ICoreAPI api)
        {
            if (__instance.TypeProps != null)
            {
                foreach (var kvp in __instance.TypeProps)
                {
                    string treeType = kvp.Key;
                    FruitTreeTypeProperties props = kvp.Value;

                    var treeAttributes = __instance.Attributes["fruittreeProperties"][treeType];

                    int minX = treeAttributes["minX"].AsInt(int.MinValue);
                    int maxX = treeAttributes["maxX"].AsInt(int.MaxValue);
                    int minZ = treeAttributes["minZ"].AsInt(int.MinValue);
                    int maxZ = treeAttributes["maxZ"].AsInt(int.MaxValue);

                    // Attach the extended data directly to this instance
                    props.SetExtendedData(minX, maxX, minZ, maxZ);
                }
            }
        }
    }

    // Extension methods and storage for the additional fields
    public static class FruitTreeTypePropertiesExtensions
    {
        // ConditionalWeakTable allows us to attach data to objects without modifying their class
        // It won't prevent garbage collection of the objects
        private static ConditionalWeakTable<FruitTreeTypeProperties, ExtendedData> extendedData
            = new ConditionalWeakTable<FruitTreeTypeProperties, ExtendedData>();

        private class ExtendedData
        {
            public int MinX { get; set; }
            public int MaxX { get; set; }
            public int MinZ { get; set; }
            public int MaxZ { get; set; }
        }

        public static void SetExtendedData(this FruitTreeTypeProperties props, int minX, int maxX, int minZ, int maxZ)
        {
            var data = extendedData.GetOrCreateValue(props);
            data.MinX = minX;
            data.MaxX = maxX;
            data.MinZ = minZ;
            data.MaxZ = maxZ;
        }

        // Convenient property-style accessors
        public static int GetMinX(this FruitTreeTypeProperties props)
        {
            if (extendedData.TryGetValue(props, out var data))
                return data.MinX;
            return 0;
        }

        public static int GetMaxX(this FruitTreeTypeProperties props)
        {
            if (extendedData.TryGetValue(props, out var data))
                return data.MaxX;
            return 0;
        }

        public static int GetMinZ(this FruitTreeTypeProperties props)
        {
            if (extendedData.TryGetValue(props, out var data))
                return data.MinZ;
            return 0;
        }

        public static int GetMaxZ(this FruitTreeTypeProperties props)
        {
            if (extendedData.TryGetValue(props, out var data))
                return data.MaxZ;
            return 0;
        }
    }
}
