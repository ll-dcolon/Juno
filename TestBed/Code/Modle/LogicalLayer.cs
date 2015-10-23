using System;
using System.Collections.Generic;
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


        /// <summary>
        /// Creates the logical layer with a reference to the physical layer
        /// </summary>
        /// <param name="inPhysicalDevice">The physical layer object</param>
        public LogicalLayer(PhysicalLayer inPhysicalLayer)
        {
            _physicalLayer = inPhysicalLayer;
            _deviceConnected = false;
            _ledHigh = false;
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
