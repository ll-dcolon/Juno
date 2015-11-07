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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Objects that aid in sequencer control by providing waitFor___ methods
        private UIHandle_LLSL _uiHandler;
        private LogicalLayer _logicalLayer;


        //Used if the sequencer wants to update the UI
        private UpdateUIInterface _uiDelegate;


        //The max allowable temp for the heater to register in C
        private const int MAX_HEATER_TEMP = 25;

        //Flag to stop the processing thread;
        private volatile bool _shouldStop;

        //Thread used for the basic sequencer testing
        private Thread _testSequencerThread;
        //Thread to monitor the temerpature of the heater and shuts
        //off power if the heater is too hot
        private Thread _heaterMonitoringThread;




        /// <summary>
        /// Creates a new sequencer with a reference to the UIHandle and logicalLayer objects
        /// This object is responsible for organizing the timing of all events
        /// </summary>
        /// <param name="inUIHandler">Reference to the UIHandler</param>
        /// <param name="inLogicalLayer">Reference to the logical layer</param>
        public SequencerLayer(UIHandle_LLSL inUIHandler, LogicalLayer inLogicalLayer)
        {
            log.Debug(string.Format("Creating sequencer:{0} with logicalLayer:{1} and UIHandler:{2}", this, inLogicalLayer, inUIHandler));
            _uiHandler = inUIHandler;
            _logicalLayer = inLogicalLayer;
            _shouldStop = false;
            startSequencerThreads();
        }


        /// <summary>
        /// Sets the sequencers UI delegate
        /// </summary>
        /// <param name="inNewUIDelegate">The new UI delegate</param>
        public void setUIDelegate(UpdateUIInterface inNewUIDelegate)
        {
            _uiDelegate = inNewUIDelegate;
        }



        /// <summary>
        /// Cleans up the object by requesting the processing thread stop
        /// </summary>
        ~SequencerLayer()
        {
            log.Debug(string.Format("Destroying sequencer:{0}", this));
            this.requestStop();
        }

        private void startSequencerThreads()
        {
            log.Debug("Starting sequencer threads");
            _testSequencerThread = new Thread(this.testSequencer);
            _testSequencerThread.Name = "testSequencerThread";
            _testSequencerThread.Start();

            _heaterMonitoringThread = new Thread(this.heaterMonitorThread);
            _heaterMonitoringThread.Name = "heaterMonitoringThread";
            _heaterMonitoringThread.Start();
        }

        /// <summary>
        /// Stops the processing thread and merges it back into the main thread
        /// </summary>
        public void requestStop()
        {
            log.Debug("Requesting sequencer stop");
            _shouldStop = true;
            _testSequencerThread.Join();
            _heaterMonitoringThread.Join();
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
                _uiHandler.waitForStartTestSequenceRequest();
                if (_uiDelegate != null)
                {
                    _uiDelegate.updateSequencerState(true);
                    _uiDelegate.appendNote(string.Format("Start Sequencer\n"));
                }
                log.Info(string.Format("Running test sequener with a {0} ms delay metween commands", msToDelay));
                log.Debug("TS - Connecting to device");
                _logicalLayer.connectToDevice_LL();
                Thread.Sleep(msToDelay);
                log.Debug("TS - Turning off all the outputs");
                _logicalLayer.controlOutput(DIOPins.Heater_AN1, true);
                _logicalLayer.controlOutput(DIOPins.AirPump_AN0, true);
                _logicalLayer.controlOutput(DIOPins.WaterPump_AN2, true);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Flashing the main LED");
                _logicalLayer.flashLED_LL();
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle the main led");
                _logicalLayer.toggleLED_LL();
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4 again");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB7");
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB7 again");
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB6");
                _logicalLayer.toggleOutput(DIOPins.AirPump_AN0);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB6 again");
                _logicalLayer.toggleOutput(DIOPins.AirPump_AN0);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB6 and RA4");
                _logicalLayer.toggleOutput(DIOPins.AirPump_AN0);
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB7");
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB6");
                _logicalLayer.toggleOutput(DIOPins.AirPump_AN0);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB7");
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB7");
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB6");
                _logicalLayer.toggleOutput(DIOPins.AirPump_AN0);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4, RB6, RB7");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                _logicalLayer.toggleOutput(DIOPins.AirPump_AN0);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4, RB6, RB7, and LED");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                _logicalLayer.toggleOutput(DIOPins.AirPump_AN0);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                _logicalLayer.toggleLED_LL();
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4, RB6, RB7, and LED again");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                _logicalLayer.toggleOutput(DIOPins.AirPump_AN0);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                _logicalLayer.toggleLED_LL();
                log.Info("Test sequencer finished successfully");
                if (_uiDelegate != null)
                {
                    _uiDelegate.updateSequencerState(false);
                    _uiDelegate.appendNote(string.Format("End Sequencer\n"));
                }
            }
        }



        /// <summary>
        /// Makes sure the heater does not overheat
        /// </summary>
        private void heaterMonitorThread()
        {
            while (!_shouldStop)
            {
                _logicalLayer.waitForNewTempReading();
                double currentTemp = _logicalLayer.getCurrentWaterTemp();
                if (currentTemp >= MAX_HEATER_TEMP)
                {
                    //Turn on the heater if its too hot
                    bool currentHeaterPinState = _logicalLayer.getPinState(DIOPins.Heater_AN1);

                    if (currentHeaterPinState == false)
                    {
                        _logicalLayer.controlOutput(DIOPins.Heater_AN1, true);
                        if (_uiDelegate != null)
                        {
                          _uiDelegate.appendNote("Heater is too hot, turning it off now\n");
                        }
                    }
                }
            }
        }
    }
}
