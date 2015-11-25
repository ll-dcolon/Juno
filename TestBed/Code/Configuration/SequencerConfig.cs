using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TestBed
{
    class SequencerConfig
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
    (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Default times for all the recipies
        public static int _defaultTBHeaterOnWaterOn = 10000;
        public static int _defaultTBWaterOnWaterAndHeaterOff = 5000;
        public static int _defaultTBWaterAndHeaterOffAirOn = 5000;
        public static int _defaultTBAirOnAirOff = 6000;


        //two oz recipe
        public int _twoOzTBHeaterOnWaterOn;
        public int _twoOzTBWaterOnWaterAndHeaterOff;
        public int _twoOzTBWaterAndHeaterOffAirOn;
        public int _twoOzTBAirOnAirOff;


        //four oz recipe
        public int _fourOzTBHeaterOnWaterOn;
        public int _fourOzTBWaterOnWaterAndHeaterOff;
        public int _fourOzTBWaterAndHeaterOffAirOn;
        public int _fourOzTBAirOnAirOff;


        //eight oz recipe
        public int _eightOzTBHeaterOnWaterOn;
        public int _eightOzTBWaterOnWaterAndHeaterOff;
        public int _eightOzTBWaterAndHeaterOffAirOn;
        public int _eightOzTBAirOnAirOff;




        /// <summary>
        /// Sets up the Sequencer Config object using the jason string input
        /// </summary>
        /// <param name="inJsonString">A json string that has all config data for the sequencer object</param>
        public SequencerConfig(string inJsonString)
        {
            log.Debug(String.Format("Creating SequencerConfig object with json string {0}", inJsonString));
            JArray jArray = JArray.Parse(inJsonString);
            foreach (JObject sequencer in jArray)
            {
                string id = (string)sequencer[JSONKeys.SEQUENCER_ID];
                int tBHeaterOnWaterOn = (int)sequencer[JSONKeys.SEQUENCER_TIME_BETWEEN_HEATER_ON_WATER_ON];
                int tBWaterOnWaterAndHeaterOff = (int)sequencer[JSONKeys.SEQUENCER_TIME_BETWEEN_WATER_ON_WATER_AND_HEATER_OFF];
                int tBWaterAndHeaterOffAirOn = (int)sequencer[JSONKeys.SEQUENCER_TIME_BETWEEN_WATER_AND_HEATER_OFF_AIR_ON];
                int tBAirOnAirOff = (int)sequencer[JSONKeys.SEQUENCER_TIME_BETWEEN_AIR_ON_AIR_OFF];
                fillSequencerValues(id, tBHeaterOnWaterOn, tBWaterOnWaterAndHeaterOff, tBWaterAndHeaterOffAirOn, tBAirOnAirOff);
            }
        }





        private void fillSequencerValues(string sequencerId, int tBHeaterOnWaterOn, int tBWaterOnWaterAndHeaterOff, int tBWaterAndHeaterOffAirOn, int tBAirOnAirOff)
        {
            switch (sequencerId)
            {
                case JSONKeys.SEQUENCER_TWO_OZ_ID:
                    _twoOzTBHeaterOnWaterOn = tBHeaterOnWaterOn;
                    _twoOzTBWaterOnWaterAndHeaterOff = tBWaterOnWaterAndHeaterOff;
                    _twoOzTBWaterAndHeaterOffAirOn = tBWaterAndHeaterOffAirOn;
                    _twoOzTBAirOnAirOff = tBAirOnAirOff;
                    break;
                case JSONKeys.SEQUENCER_FOUR_OZ_ID:
                    _fourOzTBHeaterOnWaterOn = tBHeaterOnWaterOn;
                    _fourOzTBWaterOnWaterAndHeaterOff = tBWaterOnWaterAndHeaterOff;
                    _fourOzTBWaterAndHeaterOffAirOn = tBWaterAndHeaterOffAirOn;
                    _fourOzTBAirOnAirOff = tBAirOnAirOff;
                    break;
                case JSONKeys.SEQUENCER_EIGHT_OZ_ID:
                    _eightOzTBHeaterOnWaterOn = tBHeaterOnWaterOn;
                    _eightOzTBWaterOnWaterAndHeaterOff = tBWaterOnWaterAndHeaterOff;
                    _eightOzTBWaterAndHeaterOffAirOn = tBWaterAndHeaterOffAirOn;
                    _eightOzTBAirOnAirOff = tBAirOnAirOff;
                    break;
                default:
                    break;
            }

        }








    }
}
