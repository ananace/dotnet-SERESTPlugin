using System.Runtime.Serialization;

namespace SERESTPlugin.APIs.DataTypes
{

[DataContract]
class GPS
{
    [DataMember(Name = "name", IsRequired = true)]
    public string Name { get; set; }
    [DataMember(Name = "description", EmitDefaultValue = false)]
    public string Description { get; set; } = "";
    [DataMember(Name = "coordinates", IsRequired = true)]
    public Coordinate Coordinates { get; set; }
    [DataMember(Name = "visible")]
    public bool Visible { get; set; } = true;
    [DataMember(Name = "color", EmitDefaultValue = false)]
    public Color Color { get; set; }
}

[DataContract]
class GPSList
{
    [DataMember(Name = "gpses")]
    public GPS[] GPSes { get; set; }
}

}