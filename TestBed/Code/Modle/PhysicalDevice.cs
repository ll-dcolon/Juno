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

        //The port used to communicate with the serial device
        private SerialPort _serialPort;
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


        /// <summary>
        /// Creates a new physical device object and attempt to open the serial port
        /// </summary>
        public PhysicalLayer()
        {
            _lockDeviceComm = new object();
            _enqueueEvent = new AutoResetEvent(false);
            _incommingStringQueue = new ConcurrentQueue<string>();
            _canReceiveResponce = false;
            findSerialDevice();
        }


        /// <summary>
        /// Attempts to open the serial port and communicate with the device
        /// 
        /// Current defaults are set to COM3 and 9600 BaudRate
        /// </summary>
        private void findSerialDevice()
        {

            _serialPort = new SerialPort();
            _serialPort.PortName = "COM3";
            _serialPort.BaudRate = 9600;  //!@#Make these not hard coded.  Config file would be best
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            try
            {
                _serialPort.Open();
            }
            catch (Exception)
            {
                Console.WriteLine("Could not connect to the device");
                throw;
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
            deviceDelegate = inDelegate;
        }


        //Wrappers around the sendMessage function.  A parameter to the sendMessage function are the actual bytes to send
        //This is where you can see what bytes im sending for what commands
        public void connectToDevice_PL() { sendMessage(DeviceMessageIdentifier.PingDevice, buildMessage(new List<int> { 0x02, 0x27 })); }
        public void flashLED_PL() { sendMessage(DeviceMessageIdentifier.FlashLED, buildMessage(new List<int> {0x02, 0x28 })); }
        public void turnLEDOn_PL() { sendMessage(DeviceMessageIdentifier.LEDControl, buildMessage(new List<int> { 0x03, 0x29, 0x00 })); }
        public void turnLEDOff_PL() { sendMessage(DeviceMessageIdentifier.LEDControl, buildMessage(new List<int> { 0x03, 0x29, 0x01 })); }
        public void setOutputState(DIOPins pinToSet, bool shouldBeHigh)
        {
            int newPinState;
            if (shouldBeHigh) newPinState = 0x01;
            else newPinState = 0x00;
            sendMessage(DeviceMessageIdentifier.DigitalOControl, buildMessage(new List<int> { 0x05, 0x35, (int)pinToSet, 0x00, newPinState }));
        }


        /// <summary>
        /// Helper method used to combine the byte list into a string to send to sendMessage function
        /// </summary>
        /// <param name="bytes">List<int> of the bytes to send</param>
        /// <returns>A string version consisting of the character representation of the bytes sent in</returns>
        private string buildMessage(List<int> bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int item in bytes)
            {
                sb.Append((char)item);
            }
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
            if (_serialPort.IsOpen)
            {
                int numBytesInDeviceResponce = getResponceLength(inMessageIdentifier);
                lock(_lockDeviceComm)
                {
                    _incommingStringQueue = new ConcurrentQueue<string>();
                    _canReceiveResponce = true;
                    _serialPort.Write(inMessageToSend);
                    if (numBytesInDeviceResponce > 0)
                    {
                        string responce = "";
                        bool receivedResponce = waitForResponce(numBytesInDeviceResponce, ref responce);
                        if (receivedResponce)
                        {
                            processResponce(inMessageIdentifier, responce);
                        }
                    }
                    else
                    {
                        sendCommandFinishedNotification(inMessageIdentifier);
                        _canReceiveResponce = false;
                    }
                }

            }
            else
            {
                //!@#Throw and exception
                Console.WriteLine("ERROR : Can not send message when serial port is not open");
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
                if (_enqueueEvent.WaitOne(1000))
                {
                    _incommingStringQueue.TryDequeue(out outResponce);
                    if (outResponce.Length == inNumBytesToReceivce)
                    {
                        Console.WriteLine("Received responce of correct length.  " + outResponce);
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Received string " + outResponce + "But needed responce of length " + inNumBytesToReceivce);
                        return false;
                    }

                }
                else
                {
                    Console.WriteLine("Did not receive a responce after .5 seconds");
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
            switch (inMessageIdentifier)
            {
                case DeviceMessageIdentifier.PingDevice:
                    if (inResponce.Length == 1 && inResponce.ToCharArray()[0] == (char)0x59) { if (deviceDelegate != null) deviceDelegate.deviceConnected(); }
                    break;
                case DeviceMessageIdentifier.DigitalIControl:
                case DeviceMessageIdentifier.DigitalOControl:
                case DeviceMessageIdentifier.FlashLED:
                case DeviceMessageIdentifier.LEDControl:
                case DeviceMessageIdentifier.RelayControl:
                default:
                    Console.WriteLine("Message type " + inMessageIdentifier + "does not expect a responce");
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
            switch (inDeviceMessage)
            {
                case DeviceMessageIdentifier.PingDevice:
                    return 1;
                case DeviceMessageIdentifier.FlashLED:
                    return 0;
                case DeviceMessageIdentifier.LEDControl:
                    return 0;
                case DeviceMessageIdentifier.RelayControl:
                    return 0;
                case DeviceMessageIdentifier.DigitalIControl:
                    return 1;
                case DeviceMessageIdentifier.DigitalOControl:
                    return 0;
                default:
                    Console.WriteLine("Error : That message has not been implemented!!");
                    return 0;
            }
        }


        /// <summary>
        /// Used for all commands that dont expect a responce from the device.  This lets the physical layer still
        /// notify the logical layer that the message has been successfuly sent.
        /// </summary>
        /// <param name="inDeviceMessage">The type of message we sent</param>
        public void sendCommandFinishedNotification(DeviceMessageIdentifier inDeviceMessage)
        {
            switch (inDeviceMessage)
            {

                case DeviceMessageIdentifier.FlashLED:
                    if (deviceDelegate != null) deviceDelegate.flashLEDSent();
                    break;
                case DeviceMessageIdentifier.LEDControl:
                    if (deviceDelegate != null) deviceDelegate.ledControlSent();
                    break;
                case DeviceMessageIdentifier.RelayControl:
                case DeviceMessageIdentifier.DigitalIControl:
                case DeviceMessageIdentifier.DigitalOControl:
                    Console.WriteLine("Not implemented these delegate methods yet");
                    break;
                case DeviceMessageIdentifier.PingDevice:
                default:
                    Console.WriteLine("This message sends a responce and should not need these delegate messages");
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
                Console.WriteLine("ERROR - Received responce when should not have");
            }
            try
            {
                SerialPort port = (SerialPort)sender;
                string read = port.ReadExisting();
                Console.Write("Receive <<- " + read + "\n");
                _incommingStringQueue.Enqueue(read);
                _enqueueEvent.Set();

            }
            catch (Exception)
            {
                Console.Write("ERROR");
            }
        }

    }
}
