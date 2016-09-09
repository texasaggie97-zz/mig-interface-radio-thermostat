using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Globalization;

using MIG.Config;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MIG.Interfaces.HomeAutomation
{
    public static class Extensions
    {
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

    public class DeviceHolder
    {
        NetHelper netHelper;

        public DeviceHolder(bool simulate, string moduleAddress)
        {
            netHelper = new NetHelper();
            Options = new List<DeviceOption>();
            IsSimulated = simulate;
            ModuleAddress = moduleAddress;
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

        public RadioThermostat Parent { get; set; }

        public string ModuleAddress { get; set; }

        public ResponseText Control(MigInterfaceCommand request)
        {
            var response = new ResponseText("OK"); //default success value
            string eventParameter = ModuleEvents.Status_Level;
            string eventValue = "";

            Commands command;
            Enum.TryParse<Commands>(request.Command.Replace(".", "_"), out command);

            switch (command)
            {
                case Commands.Thermostat_ModeGet:
                    response = new ResponseText(GetOption("tmode").Value.ToString());
                    break;
                case Commands.Thermostat_ModeSet:
                    {
                        ThermostatMode mode = (ThermostatMode)Enum.Parse(typeof(ThermostatMode), request.GetOption(0));
                        response = TStatPost(null, "tmode", mode);
                        SetOption("tmode", mode, ModuleEvents.Thermostat_Mode);
                        Update();
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
                            response = new ResponseText(GetOption(modeSetting).Value.ToString());
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
                            response = TStatPost(null, modeSetting, convertCtoF(temperature));
                            eventParameter = ModuleEvents.Thermostat_SetPoint + request.GetOption(0);
                            eventValue = temperature.ToString(CultureInfo.InvariantCulture);
                            SetOption(modeSetting, temperature, eventParameter, eventValue);
                            Update();
                        }
                    }
                    break;
                case Commands.Thermostat_FanModeGet:
                    response = new ResponseText(GetOption("fmode").Value.ToString());
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
                            eventParameter = ModuleEvents.Thermostat_FanMode;
                            SetOption("fmode", mode, eventParameter);
                            Update();
                        }
                    }
                    break;
                case Commands.Thermostat_FanStateGet:
                    response = new ResponseText(GetOption("fstate").Value.ToString());
                    break;
                case Commands.Thermostat_OperatingStateGet:
                    response = new ResponseText(GetOption("tstate").Value.ToString());
                    break;
                case Commands.SensorMultiLevel_Get:
                    response = new ResponseText(GetOption("temp").Value.ToString());
                    break;
                default:
                    response = new ResponseText("ERROR: Unknown command: " + request.Command);
                    break;

            }

            return response;
        }


        public void Connect(IPAddress address, RadioThermostat parent)
        {
            try
            {
                this.Address = address;
                this.Parent = parent;
                this.IsActive = true;

                if (IsSimulated)
                {
                    // We acquire the lock and then call the NoLock version for performance
                    lock (OptionsLock)
                    {
                        double temperature = convertFtoC(77.0);
                        SetOptionNoLock("temp", temperature, ModuleEvents.Sensor_Temperature, temperature.ToString(CultureInfo.InvariantCulture));
                        SetOptionNoLock("tmode", ThermostatMode.Cool, ModuleEvents.Thermostat_Mode);
                        SetOptionNoLock("tstate", ThermostatOperatingState.Cooling, ModuleEvents.Thermostat_OperatingState);
                        SetOptionNoLock("fmode", ThermostatFanMode.AutoHigh, ModuleEvents.Thermostat_FanMode);
                        SetOptionNoLock("fstate", ThermostatFanState.RunningHigh, ModuleEvents.Thermostat_FanState);
                        SetOptionNoLock("override", State.DISABLED, null);
                        SetOptionNoLock("hold", State.DISABLED, null);
                        temperature = convertFtoC(65.0);
                        SetOptionNoLock("t_heat", temperature, ModuleEvents.Thermostat_SetPoint + "Heating", temperature.ToString(CultureInfo.InvariantCulture));
                        temperature = convertFtoC(76.0);
                        SetOptionNoLock("t_cool", temperature, ModuleEvents.Thermostat_SetPoint + "Cooling", temperature.ToString(CultureInfo.InvariantCulture));
                        SetOptionNoLock("ttarget", GetOption("tmode").Value, null);
                    }
                }
                else
                {
                    var tstat = TStatCall("model");
                    SetOption("model", tstat.model.ToString(), null);
                    MigService.Log.Debug(String.Format("WiFI Thermostat found. Model: {0}", GetOption("model").Value));
                    BackgroundUpdate();
                }
            }
            catch (Exception ex)
            {
                MigService.Log.Error(ex);
            }
        }

        public void Disconnect()
        {
            this.IsActive = false;
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

        private bool IsActive { get; set; }

        private Object OptionsLock = new Object();
        private Object TstatLock = new Object();

        private void BackgroundUpdate()
        {
            Utility.RunAsyncTask(() =>
                {
                    while (IsActive)
                    {
                        Update();
                        Thread.Sleep(10000);
                    }
                });
        }

        private void Update()
        {
            if (IsSimulated)
                return;

            // Wait 1 second before querying the thermostat
            string webservicebaseurl = "http://" + Address + "/tstat";
            var tstat = TStatCall();

            // We acquire the lock and then call the NoLock version for performance
            lock (OptionsLock)
            {
                double temperature = convertFtoC(double.Parse(tstat.temp.ToString()));
                SetOptionNoLock("temp", temperature, ModuleEvents.Sensor_Temperature, temperature.ToString(CultureInfo.InvariantCulture));
                SetOptionNoLock("tmode", (ThermostatMode)int.Parse(tstat.tmode.ToString()), ModuleEvents.Thermostat_Mode);
                SetOptionNoLock("tstate", ConvertThermostatStateToHG(int.Parse(tstat.tstate.ToString())), ModuleEvents.Thermostat_OperatingState);
                SetOptionNoLock("fmode", ConvertFanModeToHG(int.Parse(tstat.fmode.ToString())), ModuleEvents.Thermostat_FanMode);
                SetOptionNoLock("fstate", ConvertFanStateToHG(int.Parse(tstat.fstate.ToString())), ModuleEvents.Thermostat_FanState);
                SetOptionNoLock("override", (State)int.Parse(tstat.fmode.ToString()), null);
                SetOptionNoLock("hold", (State)int.Parse(tstat.fmode.ToString()), null);

                if (HasProperty(tstat, "t_heat"))
                {
                    temperature = convertFtoC(double.Parse(tstat.t_heat.ToString()));
                }
                else
                {
                    temperature = convertFtoC(65.0);
                }
                SetOptionNoLock("t_heat", temperature, ModuleEvents.Thermostat_SetPoint + "Heating", temperature.ToString(CultureInfo.InvariantCulture));

                if (HasProperty(tstat, "t_cool"))
                {
                    temperature = convertFtoC(double.Parse(tstat.t_cool.ToString()));
                }
                else
                {
                    temperature =convertFtoC(85.0);
                }
                SetOptionNoLock("t_cool", temperature, ModuleEvents.Thermostat_SetPoint + "Cooling", temperature.ToString(CultureInfo.InvariantCulture));

                if (HasProperty(tstat, "ttarget"))
                {
                    SetOptionNoLock("ttarget", (ThermostatMode)int.Parse(tstat.ttarget.ToString()), null);
                }
                else
                {
                    SetOptionNoLock("ttarget", GetOption("tmode").Value, null);
                }
            }
        }

        private string GetJsonString(string item, dynamic value)
        {
            return (new JObject(new JProperty(item, value))).ToString();
        }

        // Don't call if simulated
        private dynamic TStatCall(string Resource = null)
        {
            //lock (TstatLock)
            {
                string webservicebaseurl = "http://" + Address + "/tstat";
                if (Resource != null)
                {
                    webservicebaseurl += "/" + Resource;
                }
                var tstat = Net.WebService(webservicebaseurl).GetData();
                return tstat;
            }
        }

        private ResponseText TStatPost(string Resource, string Item, dynamic Value)
        {
            //lock (TstatLock)
            {
                string webservicebaseurl = "http://" + Address + "/tstat";
                ResponseText response = new ResponseText("OK");
                if (Resource != null)
                {
                    webservicebaseurl += "/" + Resource;
                }
                if (!IsSimulated)
                {
                    response = new ResponseText(Net.WebService(webservicebaseurl).Post(GetJsonString(Item, Value)).Call());
                    // We are going to wait a bit less that 1 second to allow the thermostat to respond to
                    // any new values. We do this wait in the lock section so that we prevent another thread
                    // from querying the thermostat before it has had a chance to make any required adjustments.
                    //Thread.Sleep(800);
                }
                return response;
            }
        }

        public string GetDomain()
        {
            string domain = Parent.GetType().Namespace.ToString();
            domain = domain.Substring(domain.LastIndexOf(".") + 1) + "." + Parent.GetType().Name.ToString();
            return domain;
        }

        public DeviceOption GetOption(string option)
        {
            //lock(OptionsLock)
            {
                return GetOptionNoLock(option);
            }
        }

        public void SetOption(string option, dynamic value, string eventParameter, string eventValue = null)
        { 
            //lock(OptionsLock)
            {
                SetOptionNoLock(option, value, eventParameter, eventValue);
            }
        }

        // NoLock versions expect lock to be acquired before calling
        private DeviceOption GetOptionNoLock(string option)
        {
            if (Options != null)
            {
                return Options.Find(o => o.Name == option);
            }
            return null;
        }

        private void SetOptionNoLock(string option, dynamic value, string eventParameter, string eventValue = null)
        {
            var opt = GetOptionNoLock(option);
            bool raiseEvent = false;
            if (opt == null)
            {
                opt = new DeviceOption(option, value);
                Options.Add(opt);
                raiseEvent = true;
                MigService.Log.Trace("{0}: {1}={2}", GetDomain(), option, value);
            }
            if (opt.Value != value)
            {
                raiseEvent = true;
                MigService.Log.Trace("{0}: {1}={2}", GetDomain(), option, value);
            }
            opt.Value = value;
            if (raiseEvent && eventParameter != null)
            {
                string e;
                if (eventValue == null)
                {
                    e = value.ToString();
                }
                else
                {
                    e = eventValue;
                }
                Parent.OnInterfacePropertyChanged(GetDomain(), ModuleAddress, "Radio Thermostat Node", eventParameter, e);
            }
            //holder.OnSetOption(opt);
        }

        private double convertFtoC(double F)
        {
            return Math.Round((F - 32) / 1.8, 1);
        }

        private double convertCtoF(double C)
        {
            return Math.Round(C * 1.8 + 32, 1);
        }

        #endregion
    }


}
