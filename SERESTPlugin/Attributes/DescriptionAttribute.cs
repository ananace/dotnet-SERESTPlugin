using System;

namespace SERESTPlugin.Attributes
{

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
class DescriptionAttribute : Attribute
{
    public string Description { get; set; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}

}
