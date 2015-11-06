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
        StartTestSequencerRequest,
        AnalogPinQueryRequest

    };


    //The names that we will give various pins
    public enum DIOPins
    {
        Heater_AN1 = 0x01,
        AirPump_AN0 = 0x00,
        WaterPump_AN2 = 0x02
    };


    //THe names of the analog pins
    public enum AnalogPins
    {
        Thermistor_AN5 = 0x05
    };


    public enum CounterPins
    {
        FlowMeter_RB6 = 0x06
    };


}
