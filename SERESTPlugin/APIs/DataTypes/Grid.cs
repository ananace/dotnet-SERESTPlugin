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
        Name = grid.GetFriendlyName();
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

    public BlockInformation() {}
    public BlockInformation(Sandbox.Game.Entities.MyCubeBlock block)
    {
        Type = block.DefinitionDisplayNameText;
        Name = (block as Sandbox.ModAPI.IMyTerminalBlock).CustomName;
        ID = block.EntityId;
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

}