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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Internal config objects
        private DeviceConfig _deviceConfig;

        /// <summary>
        /// Creates a new SystemConfig object to hold all the configuration
        /// objects for the entire system
        /// </summary>
        /// <param name="inCOnfigFilePath">Path to the systems configuration file</param>
        public SystemConfig(string inConfigFilePath)
        {
            log.Debug(String.Format("Entering SystemConfig with config file: {0}", inConfigFilePath));
            if (File.Exists(inConfigFilePath))
            {
                log.Debug(String.Format("Loading config file: {0}", inConfigFilePath));
                string fileText = File.ReadAllText(inConfigFilePath);
                JObject j = JObject.Parse(fileText);
                string deviceConfigJSON = j[JSONKeys.DEVICE_KEY].ToString();
                log.Debug(String.Format("Retrieved device json string : {0}", deviceConfigJSON));
                _deviceConfig = new DeviceConfig(deviceConfigJSON);
            }
            else
            {
                log.Fatal(String.Format("Could not open config file {0}.  Throwing exception", inConfigFilePath));
                throw new IOException();
            }
        }


        /// <summary>
        /// Returns the config object for the device
        /// </summary>
        public DeviceConfig getDeviceConfig()
        {
            log.Debug(String.Format("Returning device configuration : {0}", _deviceConfig));
            return _deviceConfig;
        }
    }
}
