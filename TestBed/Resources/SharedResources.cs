using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBed
{

    //Identifiers for all the events this program will throw
    public enum EventIdentifier
    {
        //Used for a superclass.  Should not use in your program
        GenericEvent,

        //Used when various buttons are clicked.  Look at the 
        //Ui and you can propably figure them out
        ConnectRequest,
        FlashLEDRequest,
        ToggleLEDRequest,
        ChangeLEDStateRequest,
        ToggleOutputRequest,
        UpdateOutputRequest,
        QueryCounterRequest,
        StartSequencerRequest,
        AnalogPinQueryRequest

    };


    //IDs for the various sequencer types
    public enum SequenceID
    {
        TestSequence,
        twoOzSequence,
        fourOzSequence,
        eightOzSequence
    };


    //The names that we will give various pins
    public enum DIOPins
    {
        Heater_AN1 = 0x01,
        AirSolenoid_AN0 = 0x00,
        WaterPump_AN2 = 0x02,
        AirPump_AN3 = 0x03
    };


    //THe names of the analog pins
    public enum AnalogPins
    {
        Thermistor_AN5 = 0x05,
        Pressure_AN4 = 0x04
    };


    //Pins that can be used as event counters
    public enum CounterPins
    {
        FlowMeter_RB6 = 0x06
    };




    /// <summary>
    /// Returns the value to set the pin to in order for the device associated with that pin to be on
    /// For example, if you have to set a pin low to turn an LED on, then passing the LED pin
    /// into this function will return false, because a pin value of false turns the LED on
    /// </summary>
    public static class HelperMethods
    {
        public static bool getDeviceOnState(DIOPins pinToSetOn)
        {
            switch (pinToSetOn)
            {
                case DIOPins.Heater_AN1:
                    return true;
                case DIOPins.AirSolenoid_AN0:
                    return true;
                case DIOPins.WaterPump_AN2:
                    return true;
                case DIOPins.AirPump_AN3:
                    return true;
                default:
                    throw new Exception();
            }
        }
    }


}
