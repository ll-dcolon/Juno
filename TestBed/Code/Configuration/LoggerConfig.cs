using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TestBed
{
    public class LoggerConfig
    {
        //The location where the logger should put the log file
        private string _logLocation;

        //The log level we should start in
        private string _logLevel;

        //The name used to start all log files
        private string _logNameHeader;


        /// <summary>
        /// Creates a LoggerConfig object using the json string input
        /// </summary>
        /// <param name="inJSONString">JSON string containing all config data for the logger</param>
        public LoggerConfig(string inJSONString)
        {
            JObject j = JObject.Parse(inJSONString);
            _logLevel = (string)j[JSONKeys.LOG_LEVEL_KEY];
            _logLocation = (string)j[JSONKeys.LOG_LOCATION_KEY];
            _logNameHeader = (string)j[JSONKeys.LOG_BASE_NAME_KEY];
        }


        /// <summary>
        /// Returns the location where the log will be placed
        /// </summary>
        /// <returns>The location where the lof will be placed</returns>
        public string getLogLocation() { return _logLocation; }

        /// <summary>
        /// Returns the default log level
        /// </summary>
        /// <returns>Default log level</returns>
        public string getLogLevel() { return _logLevel; }

        /// <summary>
        /// Returns the bae name of the log files
        /// </summary>
        /// <returns>The base name of the log files</returns>
        public string getLogNameHeader() { return _logNameHeader; }
    }
}
