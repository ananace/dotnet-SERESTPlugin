using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using SERESTPlugin.Util;

namespace SERESTPlugin
{

public class APIServer : IDisposable
{
    HttpListener _listen;
    IAsyncResult _waiting;

    static readonly IAPI[] _APIs = {
        new APIs.Core(),

        new APIs.Chat(),
        new APIs.GPS(),
        new APIs.Grid(),
        new APIs.Player()
    };

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
        foreach (var api in _APIs)
            api.Register(this);
        _listen.Start();

        _waiting = _listen.BeginGetContext(OnContextReceived, _listen);

        Logger.Info($"APIServer: Now listening on {string.Join(", ",_listen.Prefixes.ToArray())}, with {Callbacks.Count} callbacks registered.");
    }

    public void Stop()
    {
        _listen?.Stop();
        _listen = null;
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
        {
            Logger.Debug("APIServer: Received context when not listening, closing");

            var context = _listen?.EndGetContext(result);
            context?.Response?.CloseHttpCode(HttpStatusCode.ServiceUnavailable);

            return;
        }

        var ctx = _listen.EndGetContext(result);

        HandleRequest(ctx);

        _waiting = _listen.BeginGetContext(OnContextReceived, _listen);
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
                    {
                        Logger.Debug($"APIServer: Handled by {action.Target}");
                        break;
                    }
                }

                if (eventArgs.Handled)
                    break;
            }

            if (!eventArgs.Handled)
                context.Response.CloseHttpCode(HttpStatusCode.NotFound, "No endpoint handled the request");
        }
        catch (Exception ex)
        {
            Logger.Error($"APIServer: {ex.GetType().Name}: {ex.Message};\n{ex.StackTrace}");
            context.Response.CloseHttpCode(HttpStatusCode.InternalServerError, $"An {ex.GetType().Name} occurred when handling the request.");
        }
    }

    bool PathMatches(string registered, string path, Dictionary<string, string> matches)
    {
        if (registered.Contains("(") || registered.Contains(")"))
        {
            var rex = new Regex($"^{registered}$");
            var match = rex.Match(path);

            Logger.Debug($"{path} matches {rex}? {match.Success}");

            if (!match.Success)
                return false;

            foreach (var group in rex.GetGroupNames())
                matches[group] = match.Groups[group]?.Value;

            return true;
        }

        Logger.Debug($"{path} == {registered}? {path == registered}");

        return path == registered;
    }

    public struct Request
    {
        public string Method { get; set; }
        public string Path { get; set; }
    }
}

}
