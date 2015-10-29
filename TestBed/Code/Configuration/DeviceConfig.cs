using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TestBed
{
    public class DeviceConfig
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Default port and baud rate values
        public static string _defaultPort = "COM3";
        public static int _defaultBaudRate = 9600;


        //The ports that the device is on
        private string _devicePort;

        //The baud rate the device is using
        private int _baudRate;

        /// <summary>
        /// Sets up the DeviceConfig object using the jason string input
        /// </summary>
        /// <param name="inJsonString">A json string that has all config data for the object</param>
        public DeviceConfig(string inJsonString)
        {
            log.Debug(String.Format("Creating DeviceConfig object with json string {0}", inJsonString));
            JObject j = JObject.Parse(inJsonString);
            _devicePort = (string)j[JSONKeys.DEVICE_PORT_KEY];
            if (_devicePort == null) _devicePort = _defaultPort;
            log.Debug(String.Format("Set device port to be {0}", _devicePort));

            try
            {
                _baudRate = (int)j[JSONKeys.DEVICE_BAUDRATE_KEY];
            }
            catch (ArgumentNullException)
            {
                _baudRate = _defaultBaudRate;
            }
            log.Debug(String.Format("Set device com to be {0}", _baudRate));
            log.Info(String.Format("Created DeviceConfig with port: {0} and baud rate: {1}", _devicePort, _baudRate));
        }


        /// <summary>
        /// Getter for the baud rate property
        /// </summary>
        /// <returns>The baud rate of the device</returns>
        public int getBaudRate()
        {
            log.Debug(String.Format("Returning DeviceConfig baud rate {0}", _baudRate));
            return _baudRate;
        }

        /// <summary>
        /// Getter for the device port property
        /// </summary>
        /// <returns>The device's com port</returns>
        public string getDevicePort()
        {
            log.Debug(String.Format("Returning DeviceConfig port {0}", _devicePort));
            return _devicePort;
        }
    }
}
