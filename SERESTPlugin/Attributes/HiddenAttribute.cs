using System;

namespace SERESTPlugin.Attributes
{

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public class HiddenAttribute : Attribute { }

}
