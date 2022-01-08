namespace SERESTPlugin.Util
{

internal class Logger
{
    public static string Prefix { get; set; } = "SERESTPlugin - ";

    public static void Log(VRage.Utils.MyLogSeverity Level, string Message)
    {
        VRage.Utils.MyLog.Default.Log(Level, $"{Prefix}{Message}");
    }

    public static void Debug(string Message)
    {
        Log(VRage.Utils.MyLogSeverity.Debug, Message);
    }

    public static void Info(string Message)
    {
        Log(VRage.Utils.MyLogSeverity.Info, Message);
    }

    public static void Warning(string Message)
    {
        Log(VRage.Utils.MyLogSeverity.Warning, Message);
    }

    public static void Error(string Message)
    {
        Log(VRage.Utils.MyLogSeverity.Error, Message);
    }
}

}