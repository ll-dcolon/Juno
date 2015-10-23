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
        private PhysicalDevice _physicalDevice;
        private bool _deviceConnected;

        private AutoResetEvent _flashLEDSignal;
        private AutoResetEvent _controlLEDSignal;
        private AutoResetEvent _connectionSignal;

        private bool _ledHigh;



        public LogicalLayer(PhysicalDevice inPhysicalDevice)
        {
            _flashLEDSignal = new AutoResetEvent(false);
            _controlLEDSignal = new AutoResetEvent(false);
            _connectionSignal = new AutoResetEvent(false);

            _physicalDevice = inPhysicalDevice;
            _deviceConnected = false;
            _ledHigh = false;
        }



        public void toggleLED_LL()
        {
            if (_deviceConnected)
            {
                if (_ledHigh) { _physicalDevice.turnLEDOff_PL(); _ledHigh = false; }
                else { _physicalDevice.turnLEDOn_PL(); _ledHigh = true; }
            }
            else Console.WriteLine("device is not connected, can not toggle");
        }

        public void flashLED_LL()
        {
            if (_deviceConnected)
            {
                _physicalDevice.flashLED_PL();
                _ledHigh = true;
            }
            else Console.WriteLine("device is not connected, can not flash");
        }

        public void changeLEDState(bool inIsHigh)
        {
            if (inIsHigh)
            {
                _physicalDevice.turnLEDOn_PL();
                _ledHigh = true;
            }
            else
            {
                _physicalDevice.turnLEDOff_PL();
                _ledHigh = false;
            }
        }

        public void connectToDevice_LL()
        {
            _physicalDevice.connectToDevice_PL();
        }





        /********************************** Physical Device Interface ********************************************/
        public void deviceConnected()
        {
            _deviceConnected = true;
            _connectionSignal.Set();
        }
        public void flashLEDSent(){_flashLEDSignal.Set();}
        public void ledControlSent(){ _controlLEDSignal.Set();}







        /************************************ Sequencer Methods *************************************************/
        public bool waitForFlashLEDSent(int inMSToWait = 0)
        {
            if (inMSToWait == 0){_flashLEDSignal.WaitOne(); return true; }
            else{  return _flashLEDSignal.WaitOne(inMSToWait);}
        }

        public bool waitForControlLEDSent(int inMSToWait = 0)
        {
            if (inMSToWait == 0) { _controlLEDSignal.WaitOne(); return true;}
            else{ return _controlLEDSignal.WaitOne(inMSToWait);}
        }
    }
}
