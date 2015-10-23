using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;



namespace TestBed
{
    class SequencerLayer
    {
        private UIHandle_LLSL _uiHandler;
        private LogicalLayer _logicalLayer;



        //Flag to stop the processing thread;
        private volatile bool _shouldStop;

        private Thread _testSequencerThread;




        public SequencerLayer(UIHandle_LLSL inUIHandler, LogicalLayer inLogicalLayer)
        {
            _uiHandler = inUIHandler;
            _logicalLayer = inLogicalLayer;
            _shouldStop = false;
            startSequencerThreads();
        }

        /// <summary>
        /// Cleans up the object by requesting the processing thread stop
        /// </summary>
        ~SequencerLayer()
        {
            this.requestStop();
        }

        private void startSequencerThreads()
        {
            _testSequencerThread = new Thread(this.testSequencer);
            _testSequencerThread.Name = "testSequencerThread";
            _testSequencerThread.Start();
        }

        /// <summary>
        /// Stops the processing thread and merges it back into the main thread
        /// </summary>
        public void requestStop()
        {
            _shouldStop = true;
            _testSequencerThread.Join();
        }





        /// <summary>
        /// Runs the test sequencer sequence
        /// 
        /// NOTE: Only works if you do this in the initial state.  Because I use toggle,
        /// if you changed things the sequence will look a little different.  
        /// 
        /// The sequence is 
        /// Ping
        /// Flash LED
        /// Turn LED off
        /// Turn white on
        /// turn white off
        /// turn red on
        /// turn red off
        /// turn green on
        /// turn green off
        /// turn white and green on
        /// turn red on
        /// turn green off
        /// turn red off
        /// turn white off
        /// turn white on
        /// turn red on
        /// turn green on
        /// turn everything off
        /// turn everything on
        /// turn everything off
        /// </summary>
        private void testSequencer()
        {
            int msToDelay = 350;
            while (!_shouldStop)
            {
                _uiHandler.waitForStartTestSequenceClicked();
                _logicalLayer.connectToDevice_LL();
                Thread.Sleep(msToDelay);
                _logicalLayer.controlOutput(DIOPins.Heater_RA4, true);
                _logicalLayer.controlOutput(DIOPins.AirPump_RB6, true);
                _logicalLayer.controlOutput(DIOPins.WaterPump_RB7, true);
                Thread.Sleep(msToDelay);
                _logicalLayer.flashLED_LL();
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleLED_LL();
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.Heater_RA4);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.Heater_RA4);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_RB7);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_RB7);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.AirPump_RB6);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.AirPump_RB6);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.AirPump_RB6);
                _logicalLayer.toggleOutput(DIOPins.Heater_RA4);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_RB7);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.AirPump_RB6);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_RB7);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.Heater_RA4);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.Heater_RA4);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_RB7);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.AirPump_RB6);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.Heater_RA4);
                _logicalLayer.toggleOutput(DIOPins.AirPump_RB6);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_RB7);
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.Heater_RA4);
                _logicalLayer.toggleOutput(DIOPins.AirPump_RB6);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_RB7);
                _logicalLayer.toggleLED_LL();
                Thread.Sleep(msToDelay);
                _logicalLayer.toggleOutput(DIOPins.Heater_RA4);
                _logicalLayer.toggleOutput(DIOPins.AirPump_RB6);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_RB7);
                _logicalLayer.toggleLED_LL();
            }

        }
    }
}
