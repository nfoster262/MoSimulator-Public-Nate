# MECO 8324 Remy (2026)

Local MoSim Rebuilt robot mod using the supplied 8324 FBX and the supplied 2026-Rebuilt shooter calibration tables and drivetrain dimensions.

## Controls (revision 1.11.0)

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

For an aimed shot, hold **A + RB** (keyboard **K + E**). For a preset shot, press **X** or **Y**, then hold **RB**. Aiming and feeding are separate, as in the real driver code. Releasing A stops the calculated shot. While RB is held, the intake runs continuously. Feeding waits for initial readiness, then stays active until release or shooter stop. The rack retracts toward 0.13 m extension at a commanded 0.10 m/s; releasing RB deploys it again. These are the supplied Java ShotIntakeSettings defaults, not live dashboard preferences. Aimed feeding initially waits for heading and hood alignment; preset feeding does not auto-aim. Driving uses MoSim’s normal camera orientation, camera flip, and sensitivity settings.

This maps the primary driver controller. The separate copilot-only bindings from the Java project and climbing are not implemented.

## Revision 1.11.0

- Sloped the hidden front fuel-contact plate away from the upper-roller pinch. A fuel-only feed guide engages near the retracted feeding position to clear the remaining perched fuel without blocking a full rack during retraction.
- Matched the supplied `agitateAndFeed`, `VolleyFeedGate`, and `ShotIntakeSettings` behavior: steady intake during RB hold, initial readiness followed by continuous feed, 0.10 m/s rack target motion to 0.13 m extension, and redeployment on release. Removed the earlier timed upper-roller reversal. Roller speeds remain MoSim approximations of the voltage-driven real mechanism.
- Packed aimed test: 50 fuel tracked at feed start, all 50 observed launching, zero remaining jammed; 48 of those scored. Packed high-angle test: 49 tracked, 40 launches observed, zero remaining jammed. The intake-mouth count and sampled launch detector do not establish perfect overflow retention. Standard preloads scored 8/8.
- Four fuel placed at the front plate dropped below the perched region during continuous intake. Empty flat-ground driving, reversal, and turning recorded 68 wheel-contact samples with all four wheels grounded and positively loaded; maximum sampled pitch was 0.274 degrees and roll 0.101 degrees. Wheel geometry was left unchanged because this test did not reproduce a single-point balance fault.

## Revision 1.10.0

- Added a thin inward-sloping, fuel-only contact on the hopper side of the intake crash bar. It prevents the nearby roller/wall contact from supporting fuel at bar height and directs it back into the hopper.
- This contact yields during feeding and reverse so the retracting rack cannot pinch rigid simulated fuel against the conveyor.
- The bar's CAD appearance and field/robot collision envelope are unchanged.
- Standard shooting scored 8/8 preloads. Packed aimed and high-hood tests completed with no fuel remaining jammed.
- A four-fuel placement test compared idle and held-RT behavior. The previous setup produced stationary samples at the bar; the revised contact produced none during the powered test interval.

## Revision 1.9.0

- Strengthened the hood position drive against packed-fuel pressure. Previously, a full load held the hood about 3° away from its requested angle and prevented the existing aim-ready check from enabling feed. The alignment check remains in place.
- Increased the main shooter wheel’s compliant contact depth to preserve a usable path for MoSim’s rigid fuel at high hood angles.
- Shortened the upper guide’s rear edge to leave room for fuel exiting at the maximum hood angle.
- Added a second invisible, fuel-only guide over the front indexer roller. It directs fuel into the feeder entrance instead of letting the last row sit on top of the roller beneath the camera.
- Feeding now releases the holding floor immediately, allowing the remaining fuel to reach the rollers while the existing clearing pulse frees the rack.
- Moved the eight preloads clear of the new guide; checked that they do not overlap it at spawn.
- Full-hopper validation now collects 48 floor balls with eight preloads onboard before shooting, rather than treating the earlier 24-ball test as full capacity. The final aimed run launched and scored all 52 balls counted at feed start, with none remaining jammed. The 17.64° ferry run also left no fuel jammed; 51 launches were observed from its 52-ball starting load. Scoring is tracked separately from clearing a jam.

## Revision 1.8.0

- Added an invisible, sloped fuel guide across the pocket between the Limelight mount and flywheel. It directs loose fuel into the shooter channel rather than letting it perch above the flywheel.
- The guide only collides with fuel. It adds no visible printed parts and does not change contacts with the field or other robots.
- In a six-fuel drop test, three pieces remained perched with the previous geometry; all six cleared the pocket with the guide. Close-range shooting scored 8/8 preloads, and loaded feeding scored 24/24 fuel.

## Revision 1.7.0

- Increased the upper intake's effective contact clearance for MoSim's rigid fuel. This prevents the powered roller from pinning approaching fuel against the carpet; the CAD roller visuals retain their original position.
- Held-RT pickup collected 16/16 fuel in each tested slow, medium, and full-stick pass (approximately 0.77, 2.28, and 5.00 m/s peak speeds). The previous contact geometry collected 8/16 at medium speed and 9/16 at full speed. A deployed, unpowered medium-speed comparison collected 0/16.
- During sustained feeding, the upper roller briefly reverses to release fuel perched on top, then resumes normal transfer.
- Loaded turning retained all 24 added fuel within the hopper, with deployed rack drift below 0.3 mm. A loaded feed test scored all 24 fuel after the clearing-pulse change.
- Keep RT held while driving into fuel; releasing and pressing the trigger again is not required.

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
