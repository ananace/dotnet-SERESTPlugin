using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.World;

namespace SERESTPlugin.Util
{

static class MyCubeGridExtensions
{
    public static bool PlayerCanCommunicate(this MyCubeGrid grid, MyPlayer player)
    {
        var receivers = grid.GridSystems.RadioSystem.Receivers.Where(r => r.CanBeUsedByPlayer(player.Identity.IdentityId));

        return false;
    }
}

}
