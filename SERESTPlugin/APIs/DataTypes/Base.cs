using System.Runtime.Serialization;

namespace SERESTPlugin.APIs.DataTypes
{

[DataContract]
class Coordinate
{
    [DataMember(Name = "x")]
    public double X { get; set; }
    [DataMember(Name = "y")]
    public double Y { get; set; }
    [DataMember(Name = "z")]
    public double Z { get; set; }

    public Coordinate() {}
    public Coordinate(VRageMath.Vector3D vec)
    {
        X = vec.X;
        Y = vec.Y;
        Z = vec.Z;
    }
    public Coordinate(VRageMath.Vector3 vec)
    {
        X = vec.X;
        Y = vec.Y;
        Z = vec.Z;
    }

    public VRageMath.Vector3D ToVector3D()
    {
        return new VRageMath.Vector3D(X, Y, Z);
    }
}

[DataContract]
internal class Color
{
    [DataMember(Name = "r")]
    public byte R { get; set; }
    [DataMember(Name = "g")]
    public byte G { get; set; }
    [DataMember(Name = "b")]
    public byte B { get; set; }
    [DataMember(Name = "a", EmitDefaultValue = false)]
    public byte? A { get; set; }

    public Color() {}
    public Color(byte R, byte G, byte B, byte A = 255)
    {
        this.R = R;
        this.G = G;
        this.B = B;
        this.A = A;
    }
    public Color(VRageMath.Color col)
    {
        R = col.R;
        G = col.G;
        B = col.B;
        A = col.A;
    }

    public VRageMath.Color ToColor()
    {
        return new VRageMath.Color(R, G, B, A ?? 255);
    }
}

[DataContract]
internal class SimpleResult
{
    [DataMember(Name = "data")]
    public object Result { get; set; }
}


}