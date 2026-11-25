# MECO 8324 Remy (2026)

Local MoSim Rebuilt robot mod using the supplied 8324 FBX and the supplied 2026-Rebuilt shooter calibration tables and drivetrain dimensions.

## Controls (revision 1.6.0)

Remy has its own driver bindings based on `2026-Rebuilt/RobotContainer.java`. Xbox names are shown; other gamepads use the corresponding face-button positions. Existing global Rebuilt binding overrides do not replace these Remy defaults.

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Camera-relative drive (MoSim standard) | WASD | Left stick |
| Turn | J / L | Right stick horizontal |
| Flip camera | Space | Right-stick click |
| Restart match | R | Start / Menu (three lines) |
| Deploy and run intake | Hold Shift | Hold RT |
| Eject | Hold I | Hold LT |
| Deploy intake only | Semicolon | D-pad up |
| Stow intake | Z | D-pad down |
| Aim at hub and calculate shot | Hold K | Hold A / south |
| Feed shooter | Hold E | Hold RB |
| Select hub preset (30 RPS, hood 0°) | Q | X / west |
| Select ferry preset (55 RPS, hood 17.64°) | V | Y / north |
| Stop shooter | O | B / east |
| Pause | Escape | Select / View |

For an aimed shot, hold **A + RB** (keyboard **K + E**). For a preset shot, press **X** or **Y**, then hold **RB**. Aiming and feeding are separate, as in the real driver code. Releasing A stops the calculated shot. The intake remains deployed until stowed or feeding starts. Aimed feeding waits for heading and hood alignment; preset feeding does not auto-aim. Driving uses MoSim’s normal camera orientation, camera flip, and sensitivity settings.

This maps the primary driver controller. The separate copilot-only bindings from the Java project and climbing are not implemented.

## Revision 1.6.0

- Corrected starting-position line detection: the selection model now has a collider tagged `Bumpers`, as required by MoSim's starting-line trigger.
- Restored standard Menu/Start restart binding; right-stick click remains camera flip.
- Raised manual turn range from 401 to 720 degrees/second at 100% turn speed. The MoSim slider scales this range; automatic shot alignment keeps its existing limits.
- Increased wheel suspension travel from 20 to 60 mm with the same resting ride height. Rounded, low-friction bumper contacts with the field reduce snagging; original contacts with fuel and other robots remain.
- Crest recovery testing kept wheel support after settling and drove off without the prior hesitation. Full-stick turn testing reached approximately 720 degrees/second.

## Revision 1.5.0

- Continuous RT intake releases the compliant holding floor so approaching fuel reaches the rollers without releasing/repressing the trigger. The guide closes when intake stops and still releases for reverse or retracted feeding.
- Added physical rack travel limits for the intended 0.30 m stroke. A loaded turning test with 24 extra fuel retained those pieces and kept the deployed rack within a fraction of a millimeter of its endpoint.
- Continuous intake collected 16 of 16 approaching fuel in the test course, compared with 4 of 16 in revision 1.4.0. The regression shooting test fed and scored all eight preloads.
- Starting-position selection now uses a dynamic body and a ground-contact collider, matching MoSim's movement/placement workflow.

## Revision 1.4.0

- Added overlapping fuel-contact walls along the extending hopper, and a sloped sliding floor. Original CAD panel collisions with the field remain intact.
- Added an intake holding surface while the rollers are stopped. It releases for intake, feed, or eject; the compliant floor guide releases during deliberate reverse ejection and when retracted onto the conveyor for feeding.
- Extended the fuel-only lower-chassis guard below carpet level to deflect hard simulated balls sideways before they wedge underneath. It does not collide with the field or alter wheel height.
- A 24-ball extended-hopper turn/reversal test retained all 24. A two-layer, 50-ball floor pile test kept four-wheel contact while crossing the pile. Pickup, normal eight-ball aimed shooting, and two-way trench passage were checked again.

## Revision 1.3.0

- Removed the spawn-heading drive override. Movement and right-stick turning use MoSim's normal controller path; robot-code shooter/intake buttons remain unchanged.
- Added a smooth, fuel-only lower-chassis collision volume to keep loose fuel out of the belly-pan and wheel gaps. It does not increase field-obstacle size or trench height.
- Corrected clear-panel blending and opacity, reduced pale diffuse metal, and increased contrast between brushed metal and dark parts. Field lighting is unchanged.
- Checked a 25-ball pile from multiple directions, low-speed reverse travel, camera-relative input while turning, intake, eight-ball aimed scoring, and trench clearance.

## Revision 1.2.0

- Replaced the side/rear net with a top retaining net and a sliding front section that follows the hopper. Fuel collides with the cover; the flexible net does not block the field.
- Moved preloads clear of feed rollers, stabilized roller contact damping, and corrected indexer/shooter core clearance.
- Added a stopped-indexer holding surface, reduced transfer speeds, and retract the rack during feeding.
- Strengthened the hood servo. After physical roller release, apply the supplied robot simulator's wheel-speed/efficiency calibration and 16° launch-zero offset once; actual hood angle and chassis velocity still affect the shot. Fuel is not teleported or steered toward the hub.
- Aimed feeding waits for heading within 2° and hood within 1° for 0.15 seconds.

## Retained from revision 1.1.0

- Corrected the radians-to-degrees turn limit: 7 rad/s, approximately 401°/s.
- Removed the inherited drivetrain height offset; all four wheel colliders now support the chassis. Added 20 mm suspension travel and reduced robot contact margins.
- Replaced the hood backer convex hull with contact segments following its CAD bend; enabled continuous speculative collision on the robot bodies.
- Restored CAD RGB values and retained the clear panels' transparency. Net strands share one mesh.

The prefab uses physical rack and hood joints, InteractionRollers, alliance-colored bumpers, and eight fuel preloads. Rack travel is 0.30 m; drive speed is 5.24 m/s with a 5.890909 gear ratio and four-inch wheels. Shooter speed and hood angle interpolate the supplied Java calibration table. This remains a simulation approximation of the real shooter; scoring depends on distance and motion.

Earlier revision validation used a virtual gamepad through Unity's Input System: eight of eight tracked fuel pieces scored at each of three positions, simultaneous aim/feed waited for alignment and scored eight, eight stayed contained during a two-way trench pass, and four floor fuel pieces were collected with eight preloads already onboard. Physical controller hardware still needs your test in the installed game.

## Build and install locally

Select `MECO 8324 Remy.asset`, then use Tools > Modding > Build Selected Modpack (Assets + Code). Use macOS as the active build target for this machine. Output is in `BuiltMods/rebuilt_meco_8324_remy/standaloneosx`.

Check that `modpack.json` has id `rebuilt_meco_8324_remy_assets` and modDll `MECO8324Remy.dll`, and that the DLL exists next to the bundle. If the asset build reloads metadata and produces blank manifest entries, run the code-only modpack build once after the asset build.

Copy the enclosing `rebuilt_meco_8324_remy` directory into the installed game's persistent-data `Mods` directory. This installation of MoSim uses `~/Library/Application Support/CascadeStudios/MoSim/Mods`. Restart MoSim to rescan local mods. This mod has not been published to Steam Workshop.
