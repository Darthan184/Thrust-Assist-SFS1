# Thrust-Assist-SFS1

Thrust assistance mod for Spaceflight Simulator 1

![User Interface](/Images/UI_MarkerOff.png)

![User Interface](/Images/UI_MarkerOn.png)


**Assist** Selects the type of assistance
* 'Off' Switch assistance off (manual control)
* 'Surf' Assist with landing on the surface (i.e vertical velocity with respect to the planet)
* 'Mark' Assist with de-orbiting to land at the selected marker (i.e horizontal velocity with respect to the planet). Visible when the marker is enabled.

if 'surface' assistance is selected, taking the orientation of the rocket into account, this mod will set the thrust to attempt to reach 0 m/s vertical velocity at the specified altitude. If the altitude is set to _surface_ (the minimum altitude) it will attempt to land at the specified velocity.

if 'marker' assistance is selected, taking the orientation of the rocket into account, this mod will set the thrust to attempt to reach 0 m/s horizontal velocity at the specified mark.

**Height** The target altitude, only used for 'surface' assistance. N.B. this is from approximately the bottom of the rocket, not the CoM.
* << _set minimum value_
* < _reduce value_
* \> _increase value_
* \>\> _set maximum value_

**Land at** The target landing speed, only used for 'surface' assistance. Is ignored unless the height is set to the minimum value.
(controls as for height

**Throttle** The target throttle to be used. This is used to estimate the velocity needed.

**Marker** Enable the landing marker. When enabled will show the position of the current marker.
* <<< _move marker anti-clockwise to the next landmark or landed rocket_
* << _move marker anti-clockwise 50m_
* < _move marker anti-clockwise 5m_
* \>  _move marker clockwise 5m_
* \>\> _move marker clockwise 50m_
* \>\>\> _move marker clockwise to the next landmark or landed rocket_

![Marker on map](/Images/MapMarker.png)

The blue line is the marker position. The red line is the position where de-orbiting is to start assuming the correct orientation (usually retrograde) and the selected throttle with the current rocket mass and switched on engines.

---

Currently I use it to land more efficiently. It will set the throttle to reduce the vertical velocity so the spacecraft comes to a stop at the specified altitude. This allows me to land faster and so spend less fuel.

If 'surface' assistance is selected and the target altutude is set to the minimum value it will aim to reach the target altitude at the specified vertical velocity i.e land at that velocity. It does not control direction so the spacecraft needs to be pointed up to land. If, when hovering at a specified altitude, you rotate the spacecraft (do not point if down!) it will increase the throttle to maintain altitude as you start to move sideways. While moving sideways it will try to maintain the same altitude above the terrain - following the contours. This is to allow you to pick a landing site. It works best when used with SAS to maintain the desired direction.

To land with it from orbit first enable the marker and select the position you want the land at. Next, timewarp to a little short of the red dotted line on the map and point retrograde. When the red dotted line is reached the engines will start. Once you are descending, switch to 'surface' assistance aiming for the default target altitude of 32m. By pointing retrograde anf remaining sideways velocity is reduced as well. Once at the target altitude, switch 'prograde' off in SAS and manually set the direction to reduce the remaining sideways velocity to zero then set SAS to Surface 0 degress and change the target altitude to the minimum. The spacecraft will land at the specified velocity.

So far I've only tried it with plenty of thrust and coming to a halt over the target, then landing. For more efficient landing or with lower thrust you might want to move the marker a little short of the target and switch to 'surface' assistance before all the horizonal velocity has been eliminated. Might require a little practice to get this right.

At some point I might allow both 'surface' and marker' assistance to be selected at the same time - if I can work out a good way to display the correct direction of thrust needed (and make it work with both selected!).
