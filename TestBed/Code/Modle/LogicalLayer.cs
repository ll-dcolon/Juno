using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TestBed
{
    class LogicalLayer : PhysicalDeviceInterface
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //The layer that actually communicates with the device
        private PhysicalLayer _physicalLayer;
        //Flag used to determine if a successful ping command was ever sent
        private bool _deviceConnected;

        //The current state of the led on the physical device
        private bool _ledHigh;

        //Dictionary to hold all pin current states
        private ConcurrentDictionary<DIOPins, bool> _pinStates;


        //Used to keep track of the current water temperature
        private double _currentWaterTemp;
        private object _waterTempLock;
        private double _currentFlowRate;
        private object _flowRateLock;

        //Lets the logical layer directly update the UI
        private UpdateUIInterface _updateUIDelegate;

        //Notifies the sequencer thread that there has been a new temp reading
        private AutoResetEvent _newTempReadingNotification;



        /// <summary>
        /// Creates the logical layer with a reference to the physical layer
        /// </summary>
        /// <param name="inPhysicalDevice">The physical layer object</param>
        public LogicalLayer(PhysicalLayer inPhysicalLayer)
        {
            log.Debug(string.Format("Creating logical layer with reference to physical layer:{0}", inPhysicalLayer));
            _physicalLayer = inPhysicalLayer;
            _deviceConnected = false;
            _ledHigh = false;
            _pinStates = new ConcurrentDictionary<DIOPins, bool>();
            _newTempReadingNotification = new AutoResetEvent(false);

            _currentWaterTemp = 0;
            _waterTempLock = new object();

            _currentFlowRate = -1;
            _flowRateLock = new object();

            //!@#Assumes the starting state for the device is all high.
            //I set this is the physical device start up
            //Make sure if you change something here, change it in the physical 
            //device too
            log.Debug("Setting starting states of the outputs");
            _pinStates.TryAdd(DIOPins.AirPump_AN0, true);
            _pinStates.TryAdd(DIOPins.WaterPump_AN2, true);
            _pinStates.TryAdd(DIOPins.Heater_AN1, true);
        }


        /// <summary>
        /// Sets the ui deleage to the input parameter
        /// </summary>
        /// <param name="inNewUIDelegate">The new UI delegate</param>
        public void setUIDelegate(UpdateUIInterface inNewUIDelegate)
        {
            _updateUIDelegate = inNewUIDelegate;
        }


        /// <summary>
        /// Determines the current state of the LED and tells the physical layer to change
        ///  that state
        /// Also saves the state of the led in a dedicated variable
        /// Note: Assumes the commands is successful
        /// </summary>
        public void toggleLED_LL()
        {
            log.Debug("LL_ToggleLED");
            if (_deviceConnected)
            {
                if (_ledHigh) { _physicalLayer.turnLEDOff_PL(); _ledHigh = false; }
                else { _physicalLayer.turnLEDOn_PL(); _ledHigh = true; }
            }
            else log.Error("Device is not connected, can not toggle LED");
        }

        /// <summary>
        /// Tells the physical layer to flash the led
        /// Sets the ledhigh varaible to true because the led always
        /// Stops flashing in the high state
        /// </summary>
        public void flashLED_LL()
        {
            log.Debug("Flash LED");
            if (_deviceConnected)
            {
                _physicalLayer.flashLED_PL();
                _ledHigh = true;
            }
            else log.Debug("Device is not connected, can not flash");
        }

        /// <summary>
        /// Tells the physical layer to change the state of the led to the 
        /// state of the input parameter
        ///
        /// Also updates the ledHigh variable
        /// </summary>
        /// <param name="inIsHigh">The state to set the led to</param>
        public void changeLEDState(bool inIsHigh)
        {
            log.Debug("Change led state");
            if (_deviceConnected)
            {
                log.Debug(string.Format("Changing LED state to {0}", inIsHigh));
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
            else log.Error("Can not change LED state because device is not connected");

        }


        /// <summary>
        /// Tells the physical layer to ping the device
        /// </summary>
        public void connectToDevice_LL()
        {
            log.Debug("Connecting to device");
            _physicalLayer.connectToDevice_PL();
        }


        /// <summary>
        /// Tells the physical layer to toggle the given pin.
        /// Uses a dictionary to determine the current state of the
        /// pin and tells the physical layer to put the pin in the oposite state
        /// 
        /// Then it updates the dictionary to the new state
        ///
        /// If a pin in passed in it will create a new event and then pass the
        /// event to the designated responder
        /// </summary>
        /// <param name="pinToToggle"></param>
        public void toggleOutput(DIOPins pinToToggle)
        {
            ToggleOutputEvent toggleOutputEvent = new ToggleOutputEvent(pinToToggle);
            toggleOutput(toggleOutputEvent);
        }

        public void toggleOutput(ToggleOutputEvent inToggleOutputEvent)
        {
            if (_deviceConnected)
            {
                bool currentState;
                DIOPins pinToToggle = inToggleOutputEvent._pinToToggle;
                _pinStates.TryGetValue(pinToToggle, out currentState);
                log.Debug(string.Format("Trying to set pin {0} with current state {1}, to state {2}", pinToToggle, currentState, !currentState));
                if (currentState)
                {
                    _physicalLayer.setOutputState(new UpdateOutputEvent(inToggleOutputEvent._pinToToggle, false));
                    _pinStates.TryUpdate(pinToToggle, false, true);
                }
                else
                {
                    _physicalLayer.setOutputState(new UpdateOutputEvent(inToggleOutputEvent._pinToToggle, true));
                    _pinStates.TryUpdate(pinToToggle, true, false);
                }
            }
            else log.Error("Can not toggle output because device is not connected");
        }


        /// <summary>
        /// Lets you control the outputs
        ///
        /// If the user inputs a pin and state, then a new event is created and the event is
        /// passed to the designated responder
        /// </summary>
        /// <param name="pinToChange">The pin you want to change the state of</param>
        /// <param name="inShouldSetHigh">The state you want the pin to take.  True = high, false = low</param>
        public void controlOutput(DIOPins pinToChange, bool inShouldSetHigh)
        {
            controlOutput(new UpdateOutputEvent(pinToChange, inShouldSetHigh));
        }

        public void controlOutput(UpdateOutputEvent inEvent)
        {
            if (_deviceConnected)
            {
                log.Debug(string.Format("Trying to set pin {0} to state {1}", inEvent._pinToUpdate, inEvent._shouldBeHigh));
                bool oldValue;
                _pinStates.TryGetValue(inEvent._pinToUpdate, out oldValue);
                _physicalLayer.setOutputState(inEvent);
                _pinStates.TryUpdate(inEvent._pinToUpdate, inEvent._shouldBeHigh, oldValue);
            }
            else
            {
                log.Error("Can not control output pins because device is not connected");
            }

        }





        /********************************** Physical Device Interface ********************************************/
        /// <summary>
        /// Tells the logical layer that the device has been successfully connected and pinged
        /// </summary>
        public void deviceConnected()
        {
            log.Debug("Device is connected");
            if (_deviceConnected == false)
            {
                _deviceConnected = true;
                if (_updateUIDelegate != null) _updateUIDelegate.updateConnectionStatus(true);
            }
        }
        public void flashLEDSent(){}
        public void ledControlSent(){}

        /// <summary>
        /// Tells the logical layer that the update output message was successfully sent
        /// Allows the Logical layer to then update the ui with that information
        /// </summary>
        /// <param name="inEvent"></param>
        public void updateOutputSent(UpdateOutputEvent inEvent)
        {
            if (_updateUIDelegate != null)
            {
                _updateUIDelegate.updateOutputState(inEvent);
            }
        }


        /// <summary>
        /// Fires every time the physical layer has new temp data from the device
        /// </summary>
        /// <param name="inNewTemp">The new temperature from the physical device</param>
        public void newThermistorData(double inNewTemp)
        {
            double roundedInput = Math.Round(inNewTemp, 1);
            lock (_waterTempLock)
            {
                if (roundedInput != _currentWaterTemp)
                {
                    _currentWaterTemp = roundedInput;
                    _newTempReadingNotification.Set();
                    if (_updateUIDelegate != null)
                    {
                        _updateUIDelegate.updateTempValue(roundedInput);
                    }
                } 
            }
        }

        /// <summary>
        /// Receives a message from the physical layer telling the new flow rate
        /// associated with the specified flowmeter 
        /// </summary>
        /// <param name="inEvent">The event that the flow rate relates to</param>
        /// <param name="inFlowRate">The new flow rate</param>
        public void newFlowmeterData(Event inEvent, double inFlowRate)
        {
            double roundInput = Math.Round(inFlowRate, 2);
            lock (_flowRateLock)
            {
                if (roundInput != _currentFlowRate)
                {
                    _currentFlowRate = roundInput;
                    if (_updateUIDelegate != null)
                    {
                        _updateUIDelegate.updateFlowRate(_currentFlowRate);
                    }
                }
            }
            
        }







        /************************************ Sequencer Methods *************************************************/
        public bool waitForNewTempReading(int inMSToWait = 0)
        {
            if (inMSToWait == 0){_newTempReadingNotification.WaitOne(); return true; }
            else{  return _newTempReadingNotification.WaitOne(inMSToWait);}
        }


        /// <summary>
        /// Returns the current water temp in a thread safe way
        /// </summary>
        /// <returns>The current water temp in C</returns>
        public double getCurrentWaterTemp()
        {
            double toReturn = 0;
            lock (_waterTempLock)
            {
                toReturn = _currentWaterTemp;
            }
            return toReturn;
        }


        /// <summary>
        /// Attempts to return the value of a pin.  If for some reason the dictionary can not get the value
        /// it will throw an exception
        /// </summary>
        /// <param name="pin">The pin to get the state of</param>
        /// <returns>True if the pin is high, false if it is not</returns>
        public bool getPinState(DIOPins pin)
        {
            bool toReturn = false;
            if (!_pinStates.TryGetValue(pin, out toReturn)) throw new Exception();
            return toReturn;
        }
    }
}
