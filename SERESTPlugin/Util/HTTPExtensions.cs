using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace SERESTPlugin.Util
{

static class HTTPExtensions
{
    public static bool TryConvert<T>(this string data, out T result) where T : IConvertible
    {
        result = default;

        if (string.IsNullOrEmpty(data))
            return false;

        try
        {
            result = (T)Convert.ChangeType(data, typeof(T));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
    public static bool TryReadJSON<T>(this string data, out T result) where T : class
    {
        result = default;

        if (string.IsNullOrEmpty(data))
            return false;

        try
        {
            var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings{ DateTimeFormat = new DateTimeFormat("u") });
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(data)))
            {
                result = (T)serializer.ReadObject(stream);
                return true;
            }
        }
        catch (SerializationException)
        {
            return false;
        }
    }

    public static T ReadJSON<T>(this System.Net.HttpListenerRequest req) where T : class
    {
        var serializer = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings{ DateTimeFormat = new DateTimeFormat("u"), KnownTypes = new [] { typeof(APIs.DataTypes.Color), typeof(APIs.DataTypes.Coordinate), typeof(APIs.DataTypes.GPS) } });
        return (T)serializer.ReadObject(req.InputStream);
    }

    public static bool TryReadJSON<T>(this System.Net.HttpListenerRequest req, out T result) where T : class
    {
        result = default;
        if (!req.HasEntityBody)
            return false;

        try
        {
            result = ReadJSON<T>(req);
            return true;
        }
        catch (SerializationException)
        {
            return false;
        }
    }

    public static T ReadObject<T>(this System.Net.HttpListenerRequest req) where T : IConvertible
    {
        if (TryReadObject(req, out T result))
            return result;
        return default;
    }

    public static bool TryReadObject<T>(this System.Net.HttpListenerRequest req, out T result) where T : IConvertible
    {
        result = default;

        if (!req.HasEntityBody)
            return false;

        try
        {
            using (var reader = new StreamReader(req.InputStream))
            {
                var data = reader.ReadToEnd();
                return data.TryConvert(out result);
            }
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static void CloseJSON<T>(this System.Net.HttpListenerResponse resp, T json) where T : class
    {
        CloseJSON(resp, json, typeof(T));
    }

    public static void CloseJSON(this System.Net.HttpListenerResponse resp, object json, Type jsonType)
    {
        try
        {
            resp.ContentType = "application/json";

            var serializer = new DataContractJsonSerializer(jsonType, new DataContractJsonSerializerSettings{ DateTimeFormat = new DateTimeFormat("u"), KnownTypes = new [] { typeof(APIs.DataTypes.Color), typeof(APIs.DataTypes.Coordinate), typeof(APIs.DataTypes.GPS) } });
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, json);

                resp.ContentLength64 = stream.Length;
                resp.Close(stream.ToArray(), false);
            }
        }
        catch (ObjectDisposedException) { }
    }

    public static void CloseString(this System.Net.HttpListenerResponse resp, string data)
    {
        try
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);

            resp.ContentLength64 = bytes.Length;
            resp.ContentType = "text/plain";
            resp.Close(bytes, false);
        }
        catch (ObjectDisposedException) { }
    }

    public static void CloseHttpCode(this System.Net.HttpListenerResponse resp, System.Net.HttpStatusCode code, string message = null)
    {
        try
        {
            resp.StatusCode = (int)code;

            if ((int)code < 400 && string.IsNullOrEmpty(message))
            {
                resp.Close();
                return;
            }

            resp.CloseJSON(new CodeResponse { Status = code.ToString(), Message = message });
        }
        catch (ObjectDisposedException) { }
    }


    [DataContract]
    class CodeResponse
    {
        [DataMember(Name = "status", EmitDefaultValue = false)]
        public string Status { get; set; }
        [DataMember(Name = "message", EmitDefaultValue = false)]
        public string Message { get; set; }
    }
}

}
