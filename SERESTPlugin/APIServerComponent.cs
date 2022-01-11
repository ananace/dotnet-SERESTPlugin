using VRage.Game.Components;

namespace SERESTPlugin
{

[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 9001)]
class APIServerComponent : MySessionComponentBase
{
    APIServer server = null;

    public override void Init(VRage.Game.MyObjectBuilder_SessionComponent sessionComponent)
    {
        Util.Logger.Info("APIServerComponent.Init()");
        if (Sandbox.ModAPI.MyAPIGateway.Session.IsServer)
        {
            Util.Logger.Info("IsServer");

            server = new APIServer();
            server.Start();
        }
    }

    public override void LoadData()
    {
        Util.Logger.Info("APServerComponent.LoadData()");
    }

    protected override void UnloadData()
    {
        Util.Logger.Info("APServerComponent.UnloadData()");
        if (Sandbox.ModAPI.MyAPIGateway.Session.IsServer)
            server.Stop();
    }
}

}