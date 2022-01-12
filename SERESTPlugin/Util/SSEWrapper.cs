using System;
using System.IO;
using System.Linq;

namespace SERESTPlugin.Util
{

public class SSEWrapper
{
    public static TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);

    public event EventHandler OnClosed;

    public System.Net.HttpListenerContext Context { get; private set; }
    public DateTime LastData { get; private set; }

    public SSEWrapper(System.Net.HttpListenerContext ctx)
    {
        Context = ctx;

        Context.Response.ContentType = "text/event-stream";
        Context.Response.KeepAlive = true;
    }

    public void Tick()
    {
        if (DateTime.Now - LastData > HeartbeatInterval)
            SendComment();
    }

    public void SendComment(string Comment = null)
    {
        try
        {
            using (var writer = new StreamWriter(Context.Response.OutputStream))
            {
                writer.NewLine = "\n";
                writer.WriteLine($": {Comment}");
                writer.WriteLine();
            }
            Context.Response.OutputStream.Flush();

            LastData = DateTime.Now;
        }
        catch (IOException)
        {
            OnClosed?.Invoke(this, new EventArgs());
        }
    }

    public void SendEvent(string Event, string Data, string Id = null)
    {
        try
        {
            using (var writer = new StreamWriter(Context.Response.OutputStream))
            {
                writer.NewLine = "\n";
                writer.WriteLine($"event: {Event}");
                if (!string.IsNullOrEmpty(Id))
                    writer.WriteLine($"id: {Id}");
                writer.WriteLine($"data: {string.Join("\ndata: ", Data.Split('\n').Where(s => !string.IsNullOrEmpty(s)))}");
                writer.WriteLine();
            }
            Context.Response.OutputStream.Flush();

            LastData = DateTime.Now;
        }
        catch (IOException)
        {
            OnClosed?.Invoke(this, new EventArgs());
        }
    }

    public void SendJSON<T>(string Event, T Data, string Id = null)
    {
        var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, Data);

            SendEvent(Event, System.Text.Encoding.UTF8.GetString(stream.ToArray()), Id);
        }
    }
}

}
