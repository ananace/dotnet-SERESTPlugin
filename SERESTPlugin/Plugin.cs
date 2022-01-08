//using HarmonyLib;
using System.Reflection;
using VRage.Plugins;

namespace SERESTPlugin
{

public class Plugin : IPlugin
{
    public void Init(object gameInstance)   
    {
        //new Harmony("Ananace.SERESTPlugin").PatchAll(Assembly.GetExecutingAssembly());
    }

    public void Dispose() {}
    public void Update() {}
}

}