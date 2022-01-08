using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using SERESTPlugin.Util;

namespace SERESTPlugin.APIs
{

public class Core : IAPI
{
    [DataContract]
    internal class APIInformation
    {
        [DataMember(Name = "version")]
        public string Version { get; set; }
        [DataMember(Name = "endpoints")]
        public string[] Endpoints { get; set; }
    }

    public void Register(APIServer server)
    {
        server.RegisterHandler("GET", "", (s, ev) => {
            ev.Handled = true;
            
            var handled = server.Callbacks.Keys.Select(k => $"{k.Method} {k.Path}").ToArray();
            ev.Context.Response.CloseJSON(new APIInformation { Version = Assembly.GetAssembly(GetType()).ImageRuntimeVersion, Endpoints = handled });
        });
    }
}

}