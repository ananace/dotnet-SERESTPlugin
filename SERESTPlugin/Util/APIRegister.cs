using System;

namespace SERESTPlugin.Util
{

public class APIRegister
{
    readonly APIServer _Server;
    public string Path { get; private set; }

    public APIRegister(APIServer server, string path)
    {
        Path = path;
        _Server = server;
    }

    public APIRegister RegisterSubAPI(string path)
    {
        return new APIRegister(_Server, $"{Path.TrimEnd('/')}/{path.TrimStart('/')}");
    }

    public void RegisterRequest(string method, string path, EventHandler<HTTPEventArgs> action)
    {
        if (path == "")
            _Server.RegisterHandler(method, $"{Path.TrimEnd('/')}", action);
        else
            _Server.RegisterHandler(method, $"{Path.TrimEnd('/')}/{path.TrimStart('/')}", action);
    }

    public void RegisterRequest(string method, EventHandler<HTTPEventArgs> action)
    {
        RegisterRequest(method, "", action);
    }
}

}