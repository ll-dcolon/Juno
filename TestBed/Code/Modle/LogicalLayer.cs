using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TestBed
{
    class LogicalLayer : PhysicalDeviceInterface
    {
        //The layer that actually communicates with the device
        private PhysicalLayer _physicalLayer;
        //Flag used to determine if a successful ping command was ever sent
        private bool _deviceConnected;

        //The current state of the led on the physical device
        private bool _ledHigh;

        //Dictionary to hold all pin current states
        private ConcurrentDictionary<DIOPins, bool> _pinStates;


        /// <summary>
        /// Creates the logical layer with a reference to the physical layer
        /// </summary>
        /// <param name="inPhysicalDevice">The physical layer object</param>
        public LogicalLayer(PhysicalLayer inPhysicalLayer)
        {
            _physicalLayer = inPhysicalLayer;
            _deviceConnected = false;
            _ledHigh = false;
            _pinStates = new ConcurrentDictionary<DIOPins, bool>();

            //!@#Assumes the starting state for the device is all high.
            //I set this is the physical device start up
            //Make sure if you change something here, change it in the physical 
            //device too
            _pinStates.TryAdd(DIOPins.AirPump_RB6, true);
            _pinStates.TryAdd(DIOPins.WaterPump_RB7, true);
            _pinStates.TryAdd(DIOPins.Heater_RA4, true);
        }


        /// <summary>
        /// Determines the current state of the LED and tells the physical layer to change
        ///  that state
        /// </summary>
        public void toggleLED_LL()
        {
            if (_deviceConnected)
            {
                if (_ledHigh) { _physicalLayer.turnLEDOff_PL(); _ledHigh = false; }
                else { _physicalLayer.turnLEDOn_PL(); _ledHigh = true; }
            }
            else Console.WriteLine("device is not connected, can not toggle");
        }

        /// <summary>
        /// Tells the physical layer to flash the led
        /// </summary>
        public void flashLED_LL()
        {
            if (_deviceConnected)
            {
                _physicalLayer.flashLED_PL();
                _ledHigh = true;
            }
            else Console.WriteLine("device is not connected, can not flash");
        }

        /// <summary>
        /// Tells the physical layer to change the state of the led to the 
        /// state of the input parameter
        /// </summary>
        /// <param name="inIsHigh"></param>
        public void changeLEDState(bool inIsHigh)
        {
            if (inIsHigh)
            {
                _physicalLayer.turnLEDOn_PL();
                _ledHigh = true;
            }
            else
            {
                _physicalLayer.turnLEDOff_PL();
                _ledHigh = false;
            }
        }


        /// <summary>
        /// Tells the physical layer to ping the device
        /// </summary>
        public void connectToDevice_LL()
        {
            _physicalLayer.connectToDevice_PL();
        }


        /// <summary>
        /// Tells the physical layer to toggle the given pin.
        /// Uses a dictionary to determine the current state of the
        /// pin and tells the physical layer to put the pin in the oposite state
        /// 
        /// Then it updates the dictionary to the new state
        /// </summary>
        /// <param name="pinToToggle"></param>
        public void toggleOutput(DIOPins pinToToggle)
        {
            bool currentState;
            _pinStates.TryGetValue(pinToToggle, out currentState);
            if (currentState)
            {
                _physicalLayer.setOutputState(pinToToggle, false);
                _pinStates.TryUpdate(pinToToggle, false, true);
            }
            else
            {
                _physicalLayer.setOutputState(pinToToggle, true);
                _pinStates.TryUpdate(pinToToggle, true, false);
            }

        }


        /// <summary>
        /// Lets you control the outputs
        /// </summary>
        /// <param name="pinToChange">The pin you want to change the state of</param>
        /// <param name="inShouldSetHigh">The state you want the pin to take.  True = high, false = low</param>
        public void controlOutput(DIOPins pinToChange, bool inShouldSetHigh)
        {
            bool oldValue;
            _pinStates.TryGetValue(pinToChange, out oldValue);
            _physicalLayer.setOutputState(pinToChange, inShouldSetHigh);
            _pinStates.TryUpdate(pinToChange, inShouldSetHigh, oldValue);
        }





        /********************************** Physical Device Interface ********************************************/
        public void deviceConnected()
        {
            _deviceConnected = true;
        }
        public void flashLEDSent(){}
        public void ledControlSent(){}







        /************************************ Sequencer Methods *************************************************/
        //public bool waitForFlashLEDSent(int inMSToWait = 0)
        //{
        //    if (inMSToWait == 0){_flashLEDSignal.WaitOne(); return true; }
        //    else{  return _flashLEDSignal.WaitOne(inMSToWait);}
        //}
    }
}
