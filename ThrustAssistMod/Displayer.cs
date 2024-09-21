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
;
                        Double2 toPoint = 0.001*Double2.CosSin(ThrustAssistMod.Utility.radiansPerDegree * ThrustAssistMod.UI.Marker, radius*2);

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
                    UnityEngine.Debug.LogErrorFormat("[ThrustAssistMod.Displayer.MapDrawMarker-{0}] {1}", tracePoint ,excp.ToString());
                }
            }
        }

        public static bool VelocityArrowDraw(SFS.World.Location location)
        {
            string tracePoint = "S-01";

            if (ThrustAssistMod.UI.IsActive)
            {
                try
                {
                    if ( ThrustAssistMod.UI.MarkerOn)
                    {
//~                         SFS.World.Location location=SFS.World.PlayerController.main.player.Value.location.Value;
                        SFS.WorldBase.Planet planet= location.planet;
                        double radius=planet.Radius;
                        UnityEngine.Vector2 localPosition = SFS.World.WorldView.ToLocalPosition(location.position);
//~                         UnityEngine.Vector2 localPosition =location.position.ToVector2;

                        ThrustAssistMod.UI.DebugItem= string.Format("Drawing @{0}",localPosition);

                        UnityEngine.Color colour= UnityEngine.Color.red;
//~                         Double2 toPoint = Double2.CosSin(ThrustAssistMod.Utility.radiansPerDegree * ThrustAssistMod.UI.Marker, radius*2);

//~                         UnityEngine.Vector3[] points = new UnityEngine.Vector3[]
//~                             { UnityEngine.Vector3.zero ,toPoint.ToVector3};

                        GLDrawer.DrawLine
                            (
                                localPosition+ new UnityEngine.Vector2(-1f, -1f)*20
                                , localPosition+new UnityEngine.Vector2(1f, 1f)*20
                                , colour
//~                                 , 0.0025f * SFS.World.WorldView.main.viewDistance
                                ,1f
                            );
                        GLDrawer.DrawLine
                            (
                                localPosition+ new UnityEngine.Vector2(-1f, 1f)*20
                                , localPosition+new UnityEngine.Vector2(1f, -1f)*20
                                , colour
//~                                 , 0.0025f * SFS.World.WorldView.main.viewDistance
                                ,1f
                            );
//~                         GLDrawer.DrawCircle(SFS.World.WorldView.ToLocalPosition(location.position), 10,20, colour);
                    }
                    else
                    {
                        ThrustAssistMod.UI.DebugItem="";
                    }
                }
                catch (System.Exception excp)
                {
                    UnityEngine.Debug.LogErrorFormat("[ThrustAssistMod.Displayer.Draw-{0}] {1}", tracePoint ,excp.ToString());
                }
            }
            return true;
        }
    }
}
