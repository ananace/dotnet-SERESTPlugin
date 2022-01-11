using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SERESTPlugin.APIs.DataTypes
{

[DataContract]
public class AirVentBlockInput
{
    [DataMember(Name = "depressurize")]
    public bool? Depressurize { get; set; }
}

[DataContract]
public class AirVentBlock : AirVentBlockInput
{
    [DataMember(Name = "can_pressurize")]
    public bool CanPressurize { get; set; }
    [DataMember(Name = "status")]
    public string Status { get; set; }
    [DataMember(Name = "pressurization_enabled")]
    public bool PressurizationEnabled { get; set; }
    [DataMember(Name = "room_oxygen_level")]
    public float RoomOxygenLevel { get; set; }
    [DataMember(Name = "gas_input_rate")]
    public float GasInput { get; set; }
    [DataMember(Name = "gas_output_rate")]
    public float GasOutput { get; set; }

    public AirVentBlock() {}
    public AirVentBlock(SpaceEngineers.Game.ModAPI.IMyAirVent block)
    {
        CanPressurize = block.CanPressurize;
        Depressurize = block.Depressurize;
        Status = block.Status.ToString().ToLower();
        PressurizationEnabled = block.PressurizationEnabled;
        RoomOxygenLevel = block.GetOxygenLevel();
        GasInput = block.GasInputPerSecond;
        GasOutput = block.GasOutputPerSecond;
    }
}

[DataContract]
public class AssemblerBlockInput
{
    [DataMember(Name = "mode")]
    public string Mode { get; set; }
    [DataMember(Name = "cooperative")]
    public bool? Cooperative { get; set; }
    [DataMember(Name = "repeating")]
    public bool? Repeating { get; set; }
}

[DataContract]
public class AssemblerBlock : AssemblerBlockInput
{
    [DataMember(Name = "progress")]
    public float Progress { get; set; }

    public AssemblerBlock() {}
    public AssemblerBlock(Sandbox.ModAPI.IMyAssembler block)
    {
        Progress = block.CurrentProgress;
        Mode = block.Mode.ToString().ToLower();
        Cooperative = block.CooperativeMode;
        Repeating = block.Repeating;
    }
}

[DataContract]
public class AttachableTopBlock
{
    [DataMember(Name = "attached")]
    public bool Attached { get; set; }
    [DataMember(Name = "base", EmitDefaultValue = false)]
    public BlockInformation Base { get; set; }

    public AttachableTopBlock() {}
    public AttachableTopBlock(Sandbox.ModAPI.IMyAttachableTopBlock block)
    {
        Attached = block.IsAttached;
        if (block.Base != null)
            Base = new BlockInformation(block.Base);
    }
}

[DataContract]
public class BatteryBlockInput
{
    [DataMember(Name = "charge_mode")]
    public string ChargeMode { get; set; }
}

[DataContract]
public class BatteryBlock : BatteryBlockInput
{
    [DataMember(Name = "current_input")]
    public float CurrentInput { get; set; }
    [DataMember(Name = "max_input")]
    public float MaxInput { get; set; }
    [DataMember(Name = "current_stored")]
    public float CurrentStored { get; set; }
    [DataMember(Name = "max_stored")]
    public float MaxStored { get; set; }
    [DataMember(Name = "has_capacity_remaining")]
    public bool CapacityRemaining { get; set; }
    [DataMember(Name = "is_charging")]
    public bool Charging { get; set; }

    public BatteryBlock() {}
    public BatteryBlock(Sandbox.ModAPI.IMyBatteryBlock block)
    {
        ChargeMode = block.ChargeMode.ToString().ToLower();
        CurrentInput = block.CurrentInput;
        MaxInput = block.MaxInput;
        CurrentStored = block.CurrentStoredPower;
        MaxStored = block.MaxStoredPower;
        CapacityRemaining = block.HasCapacityRemaining;
        Charging = block.IsCharging;
    }
}

[DataContract]
public class BeaconBlock
{
    [DataMember(Name = "radius")]
    public float? Radius { get; set; }
    [DataMember(Name = "hud_text")]
    public string HudText { get; set; }

    public BeaconBlock() {}
    public BeaconBlock(Sandbox.ModAPI.IMyBeacon block)
    {
        Radius = block.Radius;
        HudText = block.HudText;
    }
}

[DataContract]
public class ButtonBlockInput
{
    [DataMember(Name = "anyone_can_use")]
    public bool? AnyoneCanUse { get; set; }
}

[DataContract]
public class ButtonBlock : ButtonBlockInput
{
    [DataContract]
    public class ButtonInfo
    {
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }
        [DataMember(Name = "is_assigned")]
        public bool Assigned { get; set; }
    }

    [DataMember(Name = "buttons", EmitDefaultValue = false)]
    public ButtonInfo[] Buttons { get; set; }

    public ButtonBlock() {}
    public ButtonBlock(SpaceEngineers.Game.ModAPI.IMyButtonPanel block)
    {
        AnyoneCanUse = block.AnyoneCanUse;

        if (block is SpaceEngineers.Game.Entities.Blocks.MyButtonPanel fatBlock)
            Buttons = Enumerable.Range(0, fatBlock.BlockDefinition.ButtonCount).Select(i => new ButtonInfo {
                Name = block.GetButtonName(i),
                Assigned = block.IsButtonAssigned(i)
            }).ToArray();
    }
}

[DataContract]
public class CameraBlock
{
    [DataMember(Name = "is_active")]
    public bool Active { get; set; }

    public CameraBlock() {}
    public CameraBlock(Sandbox.ModAPI.IMyCameraBlock block)
    {
        Active = block.IsActive;
    }
}

[DataContract]
public class CargoBlock
{
    [DataContract]
    public class InventoryInfo
    {
        [DataMember(Name = "id")]
        public string ID { get; set; }
        [DataMember(Name = "items")]
        public int Items { get; set; }
        [DataMember(Name = "max_items")]
        public int MaxItems { get; set; }
        [DataMember(Name = "current_volume")]
        public float CurrentVolume { get; set; }
        [DataMember(Name = "max_volume")]
        public float MaxVolume { get; set; }
        [DataMember(Name = "current_mass")]
        public float CurrentMass { get; set; }
        [DataMember(Name = "max_mass")]
        public float MaxMass { get; set; }
    }

    [DataMember(Name = "inventories", EmitDefaultValue = false)]
    public InventoryInfo[] Inventories { get; set; }

    public CargoBlock() {}
    public CargoBlock(Sandbox.Game.Entities.MyCubeBlock block)
    {
        IEnumerable<VRage.Game.Entity.MyInventoryBase> inventories = null;
        if (block.InventoryCount > 1)
            inventories = Enumerable.Range(0, block.InventoryCount).Select(i => block.GetInventoryBase(i));
        else
            inventories = new[] { block.GetInventoryBase() };

        Inventories = inventories.Select(inv => new InventoryInfo {
            ID = inv.InventoryId.String,
            CurrentVolume = (float)inv.CurrentVolume,
            MaxVolume = (float)inv.MaxVolume,
            CurrentMass = (float)inv.CurrentMass,
            MaxMass = (float)inv.MaxMass,
            Items = inv.GetItemsCount(),
            MaxItems = inv.MaxItemCount,
        }).ToArray();
    }
}

[DataContract]
public class CockpitBlock
{
    [DataMember(Name = "oxygen_capacity")]
    public float OxygenCapacity { get; set; }
    [DataMember(Name = "oxygen_ratio")]
    public float OxygenRatio { get; set; }

    public CockpitBlock() {}
    public CockpitBlock(Sandbox.ModAPI.IMyCockpit block)
    {
        OxygenCapacity = block.OxygenCapacity;
        OxygenRatio = block.OxygenFilledRatio;
    }
}

[DataContract]
public class ConnectorBlockInput
{
    [DataMember(Name = "throw_out")]
    public bool? ThrowOut { get; set; }
    [DataMember(Name = "collect_all")]
    public bool? CollectAll { get; set; }
    [DataMember(Name = "pull_strength")]
    public float? PullStrength { get; set; }
    [DataMember(Name = "parking_enabled")]
    public bool? IsParkingEnabled { get; set; }
    [DataMember(Name = "trading")]
    public bool? Trading { get; set; }
    [DataMember(Name = "power_override")]
    public bool? PowerOverride { get; set; }
}

[DataContract]
public class ConnectorBlock : ConnectorBlockInput
{
    [DataMember(Name = "status")]
    public string Status { get; set; }
    [DataMember(Name = "other")]
    public BlockInformation Other { get; set; }

    public ConnectorBlock() {}
    public ConnectorBlock(Sandbox.ModAPI.IMyShipConnector block)
    {
        ThrowOut = block.ThrowOut;
        CollectAll = block.CollectAll;
        PullStrength = block.PullStrength;
        IsParkingEnabled = block.IsParkingEnabled;
        Status = block.Status.ToString().ToLower();

        if (block.OtherConnector != null)
            Other = new BlockInformation(block.OtherConnector);

        if (block is Sandbox.Game.Entities.Cube.MyShipConnector fatBlock)
        {
            Trading = fatBlock.TradingEnabled.Value;
            PowerOverride = fatBlock.IsPowerTransferOverrideEnabled;
        }
    }
}

[DataContract]
public class ControllerBlockInput
{
    [DataMember(Name = "show_horizon_indicator")]
    public bool? ShowHorizonIndicator { get; set; }
    [DataMember(Name = "dampeners")]
    public bool? DampenersOverride { get; set; }
    [DataMember(Name = "handbrake")]
    public bool? Handbrake { get; set; }
    [DataMember(Name = "control_thrusters")]
    public bool? ControlThrusters { get; set; }
    [DataMember(Name = "control_wheels")]
    public bool? ControlWheels { get; set; }
    [DataMember(Name = "main")]
    public bool? MainCockpit { get; set; }
}

[DataContract]
public class ControllerBlock : ControllerBlockInput
{
    [DataMember(Name = "pilot")]
    public PlayerInformation Pilot { get; set; }

    public ControllerBlock() {}
    public ControllerBlock(Sandbox.ModAPI.IMyShipController block)
    {
        Pilot = new PlayerInformation { Name = block.Pilot.Name, ID = (block.Pilot as Sandbox.Game.Entities.Character.MyCharacter).GetPlayerIdentityId() };
        ShowHorizonIndicator = block.ShowHorizonIndicator;
        DampenersOverride = block.DampenersOverride;
        Handbrake = block.HandBrake;
        ControlThrusters = block.ControlThrusters;
        ControlWheels = block.ControlWheels;
        MainCockpit = block.IsMainCockpit;
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
public class ThrustBlockInput
{
    [DataMember(Name = "current_thrust")]
    public float? Thrust { get; set; }
    [DataMember(Name = "max_thrust")]
    public float? MaxThrust { get; set; }
    [DataMember(Name = "override")]
    public float? Override { get; set; }
    [DataMember(Name = "override_perc")]
    public float? OverridePercentage { get; set; }
}

[DataContract]
public class ThrustBlock : ThrustBlockInput
{
    [DataMember(Name = "direction")]
    public string Direction { get; set; }
    [DataMember(Name = "max_effective_thrust")]
    public float MaxEffectiveThrust { get; set; }

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