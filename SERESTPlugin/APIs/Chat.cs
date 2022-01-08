using System.IO;
using System.Linq;
using SERESTPlugin.Util;

namespace SERESTPlugin.APIs
{

public class Chat : IAPI
{
    public void Register(APIServer server)
    {
        var api = server.RegisterAPI("chat");

        api.RegisterRequest("POST", (s, ev) => {
            ev.Handled = true;
            string message = null;
            using (var reader = new StreamReader(ev.Context.Request.InputStream))
                message = reader.ReadLine();

            var sys = Sandbox.Game.World.MySession.Static.ChatSystem;
            if (sys.CommandSystem.CanHandle(message))
            {
                string name = Sandbox.Game.World.MySession.Static.LocalHumanPlayer.DisplayName;
                Sandbox.Game.Gui.MyHud.Chat.ShowMessage(name, message);
                sys.CommandSystem.Handle(message);
            }
            else
            {
                var query = System.Web.HttpUtility.ParseQueryString(ev.Context.Request.Url.Query);
                if (!string.IsNullOrEmpty(query["author"]))
                {
                    Sandbox.Game.Gui.MyHud.Chat.ShowMessageScripted(query["author"], message);
                    Sandbox.Game.Gui.MyHud.Chat.ShowMessage(query["author"], message);
                }
                else
                {
                    string name = Sandbox.Game.World.MySession.Static.LocalHumanPlayer.DisplayName;
                    Sandbox.Game.Gui.MyHud.Chat.ShowMessageColoredSP(message, sys.CurrentChannel);
                    sys.ChatHistory.EnqueueMessage(message, sys.CurrentChannel, Sandbox.Game.World.MySession.Static.LocalPlayerId);
                }
            }
            ev.Context.Response.CloseHttpCode(System.Net.HttpStatusCode.Accepted);
        });
    }
}

}