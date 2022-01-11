using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SERESTPlugin.APIs.DataTypes
{

[DataContract]
public class GridInformation
{
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "id")]
    public long ID { get; set; }
    [DataMember(Name = "size")]
    public string Size { get; set; }
    [DataMember(Name = "mass")]
    public float Mass { get; set; }
    [DataMember(Name = "blocks")]
    public float Blocks { get; set; }
    [DataMember(Name = "velocity")]
    public float Velocity { get; set; }
    [DataMember(Name = "position")]
    public Coordinate Position { get; set; }

    public GridInformation() {}
    public GridInformation(Sandbox.Game.Entities.MyCubeGrid grid)
    {
        Name = (grid as VRage.Game.ModAPI.IMyCubeGrid).CustomName;
        ID = grid.EntityId;
        Size = grid.GridSizeEnum.ToString();

        Mass = grid.Mass;
        Blocks = grid.BlocksCount;
        Velocity = grid.Physics.LinearVelocity.Length();

        Position = new Coordinate(grid.PositionComp.GetPosition());
    }
}

[DataContract]
public class BlockInformation
{
    // TODO: Dynamically discover this list?
    // TODO: Mods?
    static readonly IReadOnlyDictionary<string, System.Type> InterfaceMapping = new Dictionary<string, System.Type> {
        { "air_vent", typeof(SpaceEngineers.Game.ModAPI.IMyAirVent) },
        { "assembler", typeof(Sandbox.ModAPI.IMyAssembler) },
        { "attachable_top", typeof(Sandbox.ModAPI.IMyAttachableTopBlock) },
        { "battery", typeof(Sandbox.ModAPI.IMyBatteryBlock) },
        { "beacon", typeof(Sandbox.ModAPI.IMyBeacon) },
        { "button", typeof(SpaceEngineers.Game.ModAPI.IMyButtonPanel) },
        { "camera", typeof(Sandbox.ModAPI.IMyCameraBlock) },
        { "cargo", typeof(Sandbox.ModAPI.IMyCargoContainer) },
        { "cockpit", typeof(Sandbox.ModAPI.IMyCockpit) },
        { "collector", typeof(Sandbox.ModAPI.IMyCollector) },
        { "connector", typeof(Sandbox.ModAPI.IMyShipConnector) },
        { "controller", typeof(Sandbox.ModAPI.IMyShipController) },
        { "conveyor", typeof(SpaceEngineers.Game.ModAPI.IMyLargeConveyorTurretBase) }, // TODO: Implement for all UseConveyorSystem
        { "door", typeof(Sandbox.ModAPI.IMyDoor) },
        { "drill", typeof(Sandbox.ModAPI.IMyShipDrill) },
        { "exhaust", typeof(Sandbox.ModAPI.IMyExhaustBlock) },
        { "functional", typeof(Sandbox.ModAPI.IMyFunctionalBlock) },
        { "gas_generator", typeof(Sandbox.ModAPI.IMyGasGenerator) },
        { "gas_tank", typeof(Sandbox.ModAPI.IMyGasTank) },
        { "gatling", typeof(Sandbox.ModAPI.IMySmallGatlingGun) },
        { "gravity", typeof(SpaceEngineers.Game.ModAPI.IMyGravityGenerator) },
        { "gravity_sphere", typeof(SpaceEngineers.Game.ModAPI.IMyGravityGeneratorSphere) },
        { "grinder", typeof(Sandbox.ModAPI.IMyShipGrinder) },
        { "gun", typeof(Sandbox.ModAPI.IMyUserControllableGun) },
        { "gyro", typeof(Sandbox.ModAPI.IMyGyro) },
        { "jump_drive", typeof(Sandbox.ModAPI.IMyJumpDrive) },
        { "landing_gear", typeof(SpaceEngineers.Game.ModAPI.IMyLandingGear) },
        { "laser_antenna", typeof(Sandbox.ModAPI.IMyLaserAntenna) },
        { "light", typeof(Sandbox.ModAPI.IMyLightingBlock) },
        { "mass", typeof(SpaceEngineers.Game.ModAPI.IMyVirtualMass) },
        { "mechanical_connection", typeof(Sandbox.ModAPI.IMyMechanicalConnectionBlock) },
        { "merge", typeof(SpaceEngineers.Game.ModAPI.IMyShipMergeBlock) },
        { "missile_launcher", typeof(Sandbox.ModAPI.IMySmallMissileLauncher) },
        { "ore_detector", typeof(Sandbox.ModAPI.IMyOreDetector) },
        { "oxygen_farm", typeof(SpaceEngineers.Game.ModAPI.IMyOxygenFarm) },
        { "parachute", typeof(SpaceEngineers.Game.ModAPI.IMyParachute) },
        { "piston", typeof(Sandbox.ModAPI.IMyPistonBase) },
        { "power_producer", typeof(Sandbox.ModAPI.IMyPowerProducer) },
        { "production", typeof(Sandbox.ModAPI.IMyProductionBlock) },
        { "programmable", typeof(Sandbox.ModAPI.IMyProgrammableBlock) },
        { "projector", typeof(Sandbox.ModAPI.IMyProjector) },
        { "radio_antenna", typeof(Sandbox.ModAPI.IMyRadioAntenna) },
        { "reactor", typeof(Sandbox.ModAPI.IMyReactor) },
        { "remote_control", typeof(Sandbox.ModAPI.IMyRemoteControl) },
        { "sensor", typeof(Sandbox.ModAPI.IMySensorBlock) },
        { "sorter", typeof(Sandbox.ModAPI.IMyConveyorSorter) },
        { "sound", typeof(SpaceEngineers.Game.ModAPI.IMySoundBlock) },
        { "space_ball", typeof(SpaceEngineers.Game.ModAPI.IMySpaceBall) },
        { "stator", typeof(Sandbox.ModAPI.IMyMotorStator) },
        { "store", typeof(Sandbox.ModAPI.IMyStoreBlock) },
        { "suspension", typeof(Sandbox.ModAPI.IMyMotorSuspension) },
        { "text", typeof(Sandbox.ModAPI.IMyTextSurface) },
        { "text_panel", typeof(Sandbox.ModAPI.IMyTextPanel) },
        { "thrust", typeof(Sandbox.ModAPI.IMyThrust) },
        { "timer", typeof(SpaceEngineers.Game.ModAPI.IMyTimerBlock) },
        { "tool", typeof(Sandbox.ModAPI.IMyShipToolBase) },
        { "turret", typeof(Sandbox.ModAPI.IMyLargeTurretBase) },
        { "upgradable", typeof(Sandbox.ModAPI.IMyUpgradableBlock) },
        { "upgrade_module", typeof(Sandbox.ModAPI.IMyUpgradeModule) },
        { "warhead", typeof(Sandbox.ModAPI.IMyWarhead) },
        { "welder", typeof(Sandbox.ModAPI.IMyShipWelder) },
        { "wheel", typeof(Sandbox.ModAPI.IMyWheel) }
    };

    [DataMember(Name = "type")]
    public string Type { get; set; }
    [DataMember(Name = "name")]
    public string Name { get; set; }
    [DataMember(Name = "id")]
    public long ID { get; set; }
    [DataMember(Name = "mass")]
    public float Mass { get; set; }
    [DataMember(Name = "functional")]
    public bool Functional { get; set; }
    [DataMember(Name = "working")]
    public bool Working { get; set; }

    [DataMember(Name = "interfaces")]
    public List<string> Interfaces { get; set; } = new List<string>();

    public BlockInformation() {}
    public BlockInformation(Sandbox.ModAPI.IMyTerminalBlock block)
    {
        Type = block.DefinitionDisplayNameText;
        Name = block.CustomName;
        ID = block.EntityId;
        Mass = block.Mass;
        Functional = block.IsFunctional;
        Working = block.IsWorking;

        // Implemented for all applicable blocks (since all accessable blocks are IMyTerminalBlock)
        Interfaces.Add("name");
        Interfaces.Add("data");

        foreach (var mapping in InterfaceMapping)
        {
            if (mapping.Value.IsAssignableFrom(block.GetType()))
                Interfaces.Add(mapping.Key);
        }

        Interfaces.Sort();
    }
}

[DataContract]
public class LightBlock
{
    [DataMember(Name = "color")]
    public Color Color { get; set; }

    [DataMember(Name = "radius")]
    public float? Radius { get; set; }
    [DataMember(Name = "intensity")]
    public float? Intensity { get; set; }
    [DataMember(Name = "falloff")]
    public float? Falloff { get; set; }
    [DataMember(Name = "blink_interval_seconds")]
    public float? BlinkIntervalSeconds { get; set; }
    [DataMember(Name = "blink_length")]
    public float? BlinkLength { get; set; }
    [DataMember(Name = "blink_offset")]
    public float? BlinkOffset { get; set; }

    public LightBlock() {}
    public LightBlock(Sandbox.ModAPI.IMyLightingBlock block)
    {
        Color = new Color(block.Color);
        Radius = block.Radius;
        Intensity = block.Intensity;
        Falloff = block.Falloff;
        BlinkIntervalSeconds = block.BlinkIntervalSeconds;
        BlinkLength = block.BlinkLength;
        BlinkOffset = block.BlinkOffset;
    }
}

[DataContract]
public class ThrustBlock
{
    [DataMember(Name = "direction")]
    public string Direction { get; set; }

    [DataMember(Name = "current_thrust")]
    public float? Thrust { get; set; }
    [DataMember(Name = "max_thrust")]
    public float? MaxThrust { get; set; }
    [DataMember(Name = "max_effective_thrust")]
    public float? MaxEffectiveThrust { get; set; }

    [DataMember(Name = "override")]
    public float? Override { get; set; }
    [DataMember(Name = "override_perc")]
    public float? OverridePercentage { get; set; }

    public ThrustBlock() {}
    public ThrustBlock(Sandbox.ModAPI.IMyThrust block)
    {
        if (block.GridThrustDirection == VRageMath.Vector3I.Forward)
            Direction = "forward";
        else if (block.GridThrustDirection == VRageMath.Vector3I.Right)
            Direction = "right";
        else if (block.GridThrustDirection == VRageMath.Vector3I.Backward)
            Direction = "backward";
        else if (block.GridThrustDirection == VRageMath.Vector3I.Left)
            Direction = "left";
        else if (block.GridThrustDirection == VRageMath.Vector3I.Up)
            Direction = "up";
        else if (block.GridThrustDirection == VRageMath.Vector3I.Down)
            Direction = "down";

        Override = block.ThrustOverride;
        OverridePercentage = block.ThrustOverridePercentage;
        Thrust = block.CurrentThrust;
        MaxThrust = block.MaxThrust;
        MaxEffectiveThrust = block.MaxEffectiveThrust;
    }
}

[DataContract]
public class GyroBlock
{
    [DataMember(Name = "power")]
    public float? Power { get; set; }
    [DataMember(Name = "override")]
    public bool? Override { get; set; }
    [DataMember(Name = "pitch")]
    public float? Pitch { get; set; }
    [DataMember(Name = "yaw")]
    public float? Yaw { get; set; }
    [DataMember(Name = "roll")]
    public float? Roll { get; set; }

    public GyroBlock() {}
    public GyroBlock(Sandbox.ModAPI.IMyGyro block)
    {
        Power = block.GyroPower;
        Override = block.GyroOverride;
        Pitch = block.Pitch;
        Yaw = block.Yaw;
        Roll = block.Roll;
    }
}

[DataContract]
public class ProgrammableBlock
{
    [DataMember(Name = "running")]
    public bool Running { get; set; }
    [DataMember(Name = "compile_errors")]
    public bool HasErrors { get; set; }
    [DataMember(Name = "default_argument", EmitDefaultValue = false)]
    public string DefaultArgument { get; set; }

    public ProgrammableBlock() {}
    public ProgrammableBlock(Sandbox.ModAPI.IMyProgrammableBlock block)
    {
        Running = block.IsRunning;
        HasErrors = block.HasCompileErrors;
        DefaultArgument = block.TerminalRunArgument;
    }
}


}
