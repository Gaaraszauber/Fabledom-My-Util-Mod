using HarmonyLib;
using Nielsen;
using System.Reflection;
using UnityEngine;

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

        //Patch ob die Bewohner Anzahl auf 5 zu setzen bei jeden Haus
        /*[HarmonyPatch(typeof(WorldObjectData))]
        [HarmonyPatch(nameof(WorldObjectData.residentCapacity), MethodType.Getter)]
        private class ResidentCapacityPatch
        {
            [HarmonyPostfix]
            private static void Postfix(WorldObjectData __instance, ref int __result)
            {
                if (__instance.isHousing)
                {
                    __result = Mathf.Min(__result, 5);
                }
            
        }*/

    }
}
