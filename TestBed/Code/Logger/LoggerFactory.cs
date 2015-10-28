using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBed
{
    public class LoggerFactory
    {
        //The default logger that the singleton will use
        private static volatile Logger _defaultLogger;

        //Lock used to create the singleton logger correctly
        private static object singletonLock = new Object();


        /// <summary>
        /// Creates a new default logger to be accessed with 
        /// defaultLogger()
        /// </summary>
        /// <param name="inLoggerConfig">The logger config object to set up this logger</param>
        public static void createDefaultLogger(LoggerConfig inLoggerConfig)
        {
            if (_defaultLogger == null)
            {
                lock (singletonLock)
                {
                    if (_defaultLogger == null)
                    {
                        _defaultLogger = new Logger(inLoggerConfig);
                    }
                }
            }
        }



        /// <summary>
        /// Returns the default logger.  Logger must have been set up with
        /// a previous call to createDefaultLogger
        /// </summary>
        /// <returns></returns>
        public static Logger defaultLogger()
        {
            return _defaultLogger;
        }
    }
}
