using System;
using System.Runtime.Serialization;

namespace SERESTPlugin.APIs.DataTypes
{

[DataContract]
public class GPS
{
    [DataMember(Name = "name", IsRequired = true)]
    public string Name { get; set; }
    [DataMember(Name = "description", EmitDefaultValue = false)]
    public string Description { get; set; } = "";
    [DataMember(Name = "coordinates", IsRequired = true)]
    public Coordinate Coordinates { get; set; }
    [DataMember(Name = "visible")]
    public bool? Visible { get; set; } = true;
    [DataMember(Name = "color")]
    public Color Color { get; set; }
    [DataMember(Name = "lifespan", EmitDefaultValue = false)]
    public TimeSpan? Lifespan { get; set; }

    public GPS() {}
    public GPS(VRage.Game.ModAPI.IMyGps Gps)
    {
        Name = Gps.Name;
        Description = Gps.Description;
        Coordinates = new Coordinate(Gps.Coords);
        Visible = Gps.ShowOnHud;
        Color = new Color(Gps.GPSColor);
        Lifespan = Gps.DiscardAt;
    }
}

[DataContract]
public class GPSList
{
    [DataMember(Name = "gpses")]
    public GPS[] GPSes { get; set; }
}

}