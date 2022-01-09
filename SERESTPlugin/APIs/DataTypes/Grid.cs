using System.Runtime.Serialization;

namespace SERESTPlugin.APIs.DataTypes
{

[DataContract]
internal class GridInformation
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
internal class BlockInformation
{
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

    public BlockInformation() {}
    public BlockInformation(Sandbox.ModAPI.IMyTerminalBlock block)
    {
        Type = block.DefinitionDisplayNameText;
        Name = block.CustomName;
        ID = block.EntityId;
        Mass = block.Mass;
        Functional = block.IsFunctional;
        Working = block.IsWorking;
    }
}

[DataContract]
internal class LightBlock
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
internal class ThrustBlock
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

}