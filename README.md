# Thrust-Assist-SFS1

Thrust assistance mod for Spaceflight Simulator 1

![User Interface](/Images/UI.png)

* << _set minimum value_
* < _reduce value_
* \> _increase value_
* \>\> _set maximum value_

**Height** The target altitude. N.B. this is from approximately the bottom of the rocket, not the CoM.

**Land at** The target landing speed. Is ignored unless the height is set to the minimum value.

**Throttle** The target throttle to be used. This is used to estimate the vertical velocity needed.

Taking the orientation of the rocket into account this mod will set the thrust to attempt to reach 0 m/s at the specified altitude. If the altitude is set to _surface_ (the mimimum altitude) it will attempt to land at the specified velocity.

---

Currently I use it to land more efficiently. It will set the throttle to reduce the vertical velocity so the spacecraft comes to a stop at the specified altitude. This allows me to land faster and so spend less fuel.

If the target altutude is set to the minimum value it will aim to reach the target altitude at the specified vertical velocity i.e land at that velocity. It does not control direction so the spacecraft needs to be pointed up to land. If, when hovering at a specified altitude, you rotate the spacecraft (do not point if down!) it will increase the throttle to maintain altitude as you start to move sideways. While moving sideways it will try to maintain the same altitude above the terrain - following the contours. This is to allow you to pick a landing site. It works best when used with SAS to maintain the desired direction.

To land with it from orbit, you need to point retrograde with thrust assist off, start to deorbit manually, then, when you are descending, switch thrust assist on aiming for the default target altitude of 32m. By pointing retrograde the sideways velocity is reduced as well. Once at the target altitude, switch 'prograde' off in SAS and manually set the direction to reduce the remaining sideways velocity to zero then set SAS to Surface 0 degress and change the target altitude to the minimum (1m). The spacecraft will land at the specified velocity.

At some point I plan to try to specify a horizontal position to land at, to give a target for the horizontal velocity. The combination of the two should make it possible to display the direction you need to point it to land at the specified position as well as working from orbit - if I can make it work!
