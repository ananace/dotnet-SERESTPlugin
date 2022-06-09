namespace SERESTPlugin
{

public class APIDefinition
{
    public Type Type { get; set; }
    public APIAttribute Attribute { get; set; }

    public string FullPath { get {
        IEnumerable<APIAttribute> nestedApis;
        if (Type.IsNested)
        {
            List<APIAttribute> attribs = new List<APIAttribute>();
            var at = Type.DeclaringType;
            while (at != null)
            {
                var attr = at.GetCustomAttribute<APIAttribute>();
                if (attr != null)
                    attribs.Add(attr);

                at = at.DeclaringType;
            }
            nestedApis = (attribs as IEnumerable<APIAttribute>).Reverse();
        }
        else
            nestedApis = new APIAttribute[0];

        return string.Join("/",
                           nestedApis
                           .Select(a => a.Path)
                           .Concat(new string[] { Attribute.Path }
                                   .Select(s => s.Trim('/'))
                                   .Where(s => !string.IsNullOrEmpty(s))));
    } }
    public IEnumerable<APIEndpointAttribute> Endpoints { get {
        return Type.GetCustomAttributes<APIAttribute>();
    } }
    public IEnumerable<APIDataAttribute> DataSources { get {
        return Type.GetCustomAttributes<APIDataAttribute>();
    } }
}

}
