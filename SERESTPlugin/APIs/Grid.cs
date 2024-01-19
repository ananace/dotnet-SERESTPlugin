using SERESTPlugin.Attributes;
using SERESTPlugin.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using System.Reflection;
using Sandbox.Game.World;

namespace SERESTPlugin.APIs
{

public abstract class R0GridAPI : BaseAPI
{
    public abstract MyCubeGrid FindGrid();
    public MyCubeGrid Grid { get { return Data["grid"] as MyCubeGrid; } }

    public virtual bool CanCommunicate()
    {
        if (Sandbox.Game.World.MySession.Static.LocalPlayerId != 0)
            return PlayerCanCommunicate(Grid, Sandbox.Game.World.MySession.Static.LocalHumanPlayer);
        return true;
    }

    // TODO: Optional disable
    protected bool PlayerCanCommunicate(MyCubeGrid grid, MyPlayer ply)
    {
        if (grid.PlayerCanCommunicate(ply))
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
    [Hidden]
    public void TryModifyGrid()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/events", ClosesResponse = true)]
    public void GetEvents()
    {
        if (!Request.AcceptTypes.Any(type => type == "text/event-stream"))
        {
            // TODO: Return a snapshot?
            throw new HTTPException(System.Net.HttpStatusCode.NotAcceptable, "Need to accept text/event-stream");
        }
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
    [Hidden]
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
            return PlayerCanCommunicate(Block, Sandbox.Game.World.MySession.Static.LocalHumanPlayer);
        return true;
    }

    protected bool PlayerCanCommunicate(IMyTerminalBlock block, MyPlayer ply)
    {
        var grid = block.CubeGrid as MyCubeGrid;

        if (grid.PlayerCanCommunicate(ply))
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
    [Hidden]
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
        return Block.DetailedInfo;
    }
    [APIEndpoint("POST", "/info")]
    [APIEndpoint("PUT", "/info")]
    [APIEndpoint("DELETE", "/info")]
    [Hidden]
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
    [Hidden]
    public void UnsetName()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }

    [APIEndpoint("GET", "/properties")]
    public Dictionary<string, object> GetProperties()
    {
        Dictionary<string, object> properties = new Dictionary<string, object>();
        List<ITerminalProperty> registered = new List<ITerminalProperty>();
        Block.GetProperties(registered);

        foreach (var property in registered)
        {
            if (Block.TryGetProperty(property, out bool boolValue))
                properties[property.Id] = boolValue;
            else if (Block.TryGetProperty(property, out short shortValue))
                properties[property.Id] = shortValue;
            else if (Block.TryGetProperty(property, out int intValue))
                properties[property.Id] = intValue;
            else if (Block.TryGetProperty(property, out long longValue))
                properties[property.Id] = longValue;
            else if (Block.TryGetProperty(property, out long floatValue))
                properties[property.Id] = floatValue;
            else if (Block.TryGetProperty(property, out long doubleValue))
                properties[property.Id] = doubleValue;
            else if (Block.TryGetProperty(property, out string stringValue))
                properties[property.Id] = stringValue;
            else if (Block.TryGetProperty(property, out System.Text.StringBuilder stringBuilderValue))
                properties[property.Id] = stringBuilderValue.ToString();
            else if (Block.TryGetProperty(property, out VRageMath.Color colorValue))
                properties[property.Id] = new DataTypes.Color(colorValue);
        }

        return properties;
    }
    [APIEndpoint("POST", "/properties")]
    [APIEndpoint("PUT", "/properties")]
    [APIEndpoint("DELETE", "/properties")]
    [Hidden]
    public void ModifyProperties()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }
    [APIEndpoint("GET", "/properties/(?<property_name>[^/]+)")]
    public object GetProperty()
    {
        var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["property_name"]);
        var prop = Block.GetProperty(name);
        if (prop == null)
            throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No such property");

        if (prop.TryGetValue(Block, out bool boolValue))
            return boolValue;
        else if (prop.TryGetValue(Block, out short shortValue))
            return shortValue;
        else if (prop.TryGetValue(Block, out int intValue))
            return intValue;
        else if (prop.TryGetValue(Block, out long longValue))
            return longValue;
        else if (prop.TryGetValue(Block, out long floatValue))
            return floatValue;
        else if (prop.TryGetValue(Block, out long doubleValue))
            return doubleValue;
        else if (prop.TryGetValue(Block, out string stringValue))
            return stringValue;
        else if (prop.TryGetValue(Block, out System.Text.StringBuilder stringBuilderValue))
            return stringBuilderValue.ToString();
        else if (prop.TryGetValue(Block, out VRageMath.Color colorValue))
            return new DataTypes.Color(colorValue);

        throw new HTTPException(System.Net.HttpStatusCode.NotImplemented, $"Unable to handle property of type {prop.TypeName}");
    }
    [APIEndpoint("POST", "/properties/(?<property_name>[^/]+)", NeedsBody = true)]
    [APIEndpoint("PUT", "/properties/(?<property_name>[^/]+)", NeedsBody = true)]
    public void ModifyProperty()
    {
        var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["property_name"]);
        var prop = Block.GetProperty(name);
        if (prop == null)
            throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No such property");

        using (var reader = new System.IO.StreamReader(Request.InputStream))
        {
            var data = reader.ReadToEnd();

            if (prop.Is<bool>())
            {
                if (bool.TryParse(data, out bool parsedValue))
                {
                    prop.As<bool>().SetValue(Block, parsedValue);
                    return;
                }
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to parse as bool");
            }
            else if (prop.Is<short>())
            {
                if (short.TryParse(data, out short parsedValue))
                {
                    prop.As<short>().SetValue(Block, parsedValue);
                    return;
                }
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to parse as short");
            }
            else if (prop.Is<int>())
            {
                if (int.TryParse(data, out int parsedValue))
                {
                    prop.As<int>().SetValue(Block, parsedValue);
                    return;
                }
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to parse as int");
            }
            else if (prop.Is<long>())
            {
                if (long.TryParse(data, out long parsedValue))
                {
                    prop.As<long>().SetValue(Block, parsedValue);
                    return;
                }
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to parse as long");
            }
            else if (prop.Is<float>())
            {
                if (float.TryParse(data, out float parsedValue))
                {
                    prop.As<float>().SetValue(Block, parsedValue);
                    return;
                }
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to parse as float");
            }
            else if (prop.Is<double>())
            {
                if (double.TryParse(data, out double parsedValue))
                {
                    prop.As<double>().SetValue(Block, parsedValue);
                    return;
                }
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to parse as double");
            }
            else if (prop.Is<string>())
                prop.As<string>().SetValue(Block, data);
            else if (prop.Is<System.Text.StringBuilder>())
                prop.As<System.Text.StringBuilder>().SetValue(Block, new System.Text.StringBuilder(data));
            else if (prop.Is<VRageMath.Color>())
            {
                if (data.TryReadJSON(out DataTypes.Color parsedValue))
                {
                    prop.As<VRageMath.Color>().SetValue(Block, parsedValue.ToColor());
                    return;
                }
                throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to parse as color");
            }
        }

        throw new HTTPException(System.Net.HttpStatusCode.NotImplemented, $"Unable to handle property of type {prop.TypeName}");
    }
    [APIEndpoint("DELETE", "/properties")]
    public void ResetProperty()
    {
        var name = System.Web.HttpUtility.UrlDecode(EventArgs.Components["property_name"]);
        var prop = Block.GetProperty(name);
        if (prop == null)
            throw new HTTPException(System.Net.HttpStatusCode.NotFound, "No such property");

        if (prop.Is<bool>())
            prop.As<bool>().SetValue(Block, prop.As<bool>().GetDefaultValue(Block));
        else if (prop.Is<short>())
            prop.As<short>().SetValue(Block, prop.As<short>().GetDefaultValue(Block));
        else if (prop.Is<int>())
            prop.As<int>().SetValue(Block, prop.As<int>().GetDefaultValue(Block));
        else if (prop.Is<long>())
            prop.As<long>().SetValue(Block, prop.As<long>().GetDefaultValue(Block));
        else if (prop.Is<float>())
            prop.As<float>().SetValue(Block, prop.As<float>().GetDefaultValue(Block));
        else if (prop.Is<double>())
            prop.As<double>().SetValue(Block, prop.As<double>().GetDefaultValue(Block));
        else if (prop.Is<string>())
            prop.As<string>().SetValue(Block, prop.As<string>().GetDefaultValue(Block));
        else if (prop.Is<System.Text.StringBuilder>())
            prop.As<System.Text.StringBuilder>().SetValue(Block, prop.As<System.Text.StringBuilder>().GetDefaultValue(Block));
        else if (prop.Is<VRageMath.Color>())
            prop.As<VRageMath.Color>().SetValue(Block, prop.As<VRageMath.Color>().GetDefaultValue(Block));
        else
            throw new HTTPException(System.Net.HttpStatusCode.NotImplemented, $"Unable to handle property of type {prop.TypeName}");
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
    [Hidden]
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
    [Hidden]
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
    [Hidden]
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
    [Hidden]
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
    [Hidden]
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
    [Hidden]
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
    [Hidden]
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
    [Hidden]
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
    [Hidden]
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
        if (settings.Trading.HasValue && fatBlock != null)
            fatBlock.TradingEnabled.Value = settings.Trading.Value;
        if (settings.PowerOverride.HasValue && fatBlock != null)
            fatBlock.IsPowerTransferOverrideEnabled = settings.PowerOverride.Value;
    }
    [APIEndpoint("DELETE", "/connector")]
    [Hidden]
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

    [APIEndpoint("GET", "/conveyor")]
    public DataTypes.ConveyorBlock GetConveyor()
    {
        if (Block.GetProperty("UseConveyorSystem") == null)
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement conveyor");

        return new DataTypes.ConveyorBlock(Block);
    }
    [APIEndpoint("POST", "/conveyor")]
    [APIEndpoint("PUT", "/conveyor")]
    public void SetConveyor(DataTypes.ConveyorBlock settings)
    {
        var prop = Block.GetProperty("UseConveyorSystem");
        if (prop == null)
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement conveyor");

        if (settings.UseConveyorSystem.HasValue)
            prop.AsBool().SetValue(Block, settings.UseConveyorSystem.Value);
    }
    [APIEndpoint("DELETE", "/conveyor")]
    [Hidden]
    public void UnsetConveyor()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
    }


    [APIEndpoint("GET", "/functional")]
    public DataTypes.FunctionalBlock GetFunctional()
    {
        if (!(Block is IMyFunctionalBlock functionalBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement functional");

        return new DataTypes.FunctionalBlock(functionalBlock);
    }
    [APIEndpoint("POST", "/functional")]
    [APIEndpoint("PUT", "/functional")]
    public void SetFunctional(DataTypes.FunctionalBlock settings)
    {
        if (!(Block is IMyFunctionalBlock functionalBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement functional");

        if (settings.Enabled.HasValue)
            functionalBlock.Enabled = settings.Enabled.Value;
    }
    [APIEndpoint("DELETE", "/functional")]
    [Hidden]
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
        if (!(Block is IMyGyro gyroBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement gyro");

        gyroBlock.GyroOverride = false;
        gyroBlock.Pitch = 0;
        gyroBlock.Roll = 0;
        gyroBlock.Yaw = 0;
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

    [APIEndpoint("GET", "/terminal")]
    [Hidden]
    public DataTypes.TerminalBlock GetTerminal()
    {
        return new DataTypes.TerminalBlock(Block);
    }
    [APIEndpoint("POST", "/terminal")]
    [APIEndpoint("PUT", "/terminal")]
    public void SetTerminal(DataTypes.TerminalBlock settings)
    {
        if (settings.ShowInInventory.HasValue)
            Block.ShowInInventory = settings.ShowInInventory.Value;
        if (settings.ShowInTerminal.HasValue)
            Block.ShowInTerminal = settings.ShowInTerminal.Value;
        if (settings.ShowInToolbarConfig.HasValue)
            Block.ShowInToolbarConfig = settings.ShowInToolbarConfig.Value;
        if (settings.ShowOnHUD.HasValue)
            Block.ShowOnHUD = settings.ShowOnHUD.Value;
    }
    [APIEndpoint("DELETE", "/terminal")]
    [Hidden]
    public void UnsetTerminal()
    {
        throw new HTTPException(System.Net.HttpStatusCode.MethodNotAllowed);
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
    [APIEndpoint("POST", "/thrust", Obsolete = true)]
    [APIEndpoint("PUT", "/thrust",
      Description = "Change thrust override settings",
      Example = "With Newtons;\n\tcurl -X PUT {PATH} -d '{ \"override\" 100 }'\n\nWith percentage;\n\tcurl -X PUT {PATH} -d '{ \"override_perc\": 0.25 }'"
    )]
    public void SetThrust(DataTypes.ThrustBlockInput settings)
    {
        if (!(Block is IMyThrust thrustBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement thrust");

        if (settings.Override.HasValue)
            thrustBlock.ThrustOverride = settings.Override.Value;
        if (settings.OverridePercentage.HasValue)
            thrustBlock.ThrustOverridePercentage = settings.OverridePercentage.Value;
    }
    [APIEndpoint("DELETE", "/thrust", Description = "Remove any applied thrust override")]
    public void StopThrust()
    {
        if (!(Block is IMyThrust thrustBlock))
            throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Block does not implement thrust");

        thrustBlock.ThrustOverride = 0;
    }
}

public abstract class R0MultiBlockAPI : BaseAPI
{
    public abstract IEnumerable<IMyTerminalBlock> FindBlocks();
    public IEnumerable<IMyTerminalBlock> Blocks { get { return Data["blocks"] as IEnumerable<IMyTerminalBlock>; } }

    public virtual bool CanCommunicate()
    {
        if (Sandbox.Game.World.MySession.Static.LocalPlayerId != 0)
            return PlayerCanCommunicate(Blocks.First(), Sandbox.Game.World.MySession.Static.LocalHumanPlayer);
        return true;
    }

    protected bool PlayerCanCommunicate(IMyTerminalBlock block, MyPlayer ply)
    {
        var grid = block.CubeGrid as MyCubeGrid;

        if (grid.PlayerCanCommunicate(ply))
            return true;

        throw new HTTPException(System.Net.HttpStatusCode.BadRequest, "Unable to communicate with the target blocks");
    }

    [APIEndpoint("GET", "/")]
    public DataTypes.BlockInformation[] GetInformation()
    {
        return Blocks.Select(b => new DataTypes.BlockInformation(b)).ToArray();
    }
    [APIEndpoint("POST", "/")]
    [APIEndpoint("PUT", "/")]
    [APIEndpoint("DELETE", "/")]
    [Hidden]
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
