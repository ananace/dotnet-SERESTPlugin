using VRage.Game.Components;

namespace SERESTPlugin
{

[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
class APIServerComponent : MySessionComponentBase
{
    APIServer server = null;

    public override void Init(VRage.Game.MyObjectBuilder_SessionComponent sessionComponent)
    {
        Util.Logger.Info("APIServerComponent.Init()");

        server = new APIServer();
        server.Start();
    }

    public override void LoadData()
    {
        Util.Logger.Info("APServerComponent.LoadData()");
    }

    protected override void UnloadData()
    {
        Util.Logger.Info("APServerComponent.UnloadData()");
        server.Stop();
    }
}

}