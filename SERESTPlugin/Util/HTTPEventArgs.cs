using System;
using System.Collections.Generic;
using System.Net;

namespace SERESTPlugin.Util
{

public class HTTPEventArgs : EventArgs
{
    public HttpListenerContext Context { get; set; }
    public Dictionary<string, string> Components { get; set; }
    public bool Handled { get; set; } = false;
}

}