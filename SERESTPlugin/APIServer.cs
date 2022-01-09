using SERESTPlugin.Attributes;
using SERESTPlugin.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;

namespace SERESTPlugin
{

public class APIServer : IDisposable
{
    HttpListener _listen;
    Queue<HttpListenerContext> _waiting = new Queue<HttpListenerContext>();

    public static IList<IAPI> ManualAPIs { get; private set; } = new List<IAPI> { };

    readonly Dictionary<Request, List<EventHandler<HTTPEventArgs>>> _Callbacks = new Dictionary<Request, List<EventHandler<HTTPEventArgs>>>();
    public IReadOnlyDictionary<Request, List<EventHandler<HTTPEventArgs>>> Callbacks { get { return _Callbacks; } }

    public string BasePath { get; set; } = "/api";
    public string Hostname { get; set; } = "localhost";
    public ushort Port { get; set; } = 9000;

    public bool IsListening { get { return _listen?.IsListening ?? false; } }

    public string ListenPrefix { get { return $"http://{Hostname}:{Port}/"; } }

    public void Dispose()
    {
        Stop();
    }

    public void Start()
    {
        if (IsListening)
            return;

        Logger.Info($"APIServer: Starting up on {ListenPrefix}...");

        _listen = new HttpListener();
        _listen.Prefixes.Add(ListenPrefix);
        foreach (var api in ManualAPIs)
            api.Register(this);

        foreach (var api in Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(BaseAPI).IsAssignableFrom(t) && t.HasAttribute<APIAttribute>()))
        {
            IEnumerable<APIAttribute> nestedAttribs;
            if (api.IsNested)
            {
                List<APIAttribute> attribs = new List<APIAttribute>();
                var at = api.DeclaringType;
                while (at != null)
                {
                    var attr = at.GetCustomAttribute<APIAttribute>();
                    if (attr != null)
                        attribs.Add(attr);

                    at = at.DeclaringType;
                }
                nestedAttribs = (attribs as IEnumerable<APIAttribute>).Reverse();
            }
            else
                nestedAttribs = new APIAttribute[0];

            var apiAttribs = api.GetCustomAttributes<APIAttribute>();
            foreach (var apiAttrib in apiAttribs)
            {
                // if (Sandbox.Game.World.MySession.Static.IsServer && !apiAttrib.OnDedicated)
                //     continue;
                // if (!Sandbox.Game.World.MySession.Static.IsServer && !apiAttrib.OnLocal)
                //     continue;

                foreach (var endpoint in api.GetMethods().Where(m => m.HasAttribute<APIEndpointAttribute>()))
                {
                    var endpointAttribs = endpoint.GetCustomAttributes<APIEndpointAttribute>();
                    foreach (var endpointAttrib in endpointAttribs)
                    {
                        var fullPath = string.Join("/", nestedAttribs.Select(a => a.Path).Concat(new string[] { apiAttrib.Path, endpointAttrib.Path }.Select(s => s.Trim('/')).Where(s => !string.IsNullOrEmpty(s))));

                        RegisterHandler(endpointAttrib.Method, fullPath, (_, ev) => {
                            HandleBaseAPIRequest(api, apiAttrib, endpoint, endpointAttrib, ev);
                        });
                    }
                }
            }
        }

        _listen.Start();

        _waiting.Clear();
        _listen.BeginGetContext(OnContextReceived, _listen);

        Logger.Info($"APIServer: Now listening on {string.Join(", ",_listen.Prefixes.ToArray())}, with {Callbacks.Count} callbacks registered.");
    }

    public void Stop()
    {
        _waiting.Clear();

        _listen?.Stop();
        _listen = null;
    }

    public void Tick()
    {
        if (!IsListening)
            return;

        lock(_waiting)
        {
            var start = DateTime.Now;
            do
            {
                if (_waiting.Count == 0)
                    break;

                var ctx = _waiting.Dequeue();
                HandleRequest(ctx);
            }
            while (DateTime.Now - start < TimeSpan.FromMilliseconds(5));
        }
    }

    public APIRegister RegisterAPI(string path)
    {
        return new APIRegister(this, path);
    }

    public void RegisterHandler(string method, string path, EventHandler<HTTPEventArgs> action)
    {
        var fullPath = $"{BasePath.TrimEnd('/')}/{path.Trim('/')}";
        if (string.IsNullOrEmpty(path))
            fullPath = BasePath.TrimEnd('/');

        Logger.Debug($"APIServer: Adding handler for {method} {fullPath}");

        // if (!_listen.Prefixes.Contains(fullPath))
        //     _listen.Prefixes.Add(fullPath);

        var req = new Request { Method = method, Path = fullPath };
        if (!Callbacks.ContainsKey(req))
            _Callbacks[req] = new List<EventHandler<HTTPEventArgs>>();

        Callbacks[req].Add(action);
    }

    void OnContextReceived(IAsyncResult result)
    {
        if (!IsListening)
            return;

        var ctx = _listen.EndGetContext(result);

        lock(_waiting)
            _waiting.Enqueue(ctx);

        _listen.BeginGetContext(OnContextReceived, _listen);
    }

    void HandleBaseAPIRequest(Type api, APIAttribute apiAttrib, MethodInfo endpoint, APIEndpointAttribute endpointAttrib, HTTPEventArgs ev)
    {
        ev.Handled = true;
        
        if ((endpointAttrib.NeedsBody || (endpoint.GetParameters().Any() && !endpoint.GetParameters().First().IsOptional)) && !ev.Context.Request.HasEntityBody)
        {
            ev.Context.Response.CloseHttpCode(HttpStatusCode.BadRequest, "Body not provided");
            return;
        }

        try
        {
            var handler = Activator.CreateInstance(api);
            api.GetProperty("APIServer").SetValue(handler, this);
            api.GetProperty("EventArgs").SetValue(handler, ev);

            IDictionary<string, object> storage = api.GetProperty("Data").GetValue(handler) as IDictionary<string, object>;
            var needs = (apiAttrib.Needs ?? new string[0]).Union(endpointAttrib.NeedsData ?? new string[0]);

            foreach (var need in needs)
                foreach (var data in api.GetMethods().Where(m => m.HasAttribute<APIDataAttribute>()))
                {
                    var dataAttrib = data.GetCustomAttribute<APIDataAttribute>();
                    if (dataAttrib.Retrieves != need)
                        continue;

                    if (!storage.ContainsKey(dataAttrib.Retrieves))
                    {
                        var retrieved = data.Invoke(handler, new object[0]);
                        if (retrieved != null)
                            storage[dataAttrib.Retrieves] = retrieved;
                    }
                }

            // Check if all needs have been satisfied
            if (!needs.Intersect(storage.Keys).SequenceEqual(needs))
                throw new HTTPException(HttpStatusCode.BadRequest, $"Failed to satisfy all request needs, necessary: {string.Join(",", needs)}, satisfied: {string.Join(", ", storage.Keys)}");

            object result = null;
            if (endpoint.GetParameters().Any())
            {
                var parameter = endpoint.GetParameters().First();
                object[] input = null;

                if (ev.Context.Request.HasEntityBody)
                {
                    if (typeof(IConvertible).IsAssignableFrom(parameter.ParameterType))
                    {
                        using (var reader = new StreamReader(ev.Context.Request.InputStream))
                        {
                            var data = reader.ReadToEnd();
                            try
                            {
                                var converted = Convert.ChangeType(data, parameter.ParameterType);
                                input = new object[] { converted };
                            }
                            catch (FormatException ex)
                            {
                                throw new HTTPException(HttpStatusCode.BadRequest, $"Failed to parse request body: {ex.Message}");
                            }
                            catch (InvalidCastException ex)
                            {
                                throw new HTTPException(HttpStatusCode.BadRequest, $"Failed to parse request body: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            var serializer = new DataContractJsonSerializer(endpoint.GetParameters().First().ParameterType, new DataContractJsonSerializerSettings{ DateTimeFormat = new DateTimeFormat("u") });
                            input = new object[] { serializer.ReadObject(ev.Context.Request.InputStream) };
                        }
                        catch (SerializationException ex)
                        {
                            throw new HTTPException(HttpStatusCode.BadRequest, $"Failed to parse request body: {ex.Message}");
                        }
                    }
                }
                else
                    input = new object[] { parameter.DefaultValue };

                result = endpoint.Invoke(handler, input);
            }
            else
                result = endpoint.Invoke(handler, null);
            
            if (!endpointAttrib.ClosesResponse)
            {
                if (result != null)
                {
                    if (result is IConvertible)
                        ev.Context.Response.CloseString((result as IConvertible).ToString());
                    else
                        ev.Context.Response.CloseJSON(result, result.GetType());
                }
                else
                    ev.Context.Response.CloseHttpCode(HttpStatusCode.OK);
            }
        }
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException is HTTPException httpEx)
                ev.Context.Response.CloseHttpCode(httpEx.Code, httpEx.Message);
            else
            {
                Logger.Error($"APIServer: {ex.GetType().Name}; {ex.Message}\n{ex.StackTrace}");
                throw ex.InnerException;
            }
        }
        catch (HTTPException ex)
        {
            ev.Context.Response.CloseHttpCode(ex.Code, ex.Message);
        }
    }

    void HandleRequest(HttpListenerContext context)
    {
        try
        {
            Logger.Debug($"APIServer: Handling {context.Request.HttpMethod} {context.Request.Url}");

            var uri = context.Request.Url.AbsolutePath;
            if (!uri.StartsWith(BasePath))
            {
                context.Response.CloseHttpCode(HttpStatusCode.NotFound);
                return;
            }

            var eventArgs = new HTTPEventArgs { Context = context };
            foreach (var callback in Callbacks)
            {
                var matches = new Dictionary<string, string>();
                if (callback.Key.Method != context.Request.HttpMethod || !PathMatches(callback.Key.Path, uri, matches))
                    continue;

                eventArgs.Components = matches;
                foreach (var action in callback.Value)
                {
                    action.Invoke(this, eventArgs);

                    if (eventArgs.Handled)
                        break;
                }

                if (eventArgs.Handled)
                    break;
            }

            if (!eventArgs.Handled)
                context.Response.CloseHttpCode(HttpStatusCode.NotFound, "No endpoint chose to handle the request");
        }
        catch (Exception ex)
        {
            Logger.Error($"APIServer: {ex.GetType().Name}; {ex.Message}\n{ex.StackTrace}");
            context.Response.CloseHttpCode(HttpStatusCode.InternalServerError, $"An {ex.GetType().Name} occurred when handling the request.");
        }
    }

    bool PathMatches(string registered, string path, Dictionary<string, string> matches)
    {
        if (registered.Contains("(") || registered.Contains(")"))
        {
            var rex = new Regex($"^{registered}$");
            var match = rex.Match(path);

            if (!match.Success)
                return false;

            foreach (var group in rex.GetGroupNames())
                matches[group] = match.Groups[group]?.Value;

            return true;
        }

        return path == registered;
    }

    public struct Request
    {
        public string Method { get; set; }
        public string Path { get; set; }
    }
}

}
