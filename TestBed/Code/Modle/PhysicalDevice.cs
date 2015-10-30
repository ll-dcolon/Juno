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
    }






    public class PhysicalLayer
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //The port used to communicate with the serial device
        private SerialPort _serialPort;
        //The configuration object used for the device
        private DeviceConfig _deviceConfig;
        //Buffer used to hold incoming bytes until they can be parsed
        private ConcurrentQueue<string> _incommingStringQueue;
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

        //Threads used to continuously query sensors
        private Thread _thermistorVoltageThread;

        //The number of ms to sleep between queries.
        private const int MS_BETWEEN_QUERYS= 500;

        //Flag used to stop threads
        private volatile bool _shouldStop;


        /// <summary>
        /// Creates a new physical device object and attempt to open the serial port
        /// </summary>
        public PhysicalLayer(DeviceConfig inDeviceConfig)
        {
            log.Debug(string.Format("Creating physicalLayer with config:{0}", inDeviceConfig));
            _deviceConfig = inDeviceConfig;
            _lockDeviceComm = new object();
            _enqueueEvent = new AutoResetEvent(false);
            _incommingStringQueue = new ConcurrentQueue<string>();
            _canReceiveResponce = false;
            findSerialDevice();

            _shouldStop = false;
            _thermistorVoltageThread = new Thread(this.queryThermistorVoltage);
            _thermistorVoltageThread.Name = "thermistorVoltageThread";
            _thermistorVoltageThread.Start();
        }


        /// <summary>
        /// Destroys the physical layer
        /// </summary>
        ~PhysicalLayer()
        {
            log.Debug("Destroying physical layer");
            stopThreads();
        }


        /// <summary>
        /// Stops all the sensor monitoring threads
        /// </summary>
        private void stopThreads()
        {
            _shouldStop = true;
            _thermistorVoltageThread.Join();
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
            log.Debug("Setting the state of all the outputs");
            //RA4
            _serialPort.Write(buildMessage(new List<int> { 0x05, 0x35, 0x0E, 0x00, 0x01 }));
            //RB7
            _serialPort.Write(buildMessage(new List<int> { 0x05, 0x35, 0x12, 0x00, 0x01 }));
            //RB6
            _serialPort.Write(buildMessage(new List<int> { 0x05, 0x35, 0x13, 0x00, 0x01 }));
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


        //Wrappers around the sendMessage function.  A parameter to the sendMessage function are the actual bytes to send
        //This is where you can see what bytes im sending for what commands
        public void connectToDevice_PL()
        {
            log.Debug("Sending ping device message");
            sendMessage(DeviceMessageIdentifier.PingDevice, buildMessage(new List<int> { 0x02, 0x27 }));
        }

        public void flashLED_PL()
        {
            log.Debug("Sending flash led message");
            sendMessage(DeviceMessageIdentifier.FlashLED, buildMessage(new List<int> {0x02, 0x28 }));
        }

        public void turnLEDOn_PL()
        {
            log.Debug("Sending ledControl message to turn the led on");
            sendMessage(DeviceMessageIdentifier.LEDControl, buildMessage(new List<int> { 0x03, 0x29, 0x00 }));
        }

        public void turnLEDOff_PL()
        {
            log.Debug("Sending ledControl message to turn the led off");
            sendMessage(DeviceMessageIdentifier.LEDControl, buildMessage(new List<int> { 0x03, 0x29, 0x01 }));
        }

        public void setOutputState(DIOPins pinToSet, bool shouldBeHigh)
        {
            int newPinState;
            if (shouldBeHigh) newPinState = 0x01;
            else newPinState = 0x00;
            log.Debug(string.Format("Sending DigitalIOControl message to turn pin {0} to state {1}", (int)pinToSet, newPinState));
            sendMessage(DeviceMessageIdentifier.DigitalOControl, buildMessage(new List<int> { 0x05, 0x35, (int)pinToSet, 0x00, newPinState }));
        }

        public void queryAnalogChannel(AnalogPins pinToQuery)
        {
            log.Debug(string.Format("Querying Analog pin {0}", pinToQuery));
            sendMessage(DeviceMessageIdentifier.AnalogPinQuery, buildMessage(new List<int> {0x03, 0x50, (int)pinToQuery}));
        }


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
        private void sendMessage(DeviceMessageIdentifier inMessageIdentifier, string inMessageToSend)
        {
            log.Debug("Attempting to send message to device");
            if (_serialPort.IsOpen)
            {
                int numBytesInDeviceResponce = getResponceLength(inMessageIdentifier);
                log.Debug(string.Format("Expecting {0} bytes from the device as a responce", numBytesInDeviceResponce));
                lock(_lockDeviceComm)
                {
                    _incommingStringQueue = new ConcurrentQueue<string>();
                    _canReceiveResponce = true;
                    _serialPort.Write(inMessageToSend);
                    if (numBytesInDeviceResponce > 0)
                    {
                        string responce = "";
                        bool receivedResponce = waitForResponce(numBytesInDeviceResponce, ref responce);
                        log.Debug(string.Format("Received responce {0}", receivedResponce));
                        if (receivedResponce)
                        {
                            processResponce(inMessageIdentifier, responce);
                        }
                    }
                    else
                    {
                        log.Debug(string.Format("Sending command finished notification for command: {0}", inMessageIdentifier));
                        sendCommandFinishedNotification(inMessageIdentifier);
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
        private bool waitForResponce(int inNumBytesToReceivce, ref string outResponce)
        {
            log.Debug("Waiting for responce from device");
            int msToWait = 1000;
            if (_enqueueEvent.WaitOne(msToWait))
            {
                _incommingStringQueue.TryDequeue(out outResponce);
                log.Debug(string.Format("Received a responce from the device.  Responce: {0}", outResponce));
                if (outResponce.Length == inNumBytesToReceivce)
                {
                    log.Debug(string.Format("Received responce of correct length. {0}",outResponce));
                    return true;
                }
                else
                {
                    log.Error(string.Format("Received responce string:{0} - with length {1} but expected a string of length {2}", outResponce, outResponce.Length, inNumBytesToReceivce));
                    return false;
                }

            }
            else
            {
                log.Error(string.Format("Did not receive a responce in {0}ms", msToWait));
                return false;
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
        private void processResponce(DeviceMessageIdentifier inMessageIdentifier, string inResponce)
        {
            log.Debug(string.Format("Processing responce from message {0}", inMessageIdentifier));
            switch (inMessageIdentifier)
            {
                case DeviceMessageIdentifier.PingDevice:
                    if (inResponce.Length == 1 && inResponce.ToCharArray()[0] == (char)0x59)
                    {
                        log.Debug("Received correct value for pind message");
                        if (deviceDelegate != null)
                        {
                            log.Debug("Sending device connected to the delegate");
                            deviceDelegate.deviceConnected();
                        }
                    }
                    break;
                case DeviceMessageIdentifier.AnalogPinQuery:
                    Console.WriteLine(inResponce);
                    char first = inResponce[0];
                    char second = inResponce[1];
                    uint voltage = (uint)second << 8;
                    voltage |= first;
                    float fvoltage = (float)voltage;
                    Console.WriteLine("{0}", fvoltage);
                    Console.WriteLine("Wo?");
                    //!@#NEED TO DO
                    break;
                case DeviceMessageIdentifier.DigitalIControl:
                case DeviceMessageIdentifier.DigitalOControl:
                case DeviceMessageIdentifier.FlashLED:
                case DeviceMessageIdentifier.LEDControl:
                case DeviceMessageIdentifier.RelayControl:
                default:
                    log.Error(string.Format("Message type {0} does not expect a responce", inMessageIdentifier));
                    break;
            }
        }


        /// <summary>
        /// Determines the expected responce length given the message type
        /// </summary>
        /// <param name="inDeviceMessage">The type of message we are going to send</param>
        /// <returns>The number of bytes (as a string) we expect to receive from the device</returns>
        private int getResponceLength(DeviceMessageIdentifier inDeviceMessage)
        {
            int toReturn = 0;
            log.Debug(string.Format("getting responce length for message type {0}", inDeviceMessage));
            switch (inDeviceMessage)
            {
                case DeviceMessageIdentifier.PingDevice:
                case DeviceMessageIdentifier.DigitalIControl:
                    toReturn = 1;
                    break;
                case DeviceMessageIdentifier.AnalogPinQuery:
                    toReturn = 2;
                    break;
                case DeviceMessageIdentifier.FlashLED:
                case DeviceMessageIdentifier.LEDControl:
                case DeviceMessageIdentifier.RelayControl:
                case DeviceMessageIdentifier.DigitalOControl:
                    break;
                default:
                    log.Error(string.Format("Message type {0} has not been implemented", inDeviceMessage));
                    break;
            }
            log.Debug(string.Format("returning responce length:{0} for message type {1}", toReturn, inDeviceMessage));
            return toReturn;
        }


        /// <summary>
        /// Used for all commands that dont expect a responce from the device.  This lets the physical layer still
        /// notify the logical layer that the message has been successfuly sent.
        /// </summary>
        /// <param name="inDeviceMessage">The type of message we sent</param>
        public void sendCommandFinishedNotification(DeviceMessageIdentifier inDeviceMessage)
        {
            if (deviceDelegate == null)
            {
                log.Error("Can not send command finished notification because the delegate is null");
                return;
            }

            log.Debug(string.Format("Sending command finished notification for message type {0}", inDeviceMessage));
            switch (inDeviceMessage)
            {

                case DeviceMessageIdentifier.FlashLED:
                    deviceDelegate.flashLEDSent();
                    break;
                case DeviceMessageIdentifier.LEDControl:
                    deviceDelegate.ledControlSent();
                    break;
                case DeviceMessageIdentifier.RelayControl:
                case DeviceMessageIdentifier.DigitalIControl:
                case DeviceMessageIdentifier.DigitalOControl:
                    log.Error("Have not implemented these methods for device notifications");
                    break;
                default:
                    log.Error("This message should not need the device completed notification because it expects a responce from the device");
                    break;
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
            if (!_canReceiveResponce)
            {
                SerialPort port = (SerialPort)sender;
                log.Fatal(string.Format("Received responce from device when should not have.  Responce:{0}", port.ReadExisting()));
                throw new Exception();
            }
            try
            {
                SerialPort port = (SerialPort)sender;
                string read = port.ReadExisting();
                log.Debug(string.Format("Received string {0}", read));
                _incommingStringQueue.Enqueue(read);
                _enqueueEvent.Set();

            }
            catch (Exception exception)
            {
                log.Fatal("Received exception when trying to read from serial port", exception);
                throw exception;
            }
        }






        //******************************************************** Sensor Query Threads ************************************************************************//
        private void queryThermistorVoltage()
        {
            while (!_shouldStop)
            {
                queryAnalogChannel(AnalogPins.Thermistor_AN5);
                Thread.Sleep(MS_BETWEEN_QUERYS);
            }
        }
    }
}
