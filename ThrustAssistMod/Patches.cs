// ------------------------
// code from https://github.com/AstroTheRabbit/Aero-Trajectory-SFS/releases v1.4 - AeroTrajectory/Patches.cs by Astro The Rabbit
// ------------------------
using System.Linq; // contains extensions
using HarmonyLib; // contains extensions

namespace ThrustAssistMod
{
    [HarmonyLib.HarmonyPatch(typeof(SFS.World.Maps.MapManager), "DrawTrajectories")]
     class MapManager_DrawTrajectories
    {
        static void Postfix()
        {
            ThrustAssistMod.Displayer.MapDrawMarker();
        }
    }
}