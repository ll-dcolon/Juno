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
            //!@#NEED TO DO ERROR HANDLING
            JObject j = JObject.Parse(inJsonString);
            _devicePort = (string)j[JSONKeys.DEVICE_PORT_KEY];
            _baudRate = (int)j[JSONKeys.DEVICE_BAUDRATE_KEY];
        }


        /// <summary>
        /// Getter for the baud rate property
        /// </summary>
        /// <returns>The baud rate of the device</returns>
        public int getBaudRate() { return _baudRate; }

        /// <summary>
        /// Getter for the device port property
        /// </summary>
        /// <returns>The device's com port</returns>
        public string getDevicePort() { return _devicePort; }
    }
}
