/*
    This file is part of MIG Project source code.

    MIG is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MIG is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MIG.  If not, see <http://www.gnu.org/licenses/>.  
*/

/*
*     Author: Generoso Martello <gene@homegenie.it>
*     Project Homepage: https://github.com/genielabs/mig-service-dotnet
*/

using System;
using System.IO;
using System.Xml.Serialization;
using System.Threading;

using MIG;
using MIG.Config;

using MIG.Interfaces.HomeAutomation;

namespace TestProject
{
    class MainClass
    {
        public static void CheckValue(ResponseText response, string ExpectedValue)
        {
            string ActualValue = response.ResponseValue;
            if (ActualValue != ExpectedValue)
            {
                MigService.Log.Error(String.Format("ERROR: Actual != Expected: {0} != {1}", ActualValue, ExpectedValue));
            }
            else
            {
                //MigService.Log.Info(String.Format("Match: {0} == {1}", ActualValue, ExpectedValue));
            }
        }
        public static void Main (string[] args)
        {
            Console.WriteLine("Mig Interface Skelton test APP");
            RadioThermostat.SetSimulation(true);

            var migService = new MigService();

            // Load the configuration from systemconfig.xml file
            MigServiceConfiguration configuration;
            // Construct an instance of the XmlSerializer with the type
            // of object that is being deserialized.
            XmlSerializer mySerializer = new XmlSerializer(typeof(MigServiceConfiguration));
            // To read the file, create a FileStream.
            FileStream myFileStream = new FileStream("systemconfig.xml", FileMode.Open);
            // Call the Deserialize method and cast to the object type.
            configuration = (MigServiceConfiguration)mySerializer.Deserialize(myFileStream);

            // Set the configuration and start MIG Service
            migService.Configuration = configuration;
            migService.StartService();

            Thread.Sleep(10000);
            // Get a reference to the test interface
            var interfaceDomain = "HomeAutomation.RadioThermostat";
            var migInterface = migService.GetInterface(interfaceDomain);
            string modAddress = "256";
            // Test an interface API command programmatically <module_domain>/<module_address>/<command>[/<option_0>[/../<option_n>]]
            var response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.ModeGet"));
            CheckValue((ResponseText)response, "Cool");

            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.ModeSet/Heat"));
            CheckValue((ResponseText)response, "OK");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.ModeGet"));
            CheckValue((ResponseText)response, "Heat");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.SetPointGet/Heating"));
            CheckValue((ResponseText)response, "18.33");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.SetPointGet/Cooling"));
            CheckValue((ResponseText)response, "24.44");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.SetPointGet/Furnace"));
            CheckValue((ResponseText)response, "Mode not supported: Furnace");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.SetPointSet/Cooling/24.0"));
            CheckValue((ResponseText)response, "Setting point type must match current mode. t_cool != Heat");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.SetPointGet/Heating"));
            CheckValue((ResponseText)response, "18.33");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.SetPointGet/Cooling"));
            CheckValue((ResponseText)response, "24.44");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.FanModeGet"));
            CheckValue((ResponseText)response, "AutoHigh");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.FanModeSet/OnLow"));
            CheckValue((ResponseText)response, "Mode not supported: OnLow");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.FanModeSet/Circulate"));
            CheckValue((ResponseText)response, "OK");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.FanModeGet"));
            CheckValue((ResponseText)response, "Circulate");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.FanStateGet"));
            CheckValue((ResponseText)response, "RunningHigh");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Thermostat.OperatingStateGet"));
            CheckValue((ResponseText)response, "Cooling");
            response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/SensorMultiLevel.Get"));
            CheckValue((ResponseText)response, "25");
            // <module_domain> ::= "Example.InterfaceSkelton"
            // <module_address> ::= "3"
            // <command> ::= "Greet.Hello"
            // <option_0> ::= "Username"
            // For more infos about MIG API see:
            //    http://genielabs.github.io/HomeGenie/api/mig/overview.html
            //    http://genielabs.github.io/HomeGenie/api/mig/mig_api_interfaces.html

            // The same command can be invoked though the WebGateway 
            // http://<server_address>:8080/api/Example.InterfaceSkelton/1/Greet.Hello/Username

            // Test some other interface API command
            //response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Control.On"));
            //MigService.Log.Debug(((ResponseText)response).ResponseValue);
            //response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Control.Off"));
            //MigService.Log.Debug(((ResponseText)response).ResponseValue);
            //response = migInterface.InterfaceControl(new MigInterfaceCommand(interfaceDomain + "/" + modAddress + "/Temperature.Get"));
            //MigService.Log.Debug(((ResponseText)response).ResponseValue);
            
            Console.WriteLine("\n[Press Enter to Quit]\n");
            Console.ReadLine();

            migService.StopService();
        }
    }
}
