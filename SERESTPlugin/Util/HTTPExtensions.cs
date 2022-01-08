using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace SERESTPlugin.Util
{

static class HTTPExtensions
{
    public static T ReadJSON<T>(this System.Net.HttpListenerRequest req)
    {
        var serializer = new DataContractJsonSerializer(typeof(T));
        return (T)serializer.ReadObject(req.InputStream);
    }

    public static void CloseJSON<T>(this System.Net.HttpListenerResponse resp, T json)
    {
        resp.ContentType = "application/json";

        var serializer = new DataContractJsonSerializer(typeof(T));
        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, json);

            resp.ContentLength64 = stream.Length;
            resp.Close(stream.ToArray(), false);
        }
    }
    
    public static void CloseString(this System.Net.HttpListenerResponse resp, string data)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);

        resp.ContentEncoding = System.Text.Encoding.UTF8;
        resp.ContentLength64 = bytes.Length;
        resp.ContentType = "text/plain";
        resp.Close(bytes, false);
    }

    public static void CloseHttpCode(this System.Net.HttpListenerResponse resp, System.Net.HttpStatusCode code, string message = null)
    {
        resp.StatusCode = (int)code;

        if ((code == System.Net.HttpStatusCode.NoContent || code == System.Net.HttpStatusCode.Accepted || code == System.Net.HttpStatusCode.Created) && string.IsNullOrEmpty(message))
        {
            resp.Close();
            return;
        }

        resp.CloseJSON(new CodeResponse { Status = code.ToString(), Message = message });
    }


    [DataContract]
    class CodeResponse
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }
        [DataMember(Name = "message", EmitDefaultValue = false)]
        public string Message { get; set; }
    }
}

}