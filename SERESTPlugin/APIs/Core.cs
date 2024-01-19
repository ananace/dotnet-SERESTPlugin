using SERESTPlugin.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SERESTPlugin.APIs
{

[API("/")]
public class CoreAPI : BaseAPI
{
    [DataContract]
    public class APIInformation
    {
        [DataMember(Name = "version")]
        public string Version { get; set; }
        [DataMember(Name = "apis")]
        public IEnumerable<string> APIs { get; set; }
        [DataMember(Name = "manual_apis")]
        public IEnumerable<string> ManualAPIs { get; set; }
    }

    [APIEndpoint("GET", "/version")]
    public APIInformation GetInfo()
    {
        return new APIInformation {
            Version = Assembly.GetAssembly(GetType()).ImageRuntimeVersion,
            APIs = Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(BaseAPI).IsAssignableFrom(t) && t.HasAttribute<APIAttribute>()).Select(a => a.GetCustomAttribute<APIAttribute>().Path),
            ManualAPIs = APIServer.ManualAPIs.Select(api => api.GetType().ToString())
        };
    }

    [APIEndpoint("GET", "/")]
    public Dictionary<string, APIDefinition> GetData()
    {
        return APIServer.AutomaticAPIs.ToDictionary(def => def.Attribute.Path);
    }
}

}
