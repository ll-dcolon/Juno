using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBed
{
   
    //Identifiers for all the UI events this program will throw
    public enum UIEventIdentifier
    {
        //Used for a superclass.  Should not use in your program
        GenericEvent,

        //Used when various buttons are clicked.  Look at the 
        //Ui and you can propably figure them out
        ConnectClicked,
        FlashLEDClicked,
        ToggleLEDClicked,
        ChangeLEDStateClicked,
        ToggleOutputClicked,
        StartTestSequencerClicked
        
    };


    //Identifiers for all the message types this program can send
    //to the device
    public enum DeviceMessageIdentifier
    {
        PingDevice,
        FlashLED,
        LEDControl,
        RelayControl,
        DigitalIControl,
        DigitalOControl,
        AnalogPinQuery
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
    }


}
