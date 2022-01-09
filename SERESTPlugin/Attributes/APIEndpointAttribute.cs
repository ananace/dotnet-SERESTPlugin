using System;

namespace SERESTPlugin.Attributes
{

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
class APIEndpointAttribute : Attribute
{
    public string Path { get; set; }
    public string Method { get; private set; }
    public bool ClosesResponse { get; set; }
    public bool NeedsBody { get; set; }
    public string[] NeedsData { get; set; }

    public APIEndpointAttribute(string Method, string Path) { this.Method = Method; this.Path = Path; }
}

}