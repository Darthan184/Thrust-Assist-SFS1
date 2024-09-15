using System.Linq; // contains extensions
using HarmonyLib; // contains extensions

namespace ThrustAssistMod
{
    [HarmonyLib.HarmonyPatch(typeof(SFS.World.GameManager), "RevertToLaunch")]
     class GameManager_RevertToLaunch
    {
        static void Postfix()
        {
            ThrustAssistMod.UI.AssistOn=false;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(SFS.World.GameManager), "LoadSave")]
     class GameManager_LoadSave
    {
        static void Postfix()
        {
            ThrustAssistMod.UI.AssistOn=false;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(SFS.World.Maps.MapManager), "DrawTrajectories")]
     class MapManager_DrawTrajectories
    {
        static void Postfix()
        {
            ThrustAssistMod.Displayer.MapDrawMarker();
        }
    }
}