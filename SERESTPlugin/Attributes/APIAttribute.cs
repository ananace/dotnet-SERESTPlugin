using System;

namespace SERESTPlugin.Attributes
{

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class APIAttribute : Attribute
{
    public string Path { get; private set; }
    public bool OnLocal { get; set; } = true;
    public bool OnDedicated { get; set; } = true;

    public string[] Needs { get; set; }

    public APIAttribute(string Path) { this.Path = Path; }
}

}
