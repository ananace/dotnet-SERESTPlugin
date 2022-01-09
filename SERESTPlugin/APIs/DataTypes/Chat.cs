using System.Runtime.Serialization;

namespace SERESTPlugin.APIs.DataTypes
{

[DataContract]
internal class ChatMessage
{
    [DataMember(Name = "sender")]
    public long Sender { get; set; }
    [DataMember(Name = "target")]
    public long Target { get; set; }
    [DataMember(Name = "author")]
    public string Author { get; set; }
    [DataMember(Name = "author_font")]
    public string AuthorFont { get; set; }
    [DataMember(Name = "channel")]
    public string Channel { get; set; }
    [DataMember(Name = "message")]
    public string Message { get; set; }
    [DataMember(Name = "timestamp")]
    public System.DateTime Timestamp { get; set; }

    public ChatMessage() {}
    public ChatMessage(Sandbox.Game.Entities.Character.MyUnifiedChatItem message)
    {
        if (!string.IsNullOrEmpty(message.CustomAuthor))
            Author = message.CustomAuthor;
        else
        {
            var senderEnt = Sandbox.Game.Entities.MyEntities.GetEntityById(message.SenderId);
            Author = senderEnt?.GetFriendlyName();
        }
        Sender = message.SenderId;
        Target = message.TargetId;
        Channel = message.Channel.ToString();
        Message = message.Text;
        Timestamp = message.Timestamp;
        AuthorFont = message.AuthorFont;
    }
}

[DataContract]
internal class ChatHistory
{
    [DataMember(Name = "messages")]
    public ChatMessage[] Messages { get; set; }
}

}