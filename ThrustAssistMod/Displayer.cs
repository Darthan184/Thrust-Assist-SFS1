namespace ThrustAssistMod
{
    public class Displayer
    {
        private const double radiansPerDegree=0.01745329238474369;

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
;
                        Double2 toPoint = 0.001*Double2.CosSin(radiansPerDegree * ThrustAssistMod.UI.Marker, radius*2);

                        UnityEngine.Vector3[] points = new UnityEngine.Vector3[]
                            { UnityEngine.Vector3.zero ,toPoint.ToVector3};

                        SFS.World.Maps.Map.dashedLine.DrawLine
                            (
                                points
                                ,planet
                                ,UnityEngine.Color.red
                                ,UnityEngine.Color.green
                            );
                    }
                }
                catch (System.Exception excp)
                {
                    UnityEngine.Debug.LogErrorFormat("[ThrustAssistMod.Displayer.MapDrawer-{0}] {1}", tracePoint ,excp.ToString());
                }
            }
        }
    }
}
