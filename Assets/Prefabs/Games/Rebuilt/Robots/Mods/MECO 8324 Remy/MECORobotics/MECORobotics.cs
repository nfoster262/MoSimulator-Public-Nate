using Games.Rebuilt.Components;
using Games.Rebuilt.Robots;
using MoSimCore.BaseClasses.GameManagement;
using MoSimCore.Enums;
using RobotFramework.Components;
using RobotFramework.Controllers.PidSystems;
using RobotFramework.Enums;
using UnityEngine;

// Remy: rack intake, full-width conveyor, fixed shooter with adjustable hood.
public class MECORobotics : RebuiltRobotBase
{
    [SerializeField] public GenericJoint intakeRack;
    [SerializeField] public GenericJoint hood;
    [SerializeField] public InteractionRoller[] intakeRollers;
    [SerializeField] public InteractionRoller[] conveyorRollers;
    [SerializeField] public InteractionRoller[] indexerRollers;
    [SerializeField] public InteractionRoller[] shooterRollers;
    [SerializeField] public Transform shooterExit;
    [SerializeField] public float intakeTravelMeters = 0.30f;
    private bool deployed;
    private float hoodDegrees;
    private float presetSpeed, presetHood;
    private bool wasAiming;
    private RebuiltAutoAimController aim;
    private ConfigurableJoint hoodServo;
    private BoxCollider indexerHold;
    private Collider intakeHold, intakeFloor, crashBarDeflector, frontFeedGuide;
    private float shotRps;
    private bool feeding;
    private float readyDuration;
    private float rackTargetMeters;
    private bool wasFeedHeld;
    private Rigidbody chassis;
    private WheelCollider[] driveWheels;
    private RobotFramework.Controllers.Drivetrain.DriveController steeringDrive;
    private readonly Collider[] exitContacts = new Collider[64];
    private readonly System.Collections.Generic.Dictionary<Rigidbody,float> released = new System.Collections.Generic.Dictionary<Rigidbody,float>();
    // Measured distance/velocity and hood tables from 2026-Rebuilt ShooterConstants.
    private static readonly float[] Distances = {1.16848f,3.130296f,3.67284f,3.902456f,4.873244f,5.150612f,5.345938f,5.9944f};
    private static readonly float[] Speeds = {30,37.9f,40,41,46,47,47,53.7f};
    private static readonly float[] HoodRotations = {0,.019f,.020f,.020f,.020f,.020f,.020f,.049f};
    protected override void Init()
    {
        aim=GetComponent<RebuiltAutoAimController>();
        chassis=GetComponent<Rigidbody>();driveWheels=GetComponentsInChildren<WheelCollider>();
        steeringDrive=GetComponent<RobotFramework.Controllers.Drivetrain.DriveController>();
        intakeHold=intakeRack.transform.Find("Hopper Fuel Contacts/Intake Holding Surface")?.GetComponent<Collider>();
        intakeFloor=intakeRack.transform.Find("Hopper Fuel Contacts/Sliding floor")?.GetComponent<Collider>();
        frontFeedGuide=intakeRack.transform.Find("Front Feed Guide")?.GetComponent<Collider>();
        crashBarDeflector=intakeRack.transform.Find("Crash Bar Fuel Deflector")?.GetComponent<Collider>();
        // Approximate the stopped indexer's foam grip; open only on feed/eject.
        var hold = new GameObject("Indexer Holding Surface");
        hold.layer=LayerMask.NameToLayer("Robot");hold.transform.SetParent(transform,false);
        hold.transform.localPosition=new Vector3(0,.26f,-.02f);
        indexerHold=hold.AddComponent<BoxCollider>();indexerHold.size=new Vector3(.60f,.15f,.008f);
        indexerHold.contactOffset=.001f;indexerHold.excludeLayers=LayerMask.GetMask("Robot","Field");
        intakeRack.SetPid(new PidConstants(10,0,0.1f,0.5f,0));
        hoodServo=hood.GetComponent<ConfigurableJoint>();
        hoodServo.angularXMotion=ConfigurableJointMotion.Limited;
        hoodServo.lowAngularXLimit=new SoftJointLimit{limit=-18.64f};
        hoodServo.highAngularXLimit=new SoftJointLimit{limit=1};
        // Keep packed rigid fuel from holding the hood outside the aim-ready tolerance.
        hoodServo.angularXDrive=new JointDrive{positionSpring=50000,positionDamper=500,maximumForce=2500};
        Scheduler.AddSubsystems(Control);
        Scheduler.AddProtectedSystem(ApplyMechanisms);
    }
    protected override void LateInit()
    {
        // GenericJoint initializes its bodies without interpolation; keep all robot parts in sync.
        GetComponent<Rigidbody>().interpolation=RigidbodyInterpolation.Interpolate;
        intakeRack.GetComponent<Rigidbody>().interpolation=RigidbodyInterpolation.Interpolate;
        hood.GetComponent<Rigidbody>().interpolation=RigidbodyInterpolation.Interpolate;
        // These are parts of one robot, so the rack/hood must not fight its own frame.
        var all=GetComponentsInChildren<Collider>();
        var list=new System.Collections.Generic.List<Collider>();
        foreach(var c in all)if(c.attachedRigidbody==GetComponent<Rigidbody>() || c.attachedRigidbody==intakeRack.GetComponent<Rigidbody>() || c.attachedRigidbody==hood.GetComponent<Rigidbody>())list.Add(c);
        var colliders=list.ToArray();
        for(int i=0;i<colliders.Length;i++) for(int j=i+1;j<colliders.Length;j++)
            if(colliders[i].attachedRigidbody!=colliders[j].attachedRigidbody)
                Physics.IgnoreCollision(colliders[i],colliders[j]);
    }
    private void Control()
    {
        if(IntakeAction || RobotSpecial2 || DpadUp)deployed=true;
        if(StowAction)deployed=false;
        if(ManualShoot.WasTriggered()){presetSpeed=30;presetHood=0;}
        if(RobotSpecial.WasTriggered()){presetSpeed=55;presetHood=17.64f;}
        if(RobotSpecial3.WasTriggered()){presetSpeed=0;presetHood=0;}
        bool aiming=OuttakeAction;
        if(!aiming)ApplyControllerSteering();
        if(wasAiming && !aiming){presetSpeed=0;presetHood=0;}
        wasAiming=aiming;
        float speed=presetSpeed;hoodDegrees=presetHood;
        // Use MoSim's normal camera-relative drive path, sensitivity and camera flip.
        if(aiming)
        {
            aim.SetDefaultMap(DefaultMap.Hub);
            aim.Aim();
            Vector3 delta=aim.GetCurrentTarget()-shooterExit.position;delta.y=0;
            speed=Lookup(delta.magnitude,Speeds);
            hoodDegrees=Lookup(delta.magnitude,HoodRotations)*360;
        }
        // A selects the calculated shot; RB feeds separately, matching RobotContainer.java.
        bool ready=speed>0 && (!aiming || (Mathf.Abs(aim.GetRotationError())<2f &&
            Mathf.Abs(Mathf.DeltaAngle(hood.transform.localEulerAngles.x,hoodDegrees))<1f));
        bool feedHeld=AutoPass;
        readyDuration=feedHeld && ready?readyDuration+Time.fixedDeltaTime:0;
        // VolleyFeedGate latches after initial readiness until RB is released.
        bool feed=feedHeld && speed>0 && (feeding || readyDuration>=.15f);
        if(feedHeld || wasFeedHeld)deployed=true;
        if(feed && !feeding)rackTargetMeters=Mathf.Clamp(intakeRack.transform.localPosition.z+intakeTravelMeters,0,intakeTravelMeters);
        // ShotIntakeSettings defaults: hold 0.13 m, retract at 0.10 m/s.
        rackTargetMeters=feed?Mathf.MoveTowards(rackTargetMeters,.13f,.10f*Time.fixedDeltaTime):(deployed?intakeTravelMeters:0);
        wasFeedHeld=feedHeld;
        bool eject=RobotSpecial2;
        bool collect=IntakeAction;
        // The compliant floor holds stopped fuel; release it immediately for collection, reverse, or feeding.
        if(intakeFloor)intakeFloor.enabled=!collect && !eject && !feed;
        if(intakeHold)intakeHold.enabled=!collect && !eject && !feed;
        // The thin bar contact sheds perched fuel, but yields as the rack feeds rigid simulated balls.
        if(crashBarDeflector)crashBarDeflector.enabled=!feed && !eject;
        // Guide the last fuel off the upper roller only after the loaded rack retracts.
        if(frontFeedGuide)frontFeedGuide.enabled=feed && !eject && intakeRack.transform.localPosition.z<-.15f;
        feeding=feed;shotRps=speed;indexerHold.enabled=!feed && !eject;
        // agitateAndFeed keeps the intake running steadily, including during spin-up.
        for(int i=0;i<intakeRollers.Length;i++)intakeRollers[i].SetRpm(
            (eject?-650:((collect || feedHeld)?650:0))*(i==0?1:-1));
        Set(conveyorRollers,eject?600:((collect || feed)?-600:0));
        for(int i=0;i<indexerRollers.Length;i++)indexerRollers[i].SetRpm((eject?-1:1)*(feed || eject?((i==0 || i==5)?600:-600):0));
        for(int i=0;i<shooterRollers.Length;i++)shooterRollers[i].SetRpm(speed*60*(i==0?1:-48f/54f));
    }
    private void ApplyControllerSteering()
    {
        // Read only axes assigned to this robot's Rotate action, never another player's pad.
        // The default stick-axis dead zone discards small steering corrections before the action.
        float raw=0;
        foreach(var control in RotateAction.controls)
            if(control.device is UnityEngine.InputSystem.Gamepad && control is UnityEngine.InputSystem.Controls.AxisControl axis)
            {
                float value=axis.ReadUnprocessedValue();
                int binding=UnityEngine.InputSystem.InputActionRebindingExtensions.GetBindingIndexForControl(RotateAction,axis);
                if(binding>=0 && (RotateAction.bindings[binding].effectiveProcessors??"").IndexOf("invert",System.StringComparison.OrdinalIgnoreCase)<0)value=-value;
                if(Mathf.Abs(value)>Mathf.Abs(raw))raw=value;
            }
        float native=RotateAction.ReadValue<float>();
        if(RotateAction.activeControl?.device is UnityEngine.InputSystem.Keyboard && Mathf.Abs(native)>.001f)return;
        // A continuous curve with a 6% neutral zone gives useful fine-stick response.
        float amount=Mathf.Clamp01((Mathf.Abs(raw)-.06f)/.94f);
        float response=Mathf.Sign(raw)*amount*(1.3f-.3f*amount);
        float scale=steeringDrive.GetMaxOmega()*RotationSpeedMultiplier;
        // Add only the correction to MoSim's normal turn command. Translation and auto-aim stay native.
        steeringDrive.TurnAssist((response+native)*scale,false);
    }
    private void ApplyMechanisms()
    {
        bool enabled=BaseGameManager.Instance && BaseGameManager.Instance.RobotState==RobotState.Enabled;
        if(!enabled){if(frontFeedGuide)frontFeedGuide.enabled=false;if(crashBarDeflector)crashBarDeflector.enabled=true;feeding=false;readyDuration=0;wasFeedHeld=false;indexerHold.enabled=true;if(intakeHold)intakeHold.enabled=true;if(intakeFloor)intakeFloor.enabled=true;Set(intakeRollers,0);Set(conveyorRollers,0);Set(indexerRollers,0);Set(shooterRollers,0);return;}
        StabilizeFlatGroundPitch();
        intakeRack.SetLinearTarget(rackTargetMeters/.0254f).withAxis(JointAxis.Z).useAutomaticStartingOffset();
        hoodServo.targetRotation=Quaternion.Inverse(Quaternion.Euler(Mathf.Clamp(hoodDegrees,0,17.64f),0,0));
        CalibrateReleasedFuel();
    }
    private void StabilizeFlatGroundPitch()
    {
        // Anti-dive support only on a level four-wheel contact patch, never over bumps or in flight.
        if(driveWheels.Length!=4)return;
        Vector3 normal=Vector3.zero;float low=float.PositiveInfinity,high=float.NegativeInfinity;
        foreach(var wheel in driveWheels)
        {
            if(!wheel.GetGroundHit(out WheelHit hit) || Vector3.Dot(hit.normal,Vector3.up)<.995f)return;
            normal+=hit.normal;low=Mathf.Min(low,hit.point.y);high=Mathf.Max(high,hit.point.y);
        }
        if(high-low>.015f)return;
        Vector3 right=chassis.rotation*Vector3.right;
        float error=Mathf.Asin(Mathf.Clamp(Vector3.Dot(Vector3.Cross(chassis.rotation*Vector3.up,normal.normalized),right),-1f,1f));
        float damping=Vector3.Dot(chassis.angularVelocity,right);
        chassis.AddTorque(right*Mathf.Clamp(error*6000f-damping*180f,-120f,120f),ForceMode.Force);
    }
    private void CalibrateReleasedFuel()
    {
        if(!feeding || shotRps<=0)return;
        // The source robot's simulator uses the 4-inch wheel, 48:54 counterroller,
        // and 0.95 efficiency to model release. Apply it once after actual roller
        // contact, without teleporting fuel or steering it toward a scoring target.
        int count=Physics.OverlapBoxNonAlloc(transform.TransformPoint(new Vector3(0,.60f,-.18f)),
            new Vector3(.32f,.05f,.20f),exitContacts,transform.rotation,LayerMask.GetMask("GamePiece1"));
        for(int i=0;i<count;i++)
        {
            var ball=exitContacts[i].attachedRigidbody;if(!ball)continue;
            if(released.TryGetValue(ball,out float last) && Time.time-last<1f)continue;
            if(Vector3.Dot(ball.velocity,transform.up)<1f || transform.InverseTransformPoint(ball.position).y<.56f)continue;
            float speed=shotRps*(4f*Mathf.PI*.0254f)*(1f+(48f/54f)*(1.5f/4f))*.5f*.95f;
            Vector3 direction=Quaternion.AngleAxis(16f,hood.transform.right)*hood.transform.up;
            float height=Mathf.Max(0,transform.InverseTransformPoint(ball.position).y-.45f);
            Vector3 launch=direction*speed;
            launch.y=Mathf.Sqrt(Mathf.Max(0,launch.y*launch.y-2f*Physics.gravity.magnitude*height));
            ball.velocity=launch+GetComponent<Rigidbody>().GetPointVelocity(ball.position);
            released[ball]=Time.time;
        }
    }
    private static void Set(InteractionRoller[] rollers,float rpm){foreach(var r in rollers)r.SetRpm(rpm);}
    private static float Lookup(float d,float[] values)
    {
        if(d<=Distances[0])return values[0];
        for(int i=1;i<Distances.Length;i++)if(d<=Distances[i])return Mathf.Lerp(values[i-1],values[i],Mathf.InverseLerp(Distances[i-1],Distances[i],d));
        return values[values.Length-1];
    }
}
