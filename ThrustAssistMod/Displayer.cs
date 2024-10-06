namespace ThrustAssistMod
{
    public class Displayer
    {
        public static void MapDrawMarker()
        {
            string tracePoint = "S-01";

            if (ThrustAssistMod.UI.IsActive)
            {
                try
                {
                    if ( ThrustAssistMod.UI.MarkerOn)
                    {
                        SFS.WorldBase.Planet planet=SFS.World.PlayerController.main.player.Value.location.Value.planet;
                        double radius=planet.Radius;

                        if (System.Math.Abs(ThrustAssistMod.UI.DeorbitMarker-ThrustAssistMod.UI.Marker)>0.00001)
                        {
                            Double2 toDeorbitPoint = 0.001*Double2.CosSin(ThrustAssistMod.Utility.radiansPerDegree * ThrustAssistMod.UI.DeorbitMarker, radius*2);

                            UnityEngine.Vector3[] deorbitPoints = new UnityEngine.Vector3[]
                                { UnityEngine.Vector3.zero ,toDeorbitPoint.ToVector3};

                            SFS.World.Maps.Map.dashedLine.DrawLine
                                (
                                    deorbitPoints
                                    ,planet
                                    ,UnityEngine.Color.white
                                    ,UnityEngine.Color.red
                                );
                        }

                        Double2 toPoint = 0.001*Double2.CosSin(ThrustAssistMod.Utility.radiansPerDegree * ThrustAssistMod.UI.Marker, radius*2);

                        UnityEngine.Vector3[] points = new UnityEngine.Vector3[]
                            { UnityEngine.Vector3.zero ,toPoint.ToVector3};

                        SFS.World.Maps.Map.dashedLine.DrawLine
                            (
                                points
                                ,planet
                                ,UnityEngine.Color.white
                                ,UnityEngine.Color.blue
                            );
                    }
                }
                catch (System.Exception excp)
                {
                    UnityEngine.Debug.LogErrorFormat("[ThrustAssistMod.Displayer.MapDrawMarker-{0}] {1}", tracePoint ,excp.ToString());
                }
            }
        }
    }
}
