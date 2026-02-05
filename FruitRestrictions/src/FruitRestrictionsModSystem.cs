using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace FruitRestrictions.src
{
    public class FruitRestrictionsModSystem : ModSystem
    {

        private Harmony harmony;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
            harmony = new Harmony(Mod.Info.ModID);
            harmony.PatchAll();

            //api.RegisterBlockEntityBehaviorClass("HerbPlantRestriction", typeof(BlockEntityBehavior.HerbPlantRestriction));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {

        }

        public override void StartClientSide(ICoreClientAPI api)
        {
        }

        public override void Dispose()
        {
            harmony?.UnpatchAll(Mod.Info.ModID);
            base.Dispose();
        }

    }
}
