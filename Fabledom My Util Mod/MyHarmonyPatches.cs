using HarmonyLib;
using Nielsen;
using System.Reflection;

namespace Fabledom_My_Util_Mod
{
    public static class MyHarmonyPatches
    {
        [HarmonyPatch(typeof(Nielsen.Constructable), "HandlePlacementConfirmed")]
        private class ConstructablePlacementPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(Nielsen.Constructable __instance)
            {
                if (Core.InstantBuildNoMaterialsToggle)
                {
                    __instance.ForceConstructionComplete();
                    return false;
                }
                return true;
            }
        }


    }
}
