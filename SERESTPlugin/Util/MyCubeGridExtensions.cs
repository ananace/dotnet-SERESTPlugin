using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;

namespace SERESTPlugin.Util
{

static class MyCubeGridExtensions
{
    public static bool PlayerCanCommunicate(this MyCubeGrid grid, Player player)
    {
        var distance = (player.Character.PositionComp.GetPosition() - grid.PositionComp.GetPosition()).Length();
        if (grid.GridSystems.RadioSystem.Receivers.Any(r => r.CanBeUsedByPlayer(player.ID)))
        {
            var antennas = grid.GetFatBlocks().OfType<IMyRadioAntenna>().Where(b => b.IsWorking);
            if (antennas.Any(ant => distance <= ant.Radius))
                return true;
        }

        if (distance < 1000)
            return true;

        return false;
    }
}

}
