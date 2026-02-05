using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;


namespace FruitRestrictions.src.Patch
{
    [HarmonyPatch(typeof(BlockEntityBerryBush))]
    public class BerryBushPatch
    {

        [HarmonyPrefix]
        [HarmonyPatch("CheckGrow")]
        public static bool CheckGrow_Prefix(BlockEntityBerryBush __instance, float dt, ref double ___lastCheckAtTotalDays)
        {
            ICoreServerAPI? sapi = __instance.Api as ICoreServerAPI;
            if (sapi == null || !sapi.World.IsFullyLoadedChunk(__instance.Pos))
            {
                return false; // Skip this tick
            }

            if (__instance.Block.Attributes == null)
            {
                __instance.UnregisterAllTickListeners();
                return false;
            }
            // This block here is to prevent lag, we only want to check growth every 2 in-game hours - same as the original method
            // __lastCheckAtTotalDays is populated by harmony with the private field from BlockEntityBerryBush
            ___lastCheckAtTotalDays = Math.Min(___lastCheckAtTotalDays, __instance.Api.World.Calendar.TotalDays);

            double timeSinceLastCheck = GameMath.Mod(__instance.Api.World.Calendar.TotalDays - ___lastCheckAtTotalDays,
                __instance.Api.World.Calendar.DaysPerYear);
            float intervalDays = 2f / __instance.Api.World.Calendar.HoursPerDay;

            if (timeSinceLastCheck <= (double)intervalDays)
            {
                return false;
            }

            ___lastCheckAtTotalDays += intervalDays;

            var variants = __instance.Block.Variant;
            string variant = variants["type"];
            __instance.Api.Logger.Notification($"Beginning berry check on variant {variant}");
            JsonObject? restrictions = __instance.Block?.Attributes?[$"latlongrestriction"];
            if (restrictions == null) return true;
            if (variant.Equals("vitisvinifera") && variants["cultivated"].Equals("cultivated"))
            {
                variant = $"vitisvinifera-{variants["color"]}";
            }
            JsonObject? variantRestrictions = restrictions[variant];
            if (variantRestrictions == null) return true;

            int minX = variantRestrictions["minX"].AsInt(int.MinValue);
            int maxX = variantRestrictions["maxX"].AsInt(int.MaxValue);
            int minZ = variantRestrictions["minZ"].AsInt(int.MinValue);
            int maxZ = variantRestrictions["maxZ"].AsInt(int.MaxValue);

            var pos = __instance.Pos.ToLocalPosition(__instance.Api);
            var posX = pos.X;
            var posZ = pos.Z;

            bool withinX = (minX <= posX && posX <= maxX);
            bool withinZ = (minZ <= posZ && posZ <= maxZ);
            if (!withinX || !withinZ)
            {
                Block nextBlock = __instance.Api.World.GetBlock(__instance.Block?.CodeWithVariant("state", "empty"));
                __instance.Api.World.BlockAccessor.ExchangeBlock(nextBlock.Id, __instance.Pos);
                return false;
            }
            return true;
        }
    }
}
