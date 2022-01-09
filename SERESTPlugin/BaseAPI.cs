using System.Collections.Generic;
using System.Net;
using SERESTPlugin.Util;

namespace SERESTPlugin
{

public class BaseAPI
{
    public APIServer APIServer { get; set; }
    public HTTPEventArgs EventArgs { get; set; }
    public IDictionary<string, object> Data { get; private set; } = new Dictionary<string, object>();

    protected HttpListenerContext Context { get { return EventArgs.Context; } }
    protected HttpListenerRequest Request { get { return EventArgs.Context.Request; } }
    protected HttpListenerResponse Response { get { return EventArgs.Context.Response; } }
}

}
