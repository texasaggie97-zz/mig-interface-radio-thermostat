using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIG.Interfaces.HomeAutomation
{
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
}
