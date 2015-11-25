using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBed
{
    class JSONKeys
    {
        //JSON keys in the log file
        public static string DEVICE_KEY = "Device";
        public static string DEVICE_BAUDRATE_KEY = "Device_BaudRate";
        public static string DEVICE_PORT_KEY= "Device_Port";

        public static string SEQUENCER_SEQUENCERS = "Sequencers";
        public static string SEQUENCER_ID = "Sequencer_Id";
        public static string SEQUENCER_TIMING_VALUES = "Timing_Values";
        public static string SEQUENCER_TIME_BETWEEN_HEATER_ON_WATER_ON = "TB_HeaterOn_WaterOn";
        public static string SEQUENCER_TIME_BETWEEN_WATER_ON_WATER_AND_HEATER_OFF = "TB_WaterOn_WaterAndHeaterOff";
        public static string SEQUENCER_TIME_BETWEEN_WATER_AND_HEATER_OFF_AIR_ON = "TB_WaterAndHeaterOff_AirOn";
        public static string SEQUENCER_TIME_BETWEEN_AIR_ON_AIR_OFF = "TB_AirOn_AirOff";

        public const string SEQUENCER_TWO_OZ_ID = "twoOzSequencer";
        public const string SEQUENCER_FOUR_OZ_ID = "fourOzSequencer";
        public const string SEQUENCER_EIGHT_OZ_ID = "eightOzSequencer";
    }
}
