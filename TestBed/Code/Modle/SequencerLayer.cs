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



        private SequencerConfig _sequencerConfig;


        //Used if the sequencer wants to update the UI
        private UpdateUIInterface _uiDelegate;


        //The max allowable temp for the heater to register in C
        private const int MAX_HEATER_TEMP = 40;

        //Flag to stop the processing thread;
        private volatile bool _shouldStop;

        //Thread used for the basic sequencer testing
        private Thread _testSequencerThread;
        private Thread _twoOzSequencerThread;
        private Thread _fourOzSequencerThread;
        private Thread _eightOzSequencerThread;

        //Thread to monitor the temerpature of the heater and shuts
        //off power if the heater is too hot
        private Thread _heaterMonitoringThread;




        /// <summary>
        /// Creates a new sequencer with a reference to the UIHandle and logicalLayer objects
        /// This object is responsible for organizing the timing of all events
        /// </summary>
        /// <param name="inUIHandler">Reference to the UIHandler</param>
        /// <param name="inLogicalLayer">Reference to the logical layer</param>
        public SequencerLayer(UIHandle_LLSL inUIHandler, LogicalLayer inLogicalLayer, SequencerConfig inSequencerConfig)
        {
            log.Debug(string.Format("Creating sequencer:{0} with logicalLayer:{1} and UIHandler:{2}", this, inLogicalLayer, inUIHandler));
            _uiHandler = inUIHandler;
            _logicalLayer = inLogicalLayer;
            _shouldStop = false;
            _sequencerConfig = inSequencerConfig;
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

        /// <summary>
        /// Creats, names, and starts all the threads the sequencer manages
        /// </summary>
        private void startSequencerThreads()
        {
            log.Debug("Starting sequencer threads");
            _testSequencerThread = new Thread(this.testSequencer);
            _testSequencerThread.Name = "testSequencerThread";
            _testSequencerThread.Start();

            _heaterMonitoringThread = new Thread(this.heaterMonitorThread);
            _heaterMonitoringThread.Name = "heaterMonitoringThread";
            _heaterMonitoringThread.Start();

            _twoOzSequencerThread = new Thread(this.twoOzSequencer);
            _twoOzSequencerThread.Name = "twoOzSequencerThread";
            _twoOzSequencerThread.Start();

            _fourOzSequencerThread = new Thread(this.fourOzSequencer);
            _fourOzSequencerThread.Name = "fourOzSequencerThread";
            _fourOzSequencerThread.Start();

            _eightOzSequencerThread = new Thread(this.eightOzSequencer);
            _eightOzSequencerThread.Name = "eightOzSequencerThread";
            _eightOzSequencerThread.Start();
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
            _twoOzSequencerThread.Join();
            _fourOzSequencerThread.Join();
            _eightOzSequencerThread.Join();
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
                if (!_uiHandler.waitForStartTestSequenceRequest(100)) continue;
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
                _logicalLayer.controlOutput(DIOPins.Heater_AN1, !HelperMethods.getDeviceOnState(DIOPins.Heater_AN1));
                _logicalLayer.controlOutput(DIOPins.AirSolenoid_AN0, !HelperMethods.getDeviceOnState(DIOPins.AirSolenoid_AN0));
                _logicalLayer.controlOutput(DIOPins.WaterPump_AN2, !HelperMethods.getDeviceOnState(DIOPins.WaterPump_AN2));
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
                _logicalLayer.toggleOutput(DIOPins.AirSolenoid_AN0);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB6 again");
                _logicalLayer.toggleOutput(DIOPins.AirSolenoid_AN0);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB6 and RA4");
                _logicalLayer.toggleOutput(DIOPins.AirSolenoid_AN0);
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB7");
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RB6");
                _logicalLayer.toggleOutput(DIOPins.AirSolenoid_AN0);
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
                _logicalLayer.toggleOutput(DIOPins.AirSolenoid_AN0);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4, RB6, RB7");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                _logicalLayer.toggleOutput(DIOPins.AirSolenoid_AN0);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4, RB6, RB7, and LED");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                _logicalLayer.toggleOutput(DIOPins.AirSolenoid_AN0);
                _logicalLayer.toggleOutput(DIOPins.WaterPump_AN2);
                _logicalLayer.toggleLED_LL();
                Thread.Sleep(msToDelay);
                log.Debug("TS - Toggle RA4, RB6, RB7, and LED again");
                _logicalLayer.toggleOutput(DIOPins.Heater_AN1);
                _logicalLayer.toggleOutput(DIOPins.AirSolenoid_AN0);
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


        private void twoOzSequencer()
        {
            while (!_shouldStop)
            {
                if (!_uiHandler.waitForStartTwoOzSequencerRequest(100)) continue;
                if (_uiDelegate != null)
                {
                    _uiDelegate.updateSequencerState(true);
                    _uiDelegate.appendNote(string.Format("Starting 2oz Recipe\n"));
                }


                _logicalLayer.controlOutput(DIOPins.Heater_AN1, HelperMethods.getDeviceOnState(DIOPins.Heater_AN1));
                Thread.Sleep(_sequencerConfig._twoOzTBHeaterOnWaterOn);
                _logicalLayer.controlOutput(DIOPins.WaterPump_AN2, HelperMethods.getDeviceOnState(DIOPins.WaterPump_AN2));
                Thread.Sleep(_sequencerConfig._twoOzTBWaterOnWaterAndHeaterOff);
                _logicalLayer.controlOutput(DIOPins.Heater_AN1, !HelperMethods.getDeviceOnState(DIOPins.Heater_AN1));
                _logicalLayer.controlOutput(DIOPins.WaterPump_AN2, !HelperMethods.getDeviceOnState(DIOPins.WaterPump_AN2));
                Thread.Sleep(_sequencerConfig._twoOzTBWaterAndHeaterOffAirOn);
                _logicalLayer.controlOutput(DIOPins.AirSolenoid_AN0, HelperMethods.getDeviceOnState(DIOPins.AirSolenoid_AN0));
                Thread.Sleep(100);
                _logicalLayer.controlOutput(DIOPins.AirPump_AN3, HelperMethods.getDeviceOnState(DIOPins.AirPump_AN3));
                Thread.Sleep( _sequencerConfig._twoOzTBAirOnAirOff);
                _logicalLayer.controlOutput(DIOPins.AirPump_AN3, !HelperMethods.getDeviceOnState(DIOPins.AirPump_AN3));
                _logicalLayer.controlOutput(DIOPins.AirSolenoid_AN0, !HelperMethods.getDeviceOnState(DIOPins.AirSolenoid_AN0));


                if (_uiDelegate != null)
                {
                    _uiDelegate.updateSequencerState(false);
                    _uiDelegate.appendNote(string.Format("Finished 2oz Recipe\n"));
                }
            }
        }





        private void fourOzSequencer()
        {
            while (!_shouldStop)
            {
                if (!_uiHandler.waitForStartFourOzSequencerRequest(100)) continue;
                if (_uiDelegate != null)
                {
                    _uiDelegate.updateSequencerState(true);
                    _uiDelegate.appendNote(string.Format("Starting 4oz Recipe\n"));
                }



                _logicalLayer.controlOutput(DIOPins.Heater_AN1, HelperMethods.getDeviceOnState(DIOPins.Heater_AN1));
                Thread.Sleep(_sequencerConfig._fourOzTBHeaterOnWaterOn); ;
                _logicalLayer.controlOutput(DIOPins.WaterPump_AN2, HelperMethods.getDeviceOnState(DIOPins.WaterPump_AN2));
                Thread.Sleep(_sequencerConfig._fourOzTBWaterOnWaterAndHeaterOff); ;
                _logicalLayer.controlOutput(DIOPins.Heater_AN1, !HelperMethods.getDeviceOnState(DIOPins.Heater_AN1));
                _logicalLayer.controlOutput(DIOPins.WaterPump_AN2, !HelperMethods.getDeviceOnState(DIOPins.WaterPump_AN2));
                Thread.Sleep(_sequencerConfig._fourOzTBWaterAndHeaterOffAirOn); ;
                _logicalLayer.controlOutput(DIOPins.AirSolenoid_AN0, HelperMethods.getDeviceOnState(DIOPins.AirSolenoid_AN0));
                Thread.Sleep(100);
                _logicalLayer.controlOutput(DIOPins.AirPump_AN3, HelperMethods.getDeviceOnState(DIOPins.AirPump_AN3));
                Thread.Sleep(_sequencerConfig._fourOzTBAirOnAirOff); ;
                _logicalLayer.controlOutput(DIOPins.AirPump_AN3, !HelperMethods.getDeviceOnState(DIOPins.AirPump_AN3));
                _logicalLayer.controlOutput(DIOPins.AirSolenoid_AN0, !HelperMethods.getDeviceOnState(DIOPins.AirSolenoid_AN0));


                if (_uiDelegate != null)
                {
                    _uiDelegate.updateSequencerState(false);
                    _uiDelegate.appendNote(string.Format("Finished 4oz Recipe\n"));
                }
            }
        }

        private void eightOzSequencer()
        {
            while (!_shouldStop)
            {
                if (!_uiHandler.waitForStartEightOzSequencerRequest(100)) continue;
                _uiHandler.waitForStartEightOzSequencerRequest();
                if (_uiDelegate != null)
                {
                    _uiDelegate.updateSequencerState(true);
                    _uiDelegate.appendNote(string.Format("Starting 8oz Recipe\n"));
                }



                _logicalLayer.controlOutput(DIOPins.Heater_AN1, HelperMethods.getDeviceOnState(DIOPins.Heater_AN1));
                Thread.Sleep(_sequencerConfig._eightOzTBHeaterOnWaterOn);
                _logicalLayer.controlOutput(DIOPins.WaterPump_AN2, HelperMethods.getDeviceOnState(DIOPins.WaterPump_AN2));
                Thread.Sleep(_sequencerConfig._eightOzTBWaterOnWaterAndHeaterOff);
                _logicalLayer.controlOutput(DIOPins.Heater_AN1, !HelperMethods.getDeviceOnState(DIOPins.Heater_AN1));
                _logicalLayer.controlOutput(DIOPins.WaterPump_AN2, !HelperMethods.getDeviceOnState(DIOPins.WaterPump_AN2));
                Thread.Sleep(_sequencerConfig._eightOzTBWaterAndHeaterOffAirOn);
                _logicalLayer.controlOutput(DIOPins.AirSolenoid_AN0, HelperMethods.getDeviceOnState(DIOPins.AirSolenoid_AN0));
                Thread.Sleep(100);
                _logicalLayer.controlOutput(DIOPins.AirPump_AN3, HelperMethods.getDeviceOnState(DIOPins.AirPump_AN3));
                Thread.Sleep(_sequencerConfig._eightOzTBAirOnAirOff);
                _logicalLayer.controlOutput(DIOPins.AirPump_AN3, !HelperMethods.getDeviceOnState(DIOPins.AirPump_AN3));
                _logicalLayer.controlOutput(DIOPins.AirSolenoid_AN0, !HelperMethods.getDeviceOnState(DIOPins.AirSolenoid_AN0));


                if (_uiDelegate != null)
                {
                    _uiDelegate.updateSequencerState(false);
                    _uiDelegate.appendNote(string.Format("Finished 8oz Recipe\n"));
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
                if (!_logicalLayer.waitForNewTempReading(100)) continue;
                double currentTemp = _logicalLayer.getCurrentWaterTemp();
                if (currentTemp >= MAX_HEATER_TEMP)
                {
                    //Turn on the heater if its too hot
                    bool currentHeaterPinState = _logicalLayer.getPinState(DIOPins.Heater_AN1);

                    ///!@#CHANGE WHEN NOT ON THE TESTBED ANY MORE
                    currentHeaterPinState = false;

                    if (currentHeaterPinState == HelperMethods.getDeviceOnState(DIOPins.Heater_AN1))
                    {
                        _logicalLayer.controlOutput(DIOPins.Heater_AN1, !HelperMethods.getDeviceOnState(DIOPins.Heater_AN1));
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
