using HarmonyLib;
using Vintagestory.GameContent;


namespace FruitRestrictions.src.Patch
{
    [HarmonyPatch(typeof(FruitTreeGrowingBranchBH))]
    public class FruitTreePatch
    {

        [HarmonyPrefix]
        [HarmonyPatch("TryGrow")]
        public static bool TryGrow_Prefix(FruitTreeGrowingBranchBH __instance)
        {   
            if (__instance == null || __instance.Blockentity == null || __instance.Blockentity is not BlockEntityFruitTreeBranch )
            {
                return true;
            }
            BlockEntityFruitTreeBranch? ownBe = __instance.Blockentity as BlockEntityFruitTreeBranch;
            if (ownBe == null)
            {
                return true;
            }
            FruitTreeRootBH behavior = ownBe.GetBehavior<FruitTreeRootBH>();
            if (behavior == null || !behavior.propsByType.TryGetValue(ownBe.TreeType, out var value) || !ownBe.blockBranch.TypeProps.TryGetValue(ownBe.TreeType, out var treeProps))
            {
                return true;
            }
            int minX = FruitTreeTypePropertiesExtensions.GetMinX(treeProps);
            int maxX = FruitTreeTypePropertiesExtensions.GetMaxX(treeProps);
            int minZ = FruitTreeTypePropertiesExtensions.GetMinZ(treeProps);
            int maxZ = FruitTreeTypePropertiesExtensions.GetMaxZ(treeProps);
            var pos = ownBe.Pos.ToLocalPosition(__instance.Api);
            if (pos.X <= minX || pos.X >= maxX || pos.Z <= minZ || pos.Z >= maxZ)
            {
                __instance.Api.Logger.Notification($"Tree {ownBe.TreeType} is out of allowed bounds, setting to dead state (x: {minX} - {maxX}, z: {minZ} - {maxZ}) - {pos.X}, {pos.Y}, {pos.Z}");
                var prop = behavior.propsByType[ownBe.TreeType];
                prop.workingState = EnumFruitTreeState.Dead;
                prop.lastStateChangeTotalDays = __instance.Api.World.Calendar.TotalDays;
                foreach(var props in behavior.propsByType.Values)
                {
                    props.State = props.workingState;
                }

                ownBe.MarkDirty(true);
            }
            
            return true;
        }
    }
}