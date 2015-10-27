using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace TestBed
{
    class SystemConfig
    {
        //Internal config objects
        private DeviceConfig _deviceConfig;
        private LoggerConfig _loggerConfig;

        /// <summary>
        /// Creates a new SystemConfig object to hold all the configuration
        /// objects for the entire system
        /// </summary>
        /// <param name="inCOnfigFilePath">Path to the systems configuration file</param>
        public SystemConfig(string inConfigFilePath)
        {
            if (File.Exists(inConfigFilePath))
            {
                string fileText = File.ReadAllText(inConfigFilePath);
                JObject j = JObject.Parse(fileText);
                string deviceConfigJSON = j[JSONKeys.DEVICE_KEY].ToString();
                string loggerConfigJSON = j[JSONKeys.LOGGER_KEY].ToString();
                _deviceConfig = new DeviceConfig(deviceConfigJSON);
                _loggerConfig = new LoggerConfig(loggerConfigJSON);
            }
            else
            {
                Console.WriteLine("Can not open file");
            }
        }


        /// <summary>
        /// Returns the config object for the device
        /// </summary>
        public DeviceConfig getDeviceConfig() { return _deviceConfig; }

        /// <summary>
        /// Returns the config object for the logger
        /// </summary>
        public LoggerConfig getLoggerConfig() { return _loggerConfig; }
    
    }
}
