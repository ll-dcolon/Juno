using System;
using System.Runtime;
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
        void deviceConnected();
        void flashLEDSent();
        void ledControlSent();
    }






    class PhysicalDevice
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



        public PhysicalDevice()
        {
            _lockDeviceComm = new object();
            _enqueueEvent = new AutoResetEvent(false);
            _incommingStringQueue = new ConcurrentQueue<string>();
            _canReceiveResponce = false;
            findSerialDevice();
        }


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
        }



        public void setDelegate(PhysicalDeviceInterface inDelegate)
        {
            deviceDelegate = inDelegate;
        }


        public void connectToDevice_PL() { sendMessage(DeviceMessageIdentifier.PingDevice, buildMessage(new List<int> { 0x02, 0x27 })); }
        public void flashLED_PL() { sendMessage(DeviceMessageIdentifier.FlashLED, buildMessage(new List<int> {0x02, 0x28 })); }
        public void turnLEDOn_PL() { sendMessage(DeviceMessageIdentifier.LEDControl, buildMessage(new List<int> { 0x03, 0x29, 0x00 })); }
        public void turnLEDOff_PL() { sendMessage(DeviceMessageIdentifier.LEDControl, buildMessage(new List<int> { 0x03, 0x29, 0x01 })); }



        private string buildMessage(List<int> bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int item in bytes)
            {
                sb.Append((char)item);
            }
            return sb.ToString();
        }






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


        private bool waitForResponce(int inNumBytesToReceivce, ref string outResponce)
        {
            while (true)
            {
                if (_enqueueEvent.WaitOne(500))
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
                    Console.WriteLine("Did not receive a responce after .2 seconds");
                    return false;
                }
            }
        }


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
