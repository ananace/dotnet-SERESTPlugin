using System;

namespace SERESTPlugin.Attributes
{

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class APIDataAttribute : Attribute
{
    public string Retrieves { get; set; }

    public APIDataAttribute(string Retrieves) { this.Retrieves = Retrieves; }
}

}