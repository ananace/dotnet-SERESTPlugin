using SERESTPlugin.Attributes;
using SERESTPlugin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System.Reflection;

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
    [APIEndpoint("POST", "/")]
    [APIEndpoint("PUT", "/")]
    [APIEndpoint("DELETE", "/")]
    public void TryModifyGrid()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/dampeners")]
    public bool GetDampeners()
    {
        return Grid.DampenersEnabled;
    }
    [APIEndpoint("POST", "/dampeners")]
    [APIEndpoint("PUT", "/dampeners")]
    public void SetDampeners(bool? wanted = true)
    {
        Grid.EntityThrustComponent.DampenersEnabled = wanted ?? true;
    }
    [APIEndpoint("DELETE", "/dampeners")]
    public void UnsetDampeners()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
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
    [APIEndpoint("POST", "/")]
    [APIEndpoint("PUT", "/")]
    [APIEndpoint("DELETE", "/")]
    public void TryModifyBlock()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/data")]
    public string GetData()
    {
        return Block.CustomData;
    }
    [APIEndpoint("POST", "/data")]
    [APIEndpoint("PUT", "/data")]
    public void SetData(string data)
    {
        Block.CustomData = data;
    }
    [APIEndpoint("DELETE", "/data")]
    public void RemoveData()
    {
        Block.CustomData = null;
    }

    [APIEndpoint("GET", "/info")]
    public string GetInfo()
    {
        return Block.CustomInfo;
    }
    [APIEndpoint("POST", "/info")]
    [APIEndpoint("PUT", "/info")]
    [APIEndpoint("DELETE", "/info")]
    public void ModifyInfo()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/name")]
    public string GetName()
    {
        return Block.CustomName;
    }
    [APIEndpoint("POST", "/name")]
    [APIEndpoint("PUT", "/name")]
    public void SetName(string name)
    {
        Block.CustomName = name;
    }
    [APIEndpoint("DELETE", "/name")]
    public void UnsetName()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }


    [APIEndpoint("GET", "/air_vent")]
    public DataTypes.AirVentBlock GetAirVent()
    {
        if (!(Block is IMyAirVent airVentBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement air_vent");

        return new DataTypes.AirVentBlock(airVentBlock);
    }
    [APIEndpoint("POST", "/air_vent")]
    [APIEndpoint("PUT", "/air_vent")]
    public void SetAirVent(DataTypes.AirVentBlockInput settings)
    {
        if (!(Block is IMyAirVent airVentBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement air_vent");

        if (settings.Depressurize.HasValue)
            airVentBlock.Depressurize = settings.Depressurize.Value;
    }
    [APIEndpoint("DELETE", "/air_vent")]
    public void UnsetAirVent()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/assembler")]
    public DataTypes.AssemblerBlock GetAssembler()
    {
        if (!(Block is IMyAssembler assemblerBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement assembler");

        return new DataTypes.AssemblerBlock(assemblerBlock);
    }
    [APIEndpoint("POST", "/assembler")]
    [APIEndpoint("PUT", "/assembler")]
    public void SetAssembler(DataTypes.AssemblerBlockInput settings)
    {
        if (!(Block is IMyAssembler assemblerBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement assembler");

        if (!string.IsNullOrEmpty(settings.Mode))
        {
            if (!Enum.TryParse(settings.Mode, true, out Sandbox.ModAPI.Ingame.MyAssemblerMode mode))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Not a valid assembler mode");

            assemblerBlock.Mode = mode;
        }
        if (settings.Cooperative.HasValue)
            assemblerBlock.CooperativeMode = settings.Cooperative.Value;
        if (settings.Repeating.HasValue)
            assemblerBlock.Repeating = settings.Repeating.Value;
    }
    [APIEndpoint("DELETE", "/assembler")]
    public void UnsetAssembler()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/attachable_top")]
    public DataTypes.AttachableTopBlock GetAttachableTop()
    {
        if (!(Block is IMyAttachableTopBlock attachableTopBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement attachable_top");

        return new DataTypes.AttachableTopBlock(attachableTopBlock);
    }
    [APIEndpoint("POST", "/attachable_top")]
    [APIEndpoint("PUT", "/attachable_top")]
    [APIEndpoint("DELETE", "/attachable_top")]
    public void ModifyAttachableTop()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/battery")]
    public DataTypes.BatteryBlock GetBattery()
    {
        if (!(Block is IMyBatteryBlock batteryBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement battery");

        return new DataTypes.BatteryBlock(batteryBlock);
    }
    [APIEndpoint("POST", "/battery")]
    [APIEndpoint("PUT", "/battery")]
    public void SetBattery(DataTypes.BatteryBlockInput settings)
    {
        if (!(Block is IMyBatteryBlock batteryBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement battery");

        if (!string.IsNullOrEmpty(settings.ChargeMode))
        {
            if (!Enum.TryParse(settings.ChargeMode, true, out Sandbox.ModAPI.Ingame.ChargeMode mode))
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement battery");

            batteryBlock.ChargeMode = mode;
        }
    }
    [APIEndpoint("DELETE", "/battery")]
    public void UnsetBattery()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/beacon")]
    public DataTypes.BeaconBlock GetBeacon()
    {
        if (!(Block is IMyBeacon beaconBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement beacon");

        return new DataTypes.BeaconBlock(beaconBlock);
    }
    [APIEndpoint("POST", "/beacon")]
    [APIEndpoint("PUT", "/beacon")]
    public void SetBeacon(DataTypes.BeaconBlock settings)
    {
        if (!(Block is IMyBeacon beaconBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement beacon");

        if (!string.IsNullOrEmpty(settings.HudText))
            beaconBlock.HudText = settings.HudText;
        if (settings.Radius.HasValue)
            beaconBlock.Radius = settings.Radius.Value;
    }
    [APIEndpoint("DELETE", "/beacon")]
    public void UnsetBeacon()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/button")]
    public DataTypes.ButtonBlock GetButton()
    {
        if (!(Block is IMyButtonPanel buttonBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement button");

        return new DataTypes.ButtonBlock(buttonBlock);
    }
    [APIEndpoint("POST", "/button")]
    [APIEndpoint("PUT", "/button")]
    public void SetButton(DataTypes.ButtonBlockInput settings)
    {
        if (!(Block is IMyButtonPanel buttonBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement button");
        
        if (settings.AnyoneCanUse.HasValue)
            buttonBlock.AnyoneCanUse = settings.AnyoneCanUse.Value;
    }
    [APIEndpoint("DELETE", "/button")]
    public void UnsetButton()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }
    [APIEndpoint("POST", "/button/(?<button_id>[0-9]+)")]
    public void PushButton()
    {
        if (!int.TryParse(EventArgs.Components["button_id"], out int buttonId))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Invalid button id specified");

        if (!(Block is IMyButtonPanel))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement button");

        if (!(Block is SpaceEngineers.Game.Entities.Blocks.MyButtonPanel fatButtonBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to handle the button block for pressing");

        fatButtonBlock.PressButton(buttonId);
    }

    [APIEndpoint("GET", "/camera")]
    public DataTypes.CameraBlock GetCamera()
    {
        if (!(Block is IMyCameraBlock cameraBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement camera");

        return new DataTypes.CameraBlock(cameraBlock);
    }
    [APIEndpoint("POST", "/camera")]
    [APIEndpoint("PUT", "/camera")]
    [APIEndpoint("DELETE", "/camera")]
    public void ModifyCamera()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/cargo")]
    public DataTypes.CargoBlock GetCargo()
    {
        if (!(Block is MyCubeBlock fatBlock) || !fatBlock.HasInventory)
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement cargo");


        return new DataTypes.CargoBlock(fatBlock);
    }
    [APIEndpoint("POST", "/cargo")]
    [APIEndpoint("PUT", "/cargo")]
    [APIEndpoint("DELETE", "/cargo")]
    public void ModifyCargo()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/cockpit")]
    public DataTypes.CockpitBlock GetCockpit()
    {
        if (!(Block is IMyCockpit cockpitBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement cockpit");

        return new DataTypes.CockpitBlock(cockpitBlock);
    }
    [APIEndpoint("POST", "/cockpit")]
    [APIEndpoint("PUT", "/cockpit")]
    [APIEndpoint("DELETE", "/cockpit")]
    public void ModifyCockpit()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/connector")]
    public DataTypes.ConnectorBlock GetConnector()
    {
        if (!(Block is IMyShipConnector connectorBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement connector");

        return new DataTypes.ConnectorBlock(connectorBlock);
    }
    [APIEndpoint("POST", "/connector")]
    [APIEndpoint("PUT", "/connector")]
    public void SetConnector(DataTypes.ConnectorBlockInput settings)
    {
        if (!(Block is IMyShipConnector connectorBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement connector");
        var fatBlock = connectorBlock as Sandbox.Game.Entities.Cube.MyShipConnector;

        if (settings.CollectAll.HasValue)
            connectorBlock.CollectAll = settings.CollectAll.Value;
        if (settings.IsParkingEnabled.HasValue)
            connectorBlock.IsParkingEnabled = settings.IsParkingEnabled.Value;
        if (settings.PullStrength.HasValue)
            connectorBlock.PullStrength = settings.PullStrength.Value;
        if (settings.ThrowOut.HasValue)
            connectorBlock.ThrowOut = settings.ThrowOut.Value;
        if (settings.Trading.HasValue)
            fatBlock.TradingEnabled_RequestChange(settings.Trading.Value);
        if (settings.PowerOverride.HasValue)
            fatBlock.IsPowerTransferOverrideEnabled = settings.PowerOverride.Value;
    }
    [APIEndpoint("DELETE", "/connector")]
    public void UnsetConnector()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }
    [APIEndpoint("GET", "/connector/connection")]
    public string GetConnectorConnection()
    {
        if (!(Block is IMyShipConnector connectorBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement connector");

        return connectorBlock.Status.ToString().ToLower();
    }
    [APIEndpoint("POST", "/connector/connection")]
    [APIEndpoint("PUT", "/connector/connection")]
    public void AttachConnectorConnection()
    {
        if (!(Block is IMyShipConnector connectorBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement connector");

        connectorBlock.Connect();
    }
    [APIEndpoint("DELETE", "/connector/connection")]
    public void DetachConnectorConnection()
    {
        if (!(Block is IMyShipConnector connectorBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement connector");

        connectorBlock.Disconnect();
    }

    [APIEndpoint("GET", "/functional")]
    public bool GetFunctional()
    {
        if (!(Block is IMyFunctionalBlock functionalBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement functional");
        return functionalBlock.Enabled;
    }
    [APIEndpoint("POST", "/functional")]
    [APIEndpoint("PUT", "/functional")]
    public void SetFunctional(bool wanted = true)
    {
        if (!(Block is IMyFunctionalBlock functionalBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement functional");

        functionalBlock.Enabled = wanted;
    }
    [APIEndpoint("DELETE", "/functional")]
    public void UnsetFunctional()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
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
    public void UnsetGyro()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
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

    [APIEndpoint("GET", "/text")]
    public string GetText()
    {
        if (!(Block is IMyTextSurface textBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement text");

        return textBlock.GetText();
    }
    [APIEndpoint("POST", "/text")]
    [APIEndpoint("PUT", "/text")]
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

    [APIEndpoint("GET", "/thrust")]
    public DataTypes.ThrustBlock GetThrust()
    {
        if (!(Block is IMyThrust thrustBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement thrust");

        return new DataTypes.ThrustBlock(thrustBlock);
    }
    [APIEndpoint("POST", "/thrust")]
    [APIEndpoint("PUT", "/thrust")]
    public void SetThrust(DataTypes.ThrustBlockInput settings)
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
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
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
    public DataTypes.BlockInformation[] GetInformation()
    {
        return Blocks.Select(b => new DataTypes.BlockInformation(b)).ToArray();
    }
    [APIEndpoint("POST", "/")]
    [APIEndpoint("PUT", "/")]
    [APIEndpoint("DELETE", "/")]
    public void TryModifyBlocks()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
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
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
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
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
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
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
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
