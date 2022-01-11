using SERESTPlugin.Attributes;
using SERESTPlugin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;

namespace SERESTPlugin.APIs
{

public abstract class R0GridAPI : BaseAPI
{
    public abstract MyCubeGrid FindGrid();
    public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

    public virtual bool CanCommunicate()
    {
        if (Sandbox.Game.World.MySession.Static.LocalPlayerId != 0)
            return PlayerCanCommunicate(Grid, Sandbox.Game.World.MySession.Static.LocalPlayerId);
        return true;
    }

    // TODO: Optional disable
    protected bool PlayerCanCommunicate(MyCubeGrid grid, long playerId)
    {
        var ply = Sandbox.Game.World.MySession.Static.Players.TryGetIdentity(playerId);
        if (ply == null)
        {
            Logger.Debug($"Player: null");
            return true;
        }

        var distance = (ply.Character.PositionComp.GetPosition() - grid.PositionComp.GetPosition()).Length();
        if (grid.GridSystems.RadioSystem.Receivers.Any(r => r.CanBeUsedByPlayer(playerId)))
        {
            var antennas = grid.GetFatBlocks().OfType<IMyRadioAntenna>().Where(b => b.IsWorking);
            if (antennas.Any(ant => distance <= ant.Radius))
                return true;
        }

        if (distance < 1000)
            return true;
        throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to communicate with the target grid");
    }

    [APIEndpoint("GET", "/")]
    public DataTypes.GridInformation GetInformation()
    {
        return new DataTypes.GridInformation(Grid);
    }

    [APIEndpoint("GET", "/dampeners")]
    public bool GetDampeners()
    {
        return Grid.DampenersEnabled;
    }
    [APIEndpoint("POST", "/dampeners")]
    public void SetDampeners(bool? wanted = null)
    {
        throw new HTTPException(System.Net.HttpStatusCode.NotImplemented);
    }
}

public abstract class R0BlockAPI : BaseAPI
{
    public abstract IMyTerminalBlock FindBlock();
    public IMyTerminalBlock Block { get { return Data["block"] as IMyTerminalBlock; } }

    public virtual bool CanCommunicate()
    {
        if (Sandbox.Game.World.MySession.Static.LocalPlayerId != 0)
            return PlayerCanCommunicate(Block, Sandbox.Game.World.MySession.Static.LocalPlayerId);
        return true;
    }

    protected bool PlayerCanCommunicate(IMyTerminalBlock block, long playerId)
    {
        var ply = Sandbox.Game.World.MySession.Static.Players.TryGetIdentity(playerId);
        if (ply == null)
        {
            Logger.Debug($"Player: null");
            return true;
        }
        var grid = block.CubeGrid as MyCubeGrid;

        var distance = (ply.Character.PositionComp.GetPosition() - grid.PositionComp.GetPosition()).Length();
        if (grid.GridSystems.RadioSystem.Receivers.Any(r => r.CanBeUsedByPlayer(playerId)))
        {
            var antennas = grid.GetFatBlocks().OfType<IMyRadioAntenna>().Where(b => b.IsWorking);
            if (antennas.Any(ant => distance <= ant.Radius))
                return true;
        }

        if (distance < 1000)
            return true;
        throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to communicate with the target block");
    }

    [APIEndpoint("GET", "/")]
    public DataTypes.BlockInformation GetInformation()
    {
        return new DataTypes.BlockInformation(Block);
    }

    [APIEndpoint("GET", "/name")]
    public string GetName()
    {
        return Block.CustomName;
    }
    [APIEndpoint("POST", "/name")]
    public void SetName(string name)
    {
        Block.CustomName = name;
    }

    [APIEndpoint("GET", "/data")]
    public string GetData()
    {
        return Block.CustomData;
    }
    [APIEndpoint("POST", "/data")]
    public void SetData(string data)
    {
        Block.CustomData = data;
    }
    [APIEndpoint("DELETE", "/data")]
    public void RemoveData()
    {
        Block.CustomData = null;
    }

    [APIEndpoint("GET", "/functional")]
    public bool GetFunctional()
    {
        if (!(Block is IMyFunctionalBlock functionalBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement functional");
        return functionalBlock.Enabled;
    }
    [APIEndpoint("POST", "/functional")]
    public void SetFunctional(bool wanted = true)
    {
        if (!(Block is IMyFunctionalBlock functionalBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement functional");

        functionalBlock.Enabled = wanted;
    }
    [APIEndpoint("DELETE", "/functional")]
    public void UnsetFunctional()
    {
        if (!(Block is IMyFunctionalBlock functionalBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement functional");

        functionalBlock.Enabled = false;
    }

    [APIEndpoint("GET", "/text")]
    public string GetText()
    {
        if (!(Block is IMyTextSurface textBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement text");

        return textBlock.GetText();
    }
    [APIEndpoint("POST", "/text")]
    public void SetText(string text)
    {
        if (!(Block is IMyTextSurface textBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement text");

        textBlock.WriteText(text);
    }
    [APIEndpoint("DELETE", "/text")]
    public void RemoveText()
    {
        if (!(Block is IMyTextSurface textBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement text");

        textBlock.WriteText("");
    }

    [APIEndpoint("GET", "/light")]
    public DataTypes.LightBlock GetLight()
    {
        if (!(Block is IMyLightingBlock lightBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement light");

        return new DataTypes.LightBlock(lightBlock);
    }
    [APIEndpoint("POST", "/light")]
    public void SetLight(DataTypes.LightBlock settings)
    {
        if (!(Block is IMyLightingBlock lightBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement light");

        if (settings.BlinkIntervalSeconds.HasValue)
            lightBlock.BlinkIntervalSeconds = settings.BlinkIntervalSeconds.Value;
        if (settings.BlinkLength.HasValue)
            lightBlock.BlinkLength = settings.BlinkLength.Value;
        if (settings.BlinkOffset.HasValue)
            lightBlock.BlinkOffset = settings.BlinkOffset.Value;
        if (settings.Color != null)
            lightBlock.Color = settings.Color.ToColor();
        if (settings.Falloff.HasValue)
            lightBlock.Falloff = settings.Falloff.Value;
        if (settings.Intensity.HasValue)
            lightBlock.Intensity = settings.Intensity.Value;
        if (settings.Radius.HasValue)
            lightBlock.Radius = settings.Radius.Value;
    }

    [APIEndpoint("GET", "/thrust")]
    public DataTypes.ThrustBlock GetThrust()
    {
        if (!(Block is IMyThrust thrustBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement thrust");

        return new DataTypes.ThrustBlock(thrustBlock);
    }
    [APIEndpoint("POST", "/thrust")]
    public void SetThrust(DataTypes.ThrustBlock settings)
    {
        if (!(Block is IMyThrust thrustBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement thrust");

        if (settings.Override.HasValue)
            thrustBlock.ThrustOverride = settings.Override.Value;
        if (settings.OverridePercentage.HasValue)
            thrustBlock.ThrustOverridePercentage = settings.OverridePercentage.Value;
    }
    [APIEndpoint("DELETE", "/thrust")]
    public void StopThrust()
    {
        if (!(Block is IMyThrust thrustBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement thrust");

        thrustBlock.ThrustOverride = 0;
    }

    [APIEndpoint("GET", "/gyro")]
    public DataTypes.GyroBlock GetGyro()
    {
        if (!(Block is IMyGyro gyroBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement gyro");

        return new DataTypes.GyroBlock(gyroBlock);
    }
    [APIEndpoint("POST", "/gyro")]
    public void SetGyro(DataTypes.GyroBlock settings)
    {
        if (!(Block is IMyGyro gyroBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement gyro");

        if (settings.Override.HasValue)
            gyroBlock.GyroOverride = settings.Override.Value;
        if (settings.Power.HasValue)
            gyroBlock.GyroPower = settings.Power.Value;
        if (settings.Pitch.HasValue)
            gyroBlock.Pitch = settings.Pitch.Value;
        if (settings.Roll.HasValue)
            gyroBlock.Roll = settings.Roll.Value;
        if (settings.Yaw.HasValue)
            gyroBlock.Yaw = settings.Yaw.Value;
    }
    [APIEndpoint("DELETE", "/gyro")]
    public void ResetGyro()
    {
        if (!(Block is IMyGyro gyroBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement gyro");

        gyroBlock.GyroOverride = false;
        gyroBlock.GyroPower = 1.0f;
        gyroBlock.Pitch = 0;
        gyroBlock.Yaw = 0;
        gyroBlock.Roll = 0;
    }

    [APIEndpoint("GET", "/programmable")]
    public DataTypes.ProgrammableBlock GetProgram()
    {
        if (!(Block is IMyProgrammableBlock programmableBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement programmable");

        return new DataTypes.ProgrammableBlock(programmableBlock);
    }
    [APIEndpoint("POST", "/programmable/recompile")]
    public void RecompileProgram()
    {
        if (!(Block is IMyProgrammableBlock programmableBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement programmable");

        programmableBlock.Recompile();
    }
    [APIEndpoint("POST", "/programmable/run")]
    public void RunProgram(string argument = null)
    {
        if (!(Block is IMyProgrammableBlock programmableBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement programmable");

        if (string.IsNullOrEmpty(argument))
            programmableBlock.Run();
        else
            programmableBlock.Run(argument);
    }
    [APIEndpoint("GET", "/programmable/script")]
    public string GetProgramScript()
    {
        if (!(Block is IMyProgrammableBlock programmableBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement programmable");

        return programmableBlock.ProgramData;
    }
    [APIEndpoint("POST", "/programmable/script")]
    public void SetProgramScript(string text)
    {
        if (!(Block is IMyProgrammableBlock programmableBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement programmable");

        programmableBlock.ProgramData = text;
    }
}

public abstract class R0MultiBlockAPI : BaseAPI
{
    public abstract IEnumerable<IMyTerminalBlock> FindBlocks();
    public IEnumerable<IMyTerminalBlock> Blocks { get { return Data["blocks"] as IEnumerable<IMyTerminalBlock>; } }

    public virtual bool CanCommunicate()
    {
        if (Sandbox.Game.World.MySession.Static.LocalPlayerId != 0)
            return PlayerCanCommunicate(Blocks.First(), Sandbox.Game.World.MySession.Static.LocalPlayerId);
        return true;
    }

    protected bool PlayerCanCommunicate(IMyTerminalBlock block, long playerId)
    {
        var ply = Sandbox.Game.World.MySession.Static.Players.TryGetIdentity(playerId);
        if (ply == null)
        {
            Logger.Debug($"Player: null");
            return true;
        }
        var grid = block.CubeGrid as MyCubeGrid;

        var distance = (ply.Character.PositionComp.GetPosition() - grid.PositionComp.GetPosition()).Length();
        if (grid.GridSystems.RadioSystem.Receivers.Any(r => r.CanBeUsedByPlayer(playerId)))
        {
            var antennas = grid.GetFatBlocks().OfType<IMyRadioAntenna>().Where(b => b.IsWorking);
            if (antennas.Any(ant => distance <= ant.Radius))
                return true;
        }

        if (distance < 1000)
            return true;
        throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to communicate with the target block");
    }

    [APIEndpoint("GET", "/")]
    public IEnumerable<DataTypes.BlockInformation> GetInformation()
    {
        return Blocks.Select(b => new DataTypes.BlockInformation(b));
    }
    [APIEndpoint("POST", "/name")]
    public void SetName(string name)
    {
        Blocks.ForEach(b => b.CustomName = name);
    }

    [APIEndpoint("POST", "/data")]
    public void SetData(string data)
    {
        Blocks.ForEach(b => b.CustomData = data);
    }
    [APIEndpoint("DELETE", "/data")]
    public void RemoveData()
    {
        Blocks.ForEach(b => b.CustomData = null);
    }

    [APIEndpoint("POST", "/functional")]
    public void SetFunctional(bool wanted = true)
    {
        Blocks.OfType<IMyFunctionalBlock>().ForEach(b => b.Enabled = wanted);
    }
    [APIEndpoint("DELETE", "/functional")]
    public void UnsetFunctional()
    {
        Blocks.OfType<IMyFunctionalBlock>().ForEach(b => b.Enabled = false);
    }

    [APIEndpoint("POST", "/text")]
    public void SetText(string data)
    {
        Blocks.OfType<IMyTextSurface>().ForEach(b => b.WriteText(data));
    }
    [APIEndpoint("DELETE", "/text")]
    public void RemoveText()
    {
        Blocks.OfType<IMyTextSurface>().ForEach(b => b.WriteText(""));
    }

    [APIEndpoint("POST", "/light")]
    public void SetLight(DataTypes.LightBlock settings)
    {
        foreach (var lightBlock in Blocks.OfType<IMyLightingBlock>())
        {
            if (settings.BlinkIntervalSeconds.HasValue)
                lightBlock.BlinkIntervalSeconds = settings.BlinkIntervalSeconds.Value;
            if (settings.BlinkLength.HasValue)
                lightBlock.BlinkLength = settings.BlinkLength.Value;
            if (settings.BlinkOffset.HasValue)
                lightBlock.BlinkOffset = settings.BlinkOffset.Value;
            if (settings.Color != null)
                lightBlock.Color = settings.Color.ToColor();
            if (settings.Falloff.HasValue)
                lightBlock.Falloff = settings.Falloff.Value;
            if (settings.Intensity.HasValue)
                lightBlock.Intensity = settings.Intensity.Value;
            if (settings.Radius.HasValue)
                lightBlock.Radius = settings.Radius.Value;
        }
    }

    [APIEndpoint("POST", "/thrust")]
    public void SetThrust(DataTypes.ThrustBlock settings)
    {
        foreach (var thrustBlock in Blocks.OfType<IMyThrust>())
        {
            if (settings.Override.HasValue)
                thrustBlock.ThrustOverride = settings.Override.Value;
            if (settings.OverridePercentage.HasValue)
                thrustBlock.ThrustOverridePercentage = settings.OverridePercentage.Value;
        }
    }
    [APIEndpoint("DELETE", "/thrust")]
    public void StopThrust()
    {
        Blocks.OfType<IMyThrust>().ForEach(b => b.ThrustOverride = 0);
    }

    [APIEndpoint("POST", "/gyro")]
    public void SetGyro(DataTypes.GyroBlock settings)
    {
        foreach (var gyroBlock in Blocks.OfType<IMyGyro>())
        {
            if (settings.Override.HasValue)
                gyroBlock.GyroOverride = settings.Override.Value;
            if (settings.Power.HasValue)
                gyroBlock.GyroPower = settings.Power.Value;
            if (settings.Pitch.HasValue)
                gyroBlock.Pitch = settings.Pitch.Value;
            if (settings.Roll.HasValue)
                gyroBlock.Roll = settings.Roll.Value;
            if (settings.Yaw.HasValue)
                gyroBlock.Yaw = settings.Yaw.Value;
        }
    }
    [APIEndpoint("DELETE", "/gyro")]
    public void ResetGyro()
    {
        foreach (var gyroBlock in Blocks.OfType<IMyGyro>())
        {
            gyroBlock.GyroOverride = false;
            gyroBlock.GyroPower = 1.0f;
            gyroBlock.Pitch = 0;
            gyroBlock.Yaw = 0;
            gyroBlock.Roll = 0;
        }
    }

    [APIEndpoint("POST", "/programmable/recompile")]
    public void RecompileProgram()
    {
        Blocks.OfType<IMyProgrammableBlock>().ForEach(b => b.Recompile());
    }
    [APIEndpoint("POST", "/programmable/run")]
    public void RunProgram(string argument = null)
    {
        foreach (var programmableBlock in Blocks.OfType<IMyProgrammableBlock>())
        {
            if (string.IsNullOrEmpty(argument))
                programmableBlock.Run();
            else
                programmableBlock.Run(argument);
        }
    }
    [APIEndpoint("POST", "/programmable/script")]
    public void SetProgramScript(string text)
    {
        Blocks.OfType<IMyProgrammableBlock>().ForEach(b => b.ProgramData = text);
    }

}

}
