﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;

using MIG.Config;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MIG.Interfaces.RadioThermostat
{
    public static class Extensions
    {
        public static string GetDomain(this DeviceHolder holder)
        {
            string domain = holder.GetType().Namespace.ToString();
            domain = domain.Substring(domain.LastIndexOf(".") + 1) + "." + holder.GetType().Name.ToString();
            return domain;
        }

        public static DeviceOption GetOption(this DeviceHolder holder, string option)
        {
            if (holder.Options != null)
            {
                return holder.Options.Find(o => o.Name == option);
            }
            return null;
        }

        public static void SetOption(this DeviceHolder holder, string option, dynamic value)
        {
            MigService.Log.Trace("{0}: {1}={2}", holder.GetDomain(), option, value);
            var opt = holder.GetOption(option);
            if (opt == null)
            {
                opt = new DeviceOption(option, value);
                holder.Options.Add(opt);
            }
            opt.Value = value;
            //holder.OnSetOption(opt);
        }

        public static string GetSetPointSetting(ThermostatSetPoint mode)
        {
            string modeSetting;
            if (mode == ThermostatSetPoint.Cooling)
            {
                modeSetting = "t_cool";
            }
            else
            {
                modeSetting = "t_heat";
            }
            return modeSetting;
        }
    }

    public class DeviceOption
    {
        public string Name { get; set; }

        public dynamic Value { get; set; }

        public DeviceOption()
        {
        }

        public DeviceOption(string name, dynamic value)
        {
            Name = name;
            Value = value;
        }
    }

    public enum Commands
    {
        NotSet,

        Thermostat_ModeGet,
        Thermostat_ModeSet,
        Thermostat_SetPointGet,
        Thermostat_SetPointSet,
        Thermostat_FanModeGet,
        Thermostat_FanModeSet,
        Thermostat_FanStateGet,
        Thermostat_OperatingStateGet,

        SensorMultiLevel_Get,

    }

    public enum ThermostatMode
    {
        Off = 0,
        Heat = 1,
        Cool = 2,
        Auto = 3,
        AuxHeat = 4,
        Resume = 5,
        FanOnly = 6,
        Furnace = 7,
        DryAir = 8,
        MoistAir = 9,
        AutoChangeover = 10,
        HeatEconomy = 11,
        CoolEconomy = 12,
        Away = 13,
    }

    public enum ThermostatSetPoint
    {
        Unused = 0,
        Heating = 1,
        Cooling = 2,
        Unused03 = 3,
        Unused04 = 4,
        Unused05 = 5,
        Unused06 = 6,
        Furnace = 7,
        DryAir = 8,
        MoistAir = 9,
        AutoChangeover = 10,
        HeatingEconomy = 11,
        CoolingEconomy = 12,
        HeatingAway = 13,
    }

    public enum ThermostatFanMode
    {
        AutoLow = 0,
        OnLow = 1,
        AutoHigh = 2,
        OnHigh = 3,
        Unknown4 = 4,
        Unknown5 = 5,
        Circulate = 6,
    }

    public enum ThermostatFanState
    {
        Idle = 0,
        Running = 1,
        RunningHigh = 2,
        State03 = 3,
        State04 = 4,
        State05 = 5,
        State06 = 6,
        State07 = 7,
        State08 = 8,
        State09 = 9,
        State10 = 10,
        State11 = 11,
        State12 = 12,
        State13 = 13,
        State14 = 14,
        State15 = 15,
    }

    public enum ThermostatOperatingState
    {
        Idle = 0,
        Heating = 1,
        Cooling = 2,
        FanOnly = 3,
        PendingHeat = 4,
        PendingCool = 5,
        VentEconomizer = 6,
        State07 = 7,
        State08 = 8,
        State09 = 9,
        State10 = 10,
        State11 = 11,
        State12 = 12,
        State13 = 13,
        State14 = 14,
        State15 = 15,
    }


    public enum State
    {
        DISABLED,
        ENABLED,
    }

    public static class ModuleEvents
    {

        public static string VirtualMeter_Watts =
            "VirtualMeter.Watts";
        public static string Status_Level =
            "Status.Level";
        public static string Status_DoorLock =
            "Status.DoorLock";
        public static string Status_Battery =
            "Status.Battery";
        public static string Meter_KwHour =
            "Meter.KilowattHour";
        public static string Meter_KvaHour =
            "Meter.KilovoltAmpereHour";
        public static string Meter_Watts =
            "Meter.Watts";
        public static string Meter_Pulses =
            "Meter.Pulses";
        public static string Meter_AcVoltage =
            "Meter.AcVoltage";
        public static string Meter_AcCurrent =
            "Meter.AcCurrent";
        public static string Sensor_Power =
            "Sensor.Power";
        public static string Sensor_Generic =
            "Sensor.Generic";
        public static string Sensor_MotionDetect =
            "Sensor.MotionDetect";
        public static string Sensor_Temperature =
            "Sensor.Temperature";
        public static string Sensor_Luminance =
            "Sensor.Luminance";
        public static string Sensor_Humidity =
            "Sensor.Humidity";
        public static string Sensor_DoorWindow =
            "Sensor.DoorWindow";
        public static string Sensor_Key =
            "Sensor.Key";
        public static string Sensor_Alarm =
            "Sensor.Alarm";
        public static string Sensor_CarbonMonoxide =
            "Sensor.CarbonMonoxide";
        public static string Sensor_CarbonDioxide =
            "Sensor.CarbonDioxide";
        public static string Sensor_Smoke =
            "Sensor.Smoke";
        public static string Sensor_Heat =
            "Sensor.Heat";
        public static string Sensor_Flood =
            "Sensor.Flood";
        public static string Sensor_Tamper =
            "Sensor.Tamper";
        public static string Receiver_RawData =
            "Receiver.RawData";
        public static string Receiver_Status =
            "Receiver.Status";
        public static string Thermostat_FanMode =
            "Thermostat.FanMode";

    }


    public class DeviceHolder
    {
        NetHelper netHelper;

        public DeviceHolder(bool simulate)
        {
            netHelper = new NetHelper();
            Options = new List<DeviceOption>();
            IsSimulated = simulate;
        }

        public NetHelper Net
        {
            get
            {
                return netHelper;
            }
        }

        public bool IsSimulated { get; set; }
        public IPAddress Address { get; set; }
        public List<DeviceOption> Options { get; set; }

        public InterfaceRadioThermostat Parent { get; set; }

        public ResponseText Control(MigInterfaceCommand request)
        {
            var response = new ResponseText("OK"); //default success value
            bool raiseEvent = false;
            string eventParameter = ModuleEvents.Status_Level;
            string eventValue = "";
            string nodeId = request.Address;

            Commands command;
            Enum.TryParse<Commands>(request.Command.Replace(".", "_"), out command);

            switch (command)
            {
                case Commands.Thermostat_ModeGet:
                    response = new ResponseText(Extensions.GetOption(this, "tmode").Value.ToString());
                    break;
                case Commands.Thermostat_ModeSet:
                    {
                        ThermostatMode mode = (ThermostatMode)Enum.Parse(typeof(ThermostatMode), request.GetOption(0));
                        response = TStatPost(null, "tmode", mode);
                        raiseEvent = true;
                        eventParameter = "Thermostat.Mode";
                        eventValue = request.GetOption(0);
                        Extensions.SetOption(this, "tmode", mode);
                    }
                    break;
                case Commands.Thermostat_SetPointGet:
                    {
                        ThermostatSetPoint mode = (ThermostatSetPoint)Enum.Parse(typeof(ThermostatSetPoint), request.GetOption(0));
                        if (mode != ThermostatSetPoint.Heating && mode != ThermostatSetPoint.Cooling)
                        {
                            response = new ResponseText("Mode not supported: " + mode.ToString());
                        }
                        else
                        {
                            string modeSetting = Extensions.GetSetPointSetting(mode);
                            response = new ResponseText(Extensions.GetOption(this, modeSetting).Value.ToString());
                        }
                    }
                    break;
                case Commands.Thermostat_SetPointSet:
                    {
                        ThermostatSetPoint mode = (ThermostatSetPoint)Enum.Parse(typeof(ThermostatSetPoint), request.GetOption(0));
                        if (mode != ThermostatSetPoint.Heating && mode != ThermostatSetPoint.Cooling)
                        {
                            response = new ResponseText("Mode not supported: " + mode.ToString());
                        }
                        else
                        {
                            string modeSetting = Extensions.GetSetPointSetting(mode);
                            double temperature = double.Parse(request.GetOption(1).Replace(',', '.'), CultureInfo.InvariantCulture);
                            response = TStatPost(null, modeSetting, temperature);
                            raiseEvent = true;
                            eventParameter = "Thermostat.SetPoint." + request.GetOption(0);
                            eventValue = temperature.ToString(CultureInfo.InvariantCulture);
                            Extensions.SetOption(this, modeSetting, temperature);
                        }
                    }
                    break;
                case Commands.Thermostat_FanModeGet:
                    response = new ResponseText(Extensions.GetOption(this, "fmode").Value.ToString());
                    break;
                case Commands.Thermostat_FanModeSet:
                    {
                        ThermostatFanMode mode = (ThermostatFanMode)Enum.Parse(typeof(ThermostatFanMode), request.GetOption(0));
                        if (mode != ThermostatFanMode.OnHigh && mode != ThermostatFanMode.Circulate && mode != ThermostatFanMode.AutoHigh)
                        {
                            response = new ResponseText("Mode not supported: " + mode.ToString());
                        }
                        else
                        {
                            int modeSetting = ConvertFanModeFromHG(mode);
                            response = TStatPost(null, "fmode", modeSetting);
                            raiseEvent = true;
                            eventParameter = ModuleEvents.Thermostat_FanMode;
                            eventValue = mode.ToString();
                            Extensions.SetOption(this, "fmode", mode);
                        }
                    }
                    break;
                case Commands.Thermostat_FanStateGet:
                    response = new ResponseText(Extensions.GetOption(this, "fstate").Value.ToString());
                    break;
                case Commands.Thermostat_OperatingStateGet:
                    response = new ResponseText(Extensions.GetOption(this, "tstate").Value.ToString());
                    break;
                case Commands.SensorMultiLevel_Get:
                    response = new ResponseText(Extensions.GetOption(this, "temp").Value.ToString());
                    break;
                default:
                    response = new ResponseText("ERROR: Unknown command: " + request.Command);
                    break;

            }

            if (raiseEvent)
            {
                Parent.OnInterfacePropertyChanged(this.GetDomain(), nodeId, "Radio Thermostat Node", eventParameter, eventValue);
            }

            return response;
        }


        public void Initialize(IPAddress address, InterfaceRadioThermostat parent)
        {
            try
            {
                this.Address = address;
                this.Parent = parent;

                if (IsSimulated)
                {
                    Extensions.SetOption(this, "temp", 77.0);
                    // Parent.OnInterfacePropertyChanged(this.GetDomain(), nodeId, "Radio Thermostat Node", ModuleEvents.Sensor_Temperature, "77.0");
                    Extensions.SetOption(this, "tmode", ThermostatMode.Cool);
                    Extensions.SetOption(this, "tstate", ThermostatOperatingState.Cooling);
                    Extensions.SetOption(this, "fmode", ThermostatFanMode.AutoHigh);
                    Extensions.SetOption(this, "fstate", ThermostatFanState.RunningHigh);
                    Extensions.SetOption(this, "override", State.DISABLED);
                    Extensions.SetOption(this, "hold", State.DISABLED);
                    Extensions.SetOption(this, "t_heat", 0.0);
                    Extensions.SetOption(this, "t_cool", 76.0);
                    Extensions.SetOption(this, "ttarget", Extensions.GetOption(this, "tmode").Value);
                }
                else
                {
                    Update();
                    var tstat = TStatCall("model");
                    Extensions.SetOption(this, "model", tstat.model.ToString());
                    MigService.Log.Debug(String.Format("WiFI Thermostat found. Model: {0}", Extensions.GetOption(this, "model").Value));
                }
            }
            catch (Exception ex)
            {
                MigService.Log.Error(ex);
            }
        }

        #region Helper functions

        private static bool HasProperty(dynamic obj, string name)
        {
            Type objType = obj.GetType();

            if (objType == typeof(JObject))
            {
                return ((IDictionary<string, JToken>)obj).ContainsKey(name);
            }

            return objType.GetProperty(name) != null;
        }

        private static ThermostatFanMode ConvertFanModeToHG(int FanMode)
        {
            if (FanMode == 0)
                return ThermostatFanMode.AutoHigh;
            if (FanMode == 1)
                return ThermostatFanMode.Circulate;
            if (FanMode == 2)
                return ThermostatFanMode.OnHigh;
            return ThermostatFanMode.AutoHigh;
        }

        private static int ConvertFanModeFromHG(ThermostatFanMode FanMode)
        {
            if (FanMode == ThermostatFanMode.AutoHigh)
                return 0;
            if (FanMode == ThermostatFanMode.Circulate)
                return 1;
            if (FanMode == ThermostatFanMode.OnHigh)
                return 2;
            return 0;
        }

        private static ThermostatFanState ConvertFanStateToHG(int FanState)
        {
            if (FanState == 0)
                return ThermostatFanState.Idle;
            if (FanState == 1)
                return ThermostatFanState.RunningHigh;
            return ThermostatFanState.RunningHigh;
        }

        private static ThermostatOperatingState ConvertThermostatStateToHG(int TState)
        {
            if (TState == 0)
                return ThermostatOperatingState.Idle;
            if (TState == 1)
                return ThermostatOperatingState.Heating;
            if (TState == 2)
                return ThermostatOperatingState.Cooling;
            return ThermostatOperatingState.Idle;
        }

        private void Update()
        {
            if (IsSimulated)
                return;

            string webservicebaseurl = "http://" + Address + "/tstat";
            var tstat = TStatCall();

            Extensions.SetOption(this, "temp", double.Parse(tstat.temp.ToString()));
            Extensions.SetOption(this, "tmode", (ThermostatMode)int.Parse(tstat.tmode.ToString()));
            Extensions.SetOption(this, "tstate", ConvertThermostatStateToHG(int.Parse(tstat.tstate.ToString())));
            Extensions.SetOption(this, "fmode", ConvertFanModeToHG(int.Parse(tstat.fmode.ToString())));
            Extensions.SetOption(this, "fstate", ConvertFanStateToHG(int.Parse(tstat.fstate.ToString())));
            Extensions.SetOption(this, "override", (State)int.Parse(tstat.fmode.ToString()));
            Extensions.SetOption(this, "hold", (State)int.Parse(tstat.fmode.ToString()));

            if (HasProperty(tstat, "t_heat"))
            {
                Extensions.SetOption(this, "t_heat", double.Parse(tstat.t_heat.ToString()));
            }
            else
            {
                Extensions.SetOption(this, "t_heat", 0.0);
            }
            if (HasProperty(tstat, "t_cool"))
            {
                Extensions.SetOption(this, "t_cool", double.Parse(tstat.t_cool.ToString()));
            }
            else
            {
                Extensions.SetOption(this, "t_cool", 0.0);
            }
            if (HasProperty(tstat, "ttarget"))
            {
                Extensions.SetOption(this, "ttarget", (ThermostatMode)int.Parse(tstat.ttarget.ToString()));
            }
            else
            {
                Extensions.SetOption(this, "ttarget", Extensions.GetOption(this, "tmode").Value);
            }
        }

        private string GetJsonString(string item, dynamic value)
        {
            return (new JObject(new JProperty(item, value))).ToString();
        }

        // Don't call if simulated
        private dynamic TStatCall(string Resource = null)
        {
            string webservicebaseurl = "http://" + Address + "/tstat";
            if (Resource != null)
            {
                webservicebaseurl += "/" + Resource;
            }
            var tstat = Net.WebService(webservicebaseurl).GetData();
            return tstat;
        }

        private ResponseText TStatPost(string Resource, string Item, dynamic Value)
        {
            string webservicebaseurl = "http://" + Address + "/tstat";
            ResponseText response = new ResponseText("OK");
            if (Resource != null)
            {
                webservicebaseurl += "/" + Resource;
            }
            if (!IsSimulated)
            {
                response = new ResponseText(Net.WebService(webservicebaseurl).Post(GetJsonString(Resource, Value)).Call());
            }
            return response;
        }

        #endregion
    }


}