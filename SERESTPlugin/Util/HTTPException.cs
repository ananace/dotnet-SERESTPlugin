using System;
using System.Net;

namespace SERESTPlugin.Util
{

public class HTTPException : Exception
{
    public HttpStatusCode Code { get; set; }

    public HTTPException(HttpStatusCode Code)
    {
        this.Code = Code;
    }
    public HTTPException(HttpStatusCode Code, string Message)
        : base(Message)
    {
        this.Code = Code;
    }
}

}