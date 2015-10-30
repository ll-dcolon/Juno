﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        /// Updates the voltage value on the UI
        /// </summary>
        /// <param name="inVoltageValue">The new voltage value to display</param>
        void updateVoltageValue(float inVoltageValue);
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
            voltageValue.Text = "????";
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
                case "Heater (RA4) (White)":
                    pinToToggle = DIOPins.Heater_RA4;
                    break;
                case "Air Pump (RB6) (Green)":
                    pinToToggle = DIOPins.AirPump_RB6;
                    break;
                case "Water Pump (RB7) (Red)":
                    pinToToggle = DIOPins.WaterPump_RB7;
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





        //******************************************************* UpdateUIInterface Methods **********************************************************//
        public void updateVoltageValue(float inNewVoltage)
        {

        }
    }
}
