using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace SERESTPlugin.Util
{

static class BlockExtensions
{
    public static bool TryGetValue<T>(this ITerminalProperty prop, IMyTerminalBlock block, out T value)
    {
        if (prop is ITerminalProperty<T> tProp)
        {
            value = tProp.GetValue(block);
            return true;
        }

        value = default;
        return false;
    }

    public static bool TrySetValue<T>(this ITerminalProperty prop, IMyTerminalBlock block, in T value)
    {
        if (prop is ITerminalProperty<T> tProp)
        {
            tProp.SetValue(block, value);
            return true;
        }

        return false;
    }

    public static bool TryGetProperty<T>(this IMyTerminalBlock block, string propertyName, out T value)
    {
        var property = block.GetProperty(propertyName);
        if (property == null)
        {
            value = default;
            return false;
        }

        return TryGetProperty(block, property, out value);
    }

    public static bool TryGetProperty<T>(this IMyTerminalBlock block, ITerminalProperty property, out T value)
    {
        if (property is ITerminalProperty<T> tProperty)
        {
            value = tProperty.GetValue(block);
            return true;
        }

        value = default;
        return false;
    }

    public static bool TrySetProperty<T>(this IMyTerminalBlock block, string propertyName, in T value)
    {
        var property = block.GetProperty(propertyName);
        if (property == null)
            return false;

        return TrySetProperty(block, property, in value);
    }

    public static bool TrySetProperty<T>(this IMyTerminalBlock block, ITerminalProperty property, in T value)
    {
        if (property is ITerminalProperty<T> tProperty)
        {
            tProperty.SetValue(block, value);
            return true;
        }

        return false;
    }
}

}