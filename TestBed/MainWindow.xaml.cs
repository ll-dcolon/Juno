﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestBed
{
    /// <summary>
    /// Interface which will be implemented by any class that is allowed to update the UI
    /// Other objects will delegate the objects which implement this interface if they
    /// want to update the UI
    /// </summary>
    public interface UpdateUIInterface
    {
        /// <summary>
        /// Updates the temp value on the UI
        /// </summary>
        /// <param name="inVoltageValue">The new temp value to display, in C</param>
        void updateTempValue(double inTempValue);


        /// <summary>
        /// Tells the UI to update the connection status of the device
        /// </summary>
        /// <param name="inIsConnected">True if the device is connected, false if it is not</param>
        void updateConnectionStatus(bool inIsConnected);


        void updateOutputState(UpdateOutputUIEvent inEvent);
    }






    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, UpdateUIInterface
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        //Used to let another object know when UI events happen        
        private UIEventInterface _uiDelegate;





        public MainWindow()
        {
            log.Info("Starting Main Window");
            InitializeComponent();
            InitializeScreenItems();
        }


        /// <summary>
        /// Sets up the screen items to be the desired style and to
        /// have the correct values
        /// </summary>
        private void InitializeScreenItems()
        {
        }


        /// <summary>
        /// Sets the UI delegate to the supplied object
        /// </summary>
        /// <param name="inDelegate"></param>
        public void setDelegate(UIEventInterface inDelegate)
        {
            log.Info(String.Format("Setting the MainWindows delegate: {0}", inDelegate));
            _uiDelegate = inDelegate;
        }






        //Methods called when various buttons are clicked
        //General strategy is enqueue the event with the delegate and then get 
        //back to listening for events
        /************************************************************************************************************************/
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Clicked the connect button");
            ConnectUIEvent clickTarget = new ConnectUIEvent();
            if (_uiDelegate != null)
            {
                log.Debug("Adding clickk event to processing queue");
                 _uiDelegate.enqueueUIEvent(clickTarget);
            }
        }

        private void flashLEDButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Clicked the flashLED button");
            FlashLEDUIEvent clickTarget = new FlashLEDUIEvent();
            if (_uiDelegate != null)
            {
                log.Debug("Adding the flashLED event to processing queue");
                _uiDelegate.enqueueUIEvent(clickTarget);
            }
        }

        private void turnOnLEDButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Clicked the turnOnLED button");
            ChangeLEDStateUIEvent clickTarget = new ChangeLEDStateUIEvent(true);
            if (_uiDelegate != null)
            {
                log.Debug("Adding the turnOnLED event to processing queue");
                _uiDelegate.enqueueUIEvent(clickTarget);
            }
        }

        private void turnOffLEDButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Clicked the turnOffLED button");
            ChangeLEDStateUIEvent clickTarget = new ChangeLEDStateUIEvent(false);
            if (_uiDelegate != null)
            {
                log.Debug("Adding the turnOffLED event to processing queue");
                _uiDelegate.enqueueUIEvent(clickTarget);
            }
        }

        private void toggleLEDButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Clicked the toggleLED button");
            ToggleLEDUIEvent clickTarget = new ToggleLEDUIEvent();
            if (_uiDelegate != null)
            {
                log.Debug("Adding the toggleLED event to processing queue");
                _uiDelegate.enqueueUIEvent(clickTarget);
            }
        }


        //Uses the combo box to determine what output you want to toggle
        private void toggleOutput_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Clicked the toggleOutput button");
            String targetOutput = dioSelector.Text;
            DIOPins pinToToggle;
            log.Info(String.Format("{0} selected when toggleOutput button clicked", targetOutput));
            switch (targetOutput)
            {
                case "Heater (AN1) (Red)":
                    pinToToggle = DIOPins.Heater_AN1;
                    break;
                case "Air Pump (AN0) (White)":
                    pinToToggle = DIOPins.AirPump_AN0;
                    break;
                case "Water Pump (AN2) (Green)":
                    pinToToggle = DIOPins.WaterPump_AN2;
                    break;
                default:
                    log.Error(String.Format("Did not recognize selected port to toggle : {0}", targetOutput));
                    return;
            }
            ToggleOutputUIEvent clickTarget = new ToggleOutputUIEvent(pinToToggle);
            if (_uiDelegate != null)
            {
                log.Debug("Adding the toggleOutput event to the processing queue");
                _uiDelegate.enqueueUIEvent(clickTarget);
            }
        }


        /// <summary>
        /// Tells the UIHandle to start the test sequencer
        /// </summary>
        private void startTestSequencerButton_Click(object sender, RoutedEventArgs e)
        {
            log.Info("Clicked the startTestSequence button");
            StartTestSequencerUIEvent clickTarget = new StartTestSequencerUIEvent();
            if (_uiDelegate != null)
            {
                log.Debug("Adding the startTestSequence event to the processing queue");
                _uiDelegate.enqueueUIEvent(clickTarget);
            }
        }


        private void heaterIOBox_Clicked(object sender, RoutedEventArgs e)
        {
            bool shouldSetHigh = !(bool)heaterIOBox.IsChecked;
            log.Info(string.Format("Clicked heater checkbox.  Checkbox.IsChecked = {0}", shouldSetHigh));
            changeOutput(DIOPins.Heater_AN1, shouldSetHigh);
        }

        private void airIOBox_Clicked(object sender, RoutedEventArgs e)
        {
            bool shouldSetHigh = !(bool)airIOBox.IsChecked;
            log.Info(string.Format("Clicked air checkbox.  Checkbox.IsChecked = {0}", shouldSetHigh));
            changeOutput(DIOPins.AirPump_AN0, shouldSetHigh);
        }

        private void waterIOBox_Clicked(object sender, RoutedEventArgs e)
        {
            bool shouldSetHigh = !(bool)waterIOBox.IsChecked;
            log.Info(string.Format("Clicked water checkbox.  Checkbox.IsChecked = {0}", shouldSetHigh));
            changeOutput(DIOPins.WaterPump_AN2, shouldSetHigh);
        }

        private void changeOutput(DIOPins pinToChange, bool shouldSetHigh)
        {
            UpdateOutputUIEvent clickTarget = new UpdateOutputUIEvent(pinToChange, shouldSetHigh);
            if (_uiDelegate != null)
            {
                log.Debug(string.Format("Adding UpdateOutputUIEvent (pin to set : {0} should set high : {1}) to UIProcessing queue", pinToChange, shouldSetHigh));
                _uiDelegate.enqueueUIEvent(clickTarget);
            }

        }




        //******************************************************* UpdateUIInterface Methods **********************************************************//
        public void updateTempValue(double inNewTemp)
        {
            string cValueString = string.Format("{0}", inNewTemp);
            double fValue = (inNewTemp * (9.0 / 5.0)) + 32;
            string fValueString = string.Format("{0}", fValue);

            string bothStrings = string.Format("{0}f {1}c", fValue, inNewTemp);
            Dispatcher.Invoke((Action)delegate(){ voltageValue.Text = fValueString; });
        }


        /// <summary>
        /// Sets all the ui elements to be enabled if we just connected to the device
        /// </summary>
        /// <param name="inNewConnectionStatus">Our new connection status</param>
        public void updateConnectionStatus(bool inNewConnectionStatus)
        {
            if (inNewConnectionStatus)
            {
                Dispatcher.Invoke((Action)delegate () 
                {
                    isConnected.IsChecked = true;
                    connectionWarning.Visibility = System.Windows.Visibility.Hidden;
                    flashLEDButton.IsEnabled = true;
                    toggleLEDButton.IsEnabled = true;
                    turnOffLEDButton.IsEnabled = true;
                    turnOnLEDButton.IsEnabled = true;
                    heaterIOBox.IsEnabled = true;
                    airIOBox.IsEnabled = true;
                    waterIOBox.IsEnabled = true;
                    startTestSequencerButton.IsEnabled = true;
                    toggleOutput.IsEnabled = true;
                });
            }
            else
            {
                Dispatcher.Invoke((Action)delegate () { isConnected.IsChecked = false; });
            }
        }



        public void updateOutputState(UpdateOutputUIEvent inEvent)
        {
            switch (inEvent._pinToUpdate)
            {
                case DIOPins.Heater_AN1:
                    if (inEvent._shouldBeHigh) { Dispatcher.Invoke((Action)delegate () { heaterIOBox.IsChecked = false; }); }
                    else { Dispatcher.Invoke((Action)delegate () { heaterIOBox.IsChecked = true; }); }
                    break;
                case DIOPins.AirPump_AN0:
                    if (inEvent._shouldBeHigh) { Dispatcher.Invoke((Action)delegate () { airIOBox.IsChecked = false; }); }
                    else { Dispatcher.Invoke((Action)delegate () { airIOBox.IsChecked = true; }); }
                    break;
                case DIOPins.WaterPump_AN2:
                    if (inEvent._shouldBeHigh) { Dispatcher.Invoke((Action)delegate () { waterIOBox.IsChecked = false; }); }
                    else { Dispatcher.Invoke((Action)delegate () { waterIOBox.IsChecked = true; }); }
                    break;
                default:
                    break;
            }
        }

    }
}
