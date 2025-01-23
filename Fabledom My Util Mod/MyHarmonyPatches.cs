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

        [HarmonyPatch(typeof(WorldMapButtonPanel))]
        [HarmonyPatch("OnEnable")]
        private static class WorldMapButtonPanelPatch
        {
            [HarmonyPostfix]
            private static void Postfix(WorldMapButtonPanel __instance)
            {
                var debugPositiveField = AccessTools.Field(typeof(WorldMapButtonPanel), "debugPositive");
                var debugNegativeField = AccessTools.Field(typeof(WorldMapButtonPanel), "debugNegative");

                var debugPositive = debugPositiveField?.GetValue(__instance) as GameObject;
                var debugNegative = debugNegativeField?.GetValue(__instance) as GameObject;

                if (debugPositive != null && debugNegative != null)
                {
                    debugPositive.SetActive(Core.AlwaysShowDebugButtons);
                    debugNegative.SetActive(Core.AlwaysShowDebugButtons);
                }
            }
        }

    }
}
