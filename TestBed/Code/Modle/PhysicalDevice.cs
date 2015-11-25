using System;
using System.IO.Ports;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TestBed
{
    /// <summary>
    /// Any class which wants to receive events from the PhysicalDevice must
    /// implement this interface
    /// </summary>
    public interface PhysicalDeviceInterface
    {
        /// <summary>
        /// Notifies the delegate when the device has been connected (when a successful ping has been received)
        /// </summary>
        void deviceConnected();

        /// <summary>
        /// Notifies the delegate when the flashLED command has been successfully send
        /// </summary>
        void flashLEDSent();

        /// <summary>
        /// Notifies the delegate when the ledControl message has been successfully sent
        /// </summary>
        void ledControlSent();

        /// <summary>
        /// Physical layer send the new temp data from the device to its delegate
        /// </summary>
        /// <param name="inNewTemp">The new temp data from the device in C</param>
        void newThermistorData(double inNewTemp);


        /// <summary>
        /// Tells the delegate that the update output message was successfuly sent for the specified event
        /// </summary>
        /// <param name="inEvent">The event that was sent</param>
        void updateOutputSent(UpdateOutputEvent inEvent);


        /// <summary>
        /// Tells the delegate that there is new flowmeter data related to the specified event
        /// </summary>
        /// <param name="inEvent">The event that this rate relates to</param>
        /// <param name="newFlowRate">The new flow rate</param>
        void newFlowmeterData(Event inEvent, double newFlowRate);


        /// <summary>
        /// Tells the delegate that there is new pressure sensor datad
        /// </summary>
        /// <param name="newPressureReading">The new pressure reading</param>
        void newPressureData(double newPressureReading);
    }






    public class PhysicalLayer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Loggers for specific pieces of data
        private static readonly log4net.ILog tempLog = log4net.LogManager.GetLogger("Temperature");
        private static readonly log4net.ILog flowLog = log4net.LogManager.GetLogger("Flow");
        private static readonly log4net.ILog pressureLog = log4net.LogManager.GetLogger("Pressure");




        //The port used to communicate with the serial device
        private SerialPort _serialPort;
        //The configuration object used for the device
        private DeviceConfig _deviceConfig;
        //Buffer used to hold incoming bytes until they can be parsed
        private ConcurrentQueue<byte[]> _incommingByteQueue;
        //Used to lock communication with the device
        private Object _lockDeviceComm;
        //Tells the queue that it is allowed to recieve messages
        private volatile bool _canReceiveResponce;
        //Used to notify the pprocessing thread when a new element
        //has been received by the serial port
        private AutoResetEvent _enqueueEvent;
        //Reference to the delegate that will handle all the events
        //the device picks up
        private PhysicalDeviceInterface deviceDelegate;

        //THe number of bytes we expect from the device
        private int _numBytesInResponce;

        //Threads used to continuously query sensors
        private Thread _thermistorVoltageThread;

        private Thread _pressureSensorThread;

        //Thread to query flow meter counter
        private Thread _flowmeterCounterThread;

        //Queue to hold all the returned flow meter counter values
        private Queue<int> _flowCounterQueue;

        //The number of ms to sleep between queries.
        private const int MS_BETWEEN_THERMISTOR_QUERYS= 200;
        //The nubmer of ms to sleep between flowmeter counter queries
        private const int MS_BETWEEN_FLOWMETER_QUERIES = 200;
        //The number of ms to sleep between pressure sensor queries
        private const int MS_BETWEEN_PRESSURE_QUERIES = 200;
        //The number of elements that the fow meter queue can have
        private const int MAX_NUMBER_FLOWMETER_QUEUE_ELEMENTS = 20;
        //The number of ms to wait for a responce
        private const int MS_TO_WAIT_FOR_RESPONCE = 3000;
        //The number of ml represented by a pulse of the flow meter
//        private const double ML_OF_WATER_PER_PULSE = 0.5194; //For 1.2mm nozzle
        private const double ML_OF_WATER_PER_PULSE = 0.4197;


        //Theta 0 and Theta 1 for mressure sensor line
        private const double _yIntercept = -26.906;
        private const double _slope = 24.757;


        //The voltage for the thermistor
        private const double REF_VOLTAGE = 5.022;
        //The resistance used for R2 in the voltage divider
        private const int VOLTAGE_DIVIDER_RESISTANCE = 47000;
        //THermistor reference resistance 
        private const int THERMISTOR_REF_RESISTANCE = 100000;
        //THermistor reference temp in kelvin
        private const double THERMISTOR_REF_TEMP_K = 298.15;
        //THermistor constant
        private const int THERMISTOR_B_VALUE = 3950;

        //Flag used to manage threads
        private volatile bool _shouldStop;
        private volatile bool _hasStarted;


        /// <summary>
        /// Creates a new physical device object and attempt to open the serial port
        /// </summary>
        public PhysicalLayer(DeviceConfig inDeviceConfig)
        {
            log.Debug(string.Format("Creating physicalLayer with config:{0}", inDeviceConfig));
            _deviceConfig = inDeviceConfig;
            _lockDeviceComm = new object();
            _flowCounterQueue = new Queue<int>();
            _enqueueEvent = new AutoResetEvent(false);
            _canReceiveResponce = false;
            findSerialDevice();
            _incommingByteQueue = new ConcurrentQueue<byte[]>();

            _shouldStop = false;
            _hasStarted = false;
        }


        /// <summary>
        /// Destroys the physical layer
        /// </summary>
        ~PhysicalLayer()
        {
            log.Debug("Destroying physical layer");
            if (_hasStarted)
            {
                stopThreads();
                turnOffOutputs();
            }
        }

        /// <summary>
        /// Turns off all the outputs so the device is in a known state
        /// </summary>
        public void turnOffOutputs()
        {
            log.Debug("Setting the state of all the outputs to off");
            //AN0
            _serialPort.Write(buildMessage(new List<int> { 0x05, 0x35, (int)DIOPins.AirSolenoid_AN0, 0x00, Convert.ToInt32(!(HelperMethods.getDeviceOnState(DIOPins.AirSolenoid_AN0))) }));
            //AN2
            _serialPort.Write(buildMessage(new List<int> { 0x05, 0x35, (int)DIOPins.WaterPump_AN2, 0x00, Convert.ToInt32(!(HelperMethods.getDeviceOnState(DIOPins.WaterPump_AN2))) }));
            //AN1
            _serialPort.Write(buildMessage(new List<int> { 0x05, 0x35, (int)DIOPins.Heater_AN1, 0x00, Convert.ToInt32(!(HelperMethods.getDeviceOnState(DIOPins.Heater_AN1))) }));
            //AN3
            _serialPort.Write(buildMessage(new List<int> { 0x05, 0x35, (int)DIOPins.AirPump_AN3, 0x00, Convert.ToInt32(!(HelperMethods.getDeviceOnState(DIOPins.AirPump_AN3))) }));

        }


        /// <summary>
        /// Stops all the sensor monitoring threads
        /// </summary>
        public void stopThreads()
        {
            _shouldStop = true;
            _thermistorVoltageThread.Join();
            _flowmeterCounterThread.Join();
            _pressureSensorThread.Join();
        }



        /// <summary>
        /// Attempts to open the serial port and communicate with the device
        /// 
        /// Current defaults are set to COM3 and 9600 BaudRate
        /// </summary>
        private void findSerialDevice()
        {
            log.Info(string.Format("Attempting to connect to serial port:{0} with baud rate:{1}", _deviceConfig.getDevicePort(), _deviceConfig.getBaudRate()));
            _serialPort = new SerialPort();
            _serialPort.PortName = _deviceConfig.getDevicePort();
            _serialPort.BaudRate = _deviceConfig.getBaudRate();  //!@#Make these not hard coded.  Config file would be best
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            try
            {
                log.Debug("Attempting to open serial port");
                _serialPort.Open();
                log.Debug("Opened serial port");
            }
            catch (Exception e)
            {
                log.Fatal(string.Format("Could not open serial port:{0} with baud rate:{1}", _deviceConfig.getDevicePort(), _deviceConfig.getBaudRate()));
                throw e; 
            }
            InitializeDevice();
        }


        /// <summary>
        /// Sets the initial state of the physical device
        /// </summary>
        private void InitializeDevice()
        {
            //Set the state of all the digital outputs to be low
            //These states must match the initial state of the pin dictionary in the logical layer
            //If you change something here, make sure you change it there too
            turnOffOutputs();

            //Set up event counter on RB6
            _serialPort.Write(buildMessage(new List<int> { 0x04, 0x36, 0x06, 0x00 }));
        }


        /// <summary>
        /// Sets teh physical device delegate
        /// </summary>
        /// <param name="inDelegate">The object that wants to receive messages from the physical device object</param>
        public void setDelegate(PhysicalDeviceInterface inDelegate)
        {
            log.Debug(string.Format("Setting physical layer delegate to {0}", inDelegate));
            deviceDelegate = inDelegate;
        }


        //**********************************************************************************************************************************************************************
        //Wrappers around the sendMessage function.  A parameter to the sendMessage function are the actual bytes to send
        //This is where you can see what bytes im sending for what commands
        public void connectToDevice_PL()
        {
            log.Debug("Sending ping device message");
            sendMessage(new ConnectEvent(),buildMessage(new List<int> { 0x02, 0x27 }));
        }

        public void flashLED_PL()
        {
            log.Debug("Sending flash led message");
            sendMessage(new FlashLEDEvent(), buildMessage(new List<int> {0x02, 0x28 }));
        }

        public void turnLEDOn_PL()
        {
            log.Debug("Sending ledControl message to turn the led on");
            sendMessage(new ChangeLEDStateEvent(true), buildMessage(new List<int> { 0x03, 0x29, 0x00 }));
        }

        public void turnLEDOff_PL()
        {
            log.Debug("Sending ledControl message to turn the led off");
            sendMessage(new ChangeLEDStateEvent(false), buildMessage(new List<int> { 0x03, 0x29, 0x01 }));
        }

        public void setOutputState(UpdateOutputEvent inEvent)
        {
            int newPinState;
            if (inEvent._shouldBeHigh) newPinState = 0x01;
            else newPinState = 0x00;
            log.Debug(string.Format("Sending DigitalIOControl message to turn pin {0} to state {1}", (int)inEvent._pinToUpdate, newPinState));
            sendMessage(inEvent, buildMessage(new List<int> { 0x05, 0x35, (int)inEvent._pinToUpdate, 0x00, newPinState }));
        }


        public void queryAnalogChannel(QueryAnalogInputEvent inEvent)
        {
            log.Debug(string.Format("Querying Analog pin {0}", inEvent._pinToQuery));
            sendMessage(inEvent, buildMessage(new List<int> {0x03, 0x50, (int)inEvent._pinToQuery}));
        }


        public void queryCounter(QueryCounterEvent inEvent)
        {
            log.Debug(string.Format("Querying Counter pin {0}", inEvent._pinToQuery));
            sendMessage(inEvent, buildMessage(new List<int> { 0x3, 0x37, 0x06 }));
        }
        //**********************************************************************************************************************************************************************




        /// <summary>
        /// Helper method used to combine the byte list into a string to send to sendMessage function
        /// </summary>
        /// <param name="bytes">List<int> of the bytes to send</param>
        /// <returns>A string version consisting of the character representation of the bytes sent in</returns>
        private string buildMessage(List<int> bytes)
        {
            log.Debug(string.Format("Building message with bytes: {0}", bytes.ToString()));
            StringBuilder sb = new StringBuilder();
            foreach (int item in bytes)
            {
                sb.Append((char)item);
            }
            log.Debug(string.Format("Returning string message: {0}", sb.ToString()));
            return sb.ToString();
        }


        /// <summary>
        /// Handles sending a message to the serial device.  It makes sure the port is still open, locks the
        /// serial object, sends the message, and waits for a responce if necessary.  
        /// </summary>
        /// <param name="inMessageIdentifier">The type of message I am sending</param>
        /// <param name="inMessageToSend">The string to send</param>
        private void sendMessage(Event inEvent, string inMessageToSend)
        {
            log.Debug("Attempting to send message to device");
            if (_serialPort.IsOpen)
            {
                lock(_lockDeviceComm)
                {
                    _numBytesInResponce = getResponceLength(inEvent.identifier);
                    log.Debug(string.Format("Expecting {0} bytes from the device as a responce", _numBytesInResponce));
                    _canReceiveResponce = true;
                    _serialPort.Write(inMessageToSend);
                    if (_numBytesInResponce> 0)
                    {
                        byte[] responce = new byte[_numBytesInResponce];
                        bool receivedResponce = waitForResponce(_numBytesInResponce, ref responce);
                        log.Debug(string.Format("Received responce {0}", receivedResponce));
                        if (receivedResponce)
                        {
                            processResponce(inEvent, responce);
                            _canReceiveResponce = false;
                            log.Debug("Should no longer receive a responce");
                        }
                    }
                    else
                    {
                        log.Debug(string.Format("Sending command finished notification for command: {0}", inEvent.identifier));
                        sendCommandFinishedNotification(inEvent);
                        _canReceiveResponce = false;
                    }
                }

            }
            else
            {
                log.Fatal("Serial Port is not open!!!, killing the program");
                throw new Exception();
            }
        }


        /// <summary>
        /// Waits for the responce from the device.  Will timeout after 1 second.
        /// </summary>
        /// <param name="inNumBytesToReceivce">The number of bytes that we expect to get from the queue</param>
        /// <param name="outResponce">The string to fill with the responce taken from the queue</param>
        /// <returns>True if we got a responce of the correct length</returns>
        private bool waitForResponce(int inNumBytesToReceivce, ref byte[] outResponce)
        {
            log.Debug("Waiting for responce from device");
            int msToWait = MS_TO_WAIT_FOR_RESPONCE;
            if (_enqueueEvent.WaitOne(msToWait))
            {
                if (_incommingByteQueue.Count == 0)
                {
                    throw new Exception();
                }
                bool success = _incommingByteQueue.TryDequeue(out outResponce);
                log.Debug(string.Format("Received a responce from the device.  Responce: {0}", outResponce));

                if (outResponce.Length == inNumBytesToReceivce)
                {
                    log.Debug(string.Format("Received responce of correct length. {0}",outResponce));
                    return true;
                }
                else
                {
                    log.Error(string.Format("Received responce string:{0} - with length {1} but expected a string of length {2}", outResponce, outResponce.Length, inNumBytesToReceivce));
                    throw new Exception();
                }
            }
            else
            {
                log.Error(string.Format("Did not receive a responce in {0}ms", msToWait));
                return false;
                throw new Exception();
            }
        }






        /// <summary>
        /// Recieve string from the device and put it into the queue for another thread.
        /// 
        /// Only allowed to recieve a responce when the _canReceiveResponce flag is set.  It is set right before
        /// sending a message and then cleared right after the message is received.  This means I can currently
        /// not receive message initiated by the device.  For now thats fine, may need to change in the future
        /// </summary>
        /// <param name="sender">The serial port object that recieved the data</param>
        /// <param name="e">SerialDataReceivedEventArgs</param>
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            log.Debug("Received something from the device");
            if (!_canReceiveResponce)
            {
                SerialPort port = (SerialPort)sender;
                byte[] bytes = new byte[_numBytesInResponce];
                port.Read(bytes, 0, _numBytesInResponce);
                logBytesArray(bytes);
                log.Fatal(string.Format("Received responce from device when should not have.  Responce:{0}", bytes));
                throw new Exception();
            }
            try
            {
                SerialPort port = (SerialPort)sender;
                int bytesToRead = port.BytesToRead;
                //Only read if the entire message is there
                if (bytesToRead < _numBytesInResponce) return;
                byte[] bytes = new byte[_numBytesInResponce];
                port.Read(bytes, 0, _numBytesInResponce);
                logBytesArray(bytes);

                log.Debug(string.Format("Received string {0}", bytes));
                _incommingByteQueue.Enqueue(bytes);
                _enqueueEvent.Set();

            }
            catch (Exception exception)
            {
                log.Fatal("Received exception when trying to read from serial port", exception);
                throw exception;
            }
        }



        /// <summary>
        /// Looks over the responce received from the device and tries to process it
        /// 
        /// Will continue until processing is complete.  This is the same thread that sent the message.
        /// Try to limit processing as much as possible.  May eventually need to have another queue / processing thread.
        /// For now thougn, we keep it simple
        /// </summary>
        /// <param name="inMessageIdentifier">The message type we are sending</param>
        /// <param name="inResponce">The responce we recieved from the device</param>
        private void processResponce(Event inEvent, byte[] inResponce)
        {
            log.Debug(string.Format("Processing responce from message {0}", inEvent.identifier));
            switch (inEvent.identifier)
            {
                case EventIdentifier.ConnectRequest:
                    logBytesArray(inResponce);
                    if (inResponce.Length == 1 && inResponce[0] == 0x59)
                    {
                        log.Debug("Received correct value for pind message");
                        if (deviceDelegate != null)
                        {
                            log.Debug("Sending device connected to the delegate");
                            deviceDelegate.deviceConnected();
                        }
                        if (!_hasStarted)
                        {
                            startSensorThreads();
                            _hasStarted = true;
                        }
                    }
                    break;
                case EventIdentifier.AnalogPinQueryRequest:
                    QueryAnalogInputEvent queryAnalogEvent = (QueryAnalogInputEvent)inEvent;
                    double voltage = getAnalogPinVoltage(inResponce);
                    if (queryAnalogEvent._pinToQuery == AnalogPins.Thermistor_AN5)
                    {
                        double resistance = getThermistorResistance(voltage);
                        double tempC = getTempInC(resistance);
                        tempLog.Info(string.Format("{0}", tempC));
                        if (deviceDelegate != null)
                        {
                            log.Debug("Sending temp data to logical layer");
                            deviceDelegate.newThermistorData(tempC);
                        }
                    }
                    else if (queryAnalogEvent._pinToQuery == AnalogPins.Pressure_AN4)
                    {
                        voltage = Math.Round(voltage, 2);
                        double pressure = (_slope * voltage) + _yIntercept;
                        log.Debug(string.Format("Received pressure reading : {0}", pressure));
                        pressureLog.Info(string.Format("{0}", pressure));
                        if (deviceDelegate != null)
                        {
                            log.Debug("Sending new pressure data to delegate");
                            deviceDelegate.newPressureData(pressure);
                        }
                    }
                    break;
                case EventIdentifier.QueryCounterRequest:
                    logBytesArray(inResponce);
                    int responce = (inResponce[3] << 24) | (inResponce[2] << 16) | (inResponce[1] << 8) | (inResponce[0]) ;
                    handleNewFlowmeterCounterValue(inEvent, responce);
                    break;
                default:
                    log.Error(string.Format("Message type {0} does not expect a responce", inEvent.identifier));
                    break;
            }
            log.Debug("Done processing responce");
        }


        /// <summary>
        /// Determines the expected responce length given the message type
        /// </summary>
        /// <param name="eventIdentifier">The type of message we are going to send</param>
        /// <returns>The number of bytes (as a string) we expect to receive from the device</returns>
        private int getResponceLength(EventIdentifier inEventIdentifier)
        {
            int toReturn = 0;
            log.Debug(string.Format("getting responce length for message type {0}", inEventIdentifier));
            switch (inEventIdentifier)
            {
                case EventIdentifier.ConnectRequest:
                    toReturn = 1;
                    break;
                case EventIdentifier.AnalogPinQueryRequest:
                    toReturn = 2;
                    break;
                case EventIdentifier.QueryCounterRequest:
                    toReturn = 4;
                    break;
                default:
                    break;
            }
            log.Debug(string.Format("returning responce length:{0} for message type {1}", toReturn, inEventIdentifier));
            return toReturn;
        }


        /// <summary>
        /// Used for all commands that dont expect a responce from the device.  This lets the physical layer still
        /// notify the logical layer that the message has been successfuly sent.
        /// </summary>
        /// <param name="inDeviceMessage">The type of message we sent</param>
        public void sendCommandFinishedNotification(Event inEvent)
        {
            if (deviceDelegate == null)
            {
                log.Error("Can not send command finished notification because the delegate is null");
                return;
            }

            log.Debug(string.Format("Sending command finished notification for message type {0}", inEvent.identifier));
            switch (inEvent.identifier)
            {

                case EventIdentifier.FlashLEDRequest:
                    deviceDelegate.flashLEDSent();
                    break;
                case EventIdentifier.ToggleLEDRequest:
                    deviceDelegate.ledControlSent();
                    break;
                case EventIdentifier.UpdateOutputRequest:
                    deviceDelegate.updateOutputSent((UpdateOutputEvent)inEvent);
                    break;
                default:
                    break;
            }
        }










        /// <summary>
        /// Spins off all the sensor threads
        /// </summary>
        private void startSensorThreads()
        {
            _thermistorVoltageThread = new Thread(this.queryThermistorVoltage);
            _thermistorVoltageThread.Name = "thermistorVoltageThread";
            _thermistorVoltageThread.Start();


            _flowmeterCounterThread = new Thread(this.queryFlowmeterCounter);
            _flowmeterCounterThread.Name = "flowmeterCounterThread";
            _flowmeterCounterThread.Start();


            _pressureSensorThread = new Thread(this.queryPressureSensorVoltage);
            _pressureSensorThread.Name = "pressureSensorThread";
            _pressureSensorThread.Start();
        }



        /// <summary>
        /// Takes the data received from the device and converts it into a valid 
        /// voltage value
        /// </summary>
        /// <param name="inData">The data received from the device</param>
        /// <returns></returns>
        private double getAnalogPinVoltage(byte[] inData)
        {
            if (inData.Length != 2)
            {
                log.Error(string.Format("Expected inData to have length 2, instead it has length {0}", inData.Length));
                return 0;
            }

            //!@#GET RID OF MAJIC NUMBERS
            byte firstByte = inData[0];
            byte secondByte = inData[1];
            int combinedData = (int)secondByte << 8;
            combinedData |= firstByte;
            double temp = (double)combinedData / 1020;
            double voltage = temp * REF_VOLTAGE;
            return voltage;
        }


        /// <summary>
        /// Takes in a voltage and uses that to compute the resistance of the termistor
        /// It uses the know values of the voltage divider to computer this
        /// </summary>
        /// <param name="voltage">Coltage coming out of the voltage divider</param>
        /// <returns>The resistance of the thermistor</returns>
        private double getThermistorResistance(double inVoltage)
        {
            double numerator = VOLTAGE_DIVIDER_RESISTANCE * REF_VOLTAGE;
            double divide = numerator/ inVoltage;
            double resistance = divide - VOLTAGE_DIVIDER_RESISTANCE;
            return resistance;
        }


        /// <summary>
        /// Takes the resistance of the thermistor and returns the temp of the thermistor 
        /// in C
        /// User equation 1/T2 = 1/T1 - ((ln(R1/R2))/B)
        /// where T2 = output temp in kelvin
        /// T1 = Reference temp in kelvin (25C)
        /// R1 = Resistance at reference temp
        /// R2 = measured resistance 
        /// B = Thermistor constant (3950 for our thermistor)
        /// </summary>
        /// <param name="resistance">The resistance of the thermistor</param>
        /// <returns>The temp of the thermistor in C</returns>
        private double getTempInC(double inResistance)
        {
            double natLog = Math.Log(THERMISTOR_REF_RESISTANCE / inResistance);
            double divide = natLog / THERMISTOR_B_VALUE;
            double subtract = (1 / THERMISTOR_REF_TEMP_K) - divide;
            double invert = 1 / subtract;
            double tempC = invert - 273.15;
            log.Debug(string.Format("Current heater temp = {0}", tempC));
            return tempC;
        }




        /// <summary>
        /// Handles the rolling average of flow meter counter events and returns the new flow rate
        /// in ml per second
        /// 
        /// It handles the first value into the queue, then handles filling up the queue, then all subsiquent values
        /// THe first value simple goes in
        /// As the queue is filling up the values are averaged only over the timespan that we have been sampling
        /// When the queue is full then we average over all the time represented by the queue
        /// The the flow stops we imediatly stop entering values into the queue.
        /// </summary>
        /// <param name="inEvent">The event associated with this information</param>
        /// <param name="newValue">The new counter value</param>
        private void handleNewFlowmeterCounterValue(Event inEvent, int newValue)
        {
            int frontValue = 0;
            double secTimeInterval = 0;
            int lastValue = 0;
            double newFlowValue = 0;
            if (_flowCounterQueue.Count == 0)
            {
                _flowCounterQueue.Enqueue(newValue);
                return;
            }
            else if (_flowCounterQueue.Count < MAX_NUMBER_FLOWMETER_QUEUE_ELEMENTS)
            {
                frontValue = _flowCounterQueue.Peek();
                secTimeInterval = _flowCounterQueue.Count * (MS_BETWEEN_FLOWMETER_QUERIES / 1000.0);
            }
            else
            {
                frontValue = _flowCounterQueue.Dequeue();
                secTimeInterval = MAX_NUMBER_FLOWMETER_QUEUE_ELEMENTS * (MS_BETWEEN_FLOWMETER_QUERIES / 1000.0);
            }
            lastValue = _flowCounterQueue.ToArray()[_flowCounterQueue.Count - 1];
            if (lastValue == newValue)
            {
                newFlowValue = 0;
            }
            else
            {
                _flowCounterQueue.Enqueue(newValue);
                int different = getCounterDifference(newValue, frontValue);
                double numberMl = different * ML_OF_WATER_PER_PULSE;
                double rateMlPerSec = numberMl / secTimeInterval;
                newFlowValue = rateMlPerSec;
            }
            flowLog.Info(string.Format("{0}", newFlowValue));
            if (deviceDelegate != null)
            {
                deviceDelegate.newFlowmeterData(inEvent, newFlowValue);
            }
        }




        /// <summary>
        /// Get the difference between 2 numbers in a roleover safe way
        /// </summary>
        /// <param name="toSubtractFrom">in the statement a - b, this is a</param>
        /// <param name="toSubtract">in the statement a - b, this is b</param>
        /// <returns></returns>
        private int getCounterDifference(int toSubtractFrom, int toSubtract)
        {
            int toReturn = 0;
            //Handle int roleover
            if (toSubtractFrom < toSubtract)
            {
                int dif = Int32.MaxValue - toSubtract;
                dif++;
                toReturn = dif + toSubtractFrom;
            }
            else
            {
                toReturn = toSubtractFrom - toSubtract;
            }
            return toReturn;
        }



        /// <summary>
        /// Helper message to log all the elements in a byte array
        /// Used for debugging
        /// </summary>
        /// <param name="byteArray">The array to log the elements of</param>
        private void logBytesArray(byte[] byteArray)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var b in byteArray)
            {
                sb.Append(string.Format("{0}, ", b));
            }
            log.Debug(string.Format("Bytes array : {0}", sb.ToString()));
        }




        //******************************************************** Sensor Query Threads ************************************************************************//
        /// <summary>
        /// Continues to query the thermistor to find the temperature until the thread is told to stop
        /// </summary>
        private void queryThermistorVoltage()
        {
            while (!_shouldStop)
            {
                queryAnalogChannel(new QueryAnalogInputEvent(AnalogPins.Thermistor_AN5));
                Thread.Sleep(MS_BETWEEN_THERMISTOR_QUERYS);
            }
        }



        /// <summary>
        /// Continuously querier the flowmeter counter to find the flow rate until the 
        /// thread is told to stop
        /// </summary>
        private void queryFlowmeterCounter()
        {
            while (!_shouldStop)
            {
                queryCounter(new QueryCounterEvent(CounterPins.FlowMeter_RB6));
                Thread.Sleep(MS_BETWEEN_FLOWMETER_QUERIES);
            }
        }


        /// <summary>
        /// Continuously queries the pressure sensor voltage
        /// </summary>
        private void queryPressureSensorVoltage()
        {
            while (!_shouldStop)
            {
                queryAnalogChannel(new QueryAnalogInputEvent(AnalogPins.Pressure_AN4));
                Thread.Sleep(MS_BETWEEN_PRESSURE_QUERIES);
            }
        }
    }
}
