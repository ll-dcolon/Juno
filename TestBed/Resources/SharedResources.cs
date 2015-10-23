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
        ChangeLEDStateClicked
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
        DigitalOControl
    };


}
