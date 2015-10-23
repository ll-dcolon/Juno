using System;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Used to let another object know when UI events happen        
        private UIEventInterface _uiDelegate;

        public MainWindow()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Sets the UI delegate to the supplied object
        /// </summary>
        /// <param name="inDelegate"></param>
        public void setDelegate(UIEventInterface inDelegate)
        {
            _uiDelegate = inDelegate;
        }






        //Methods called when various buttons are clicked
        //General strategy is enqueue the event with the delegate and then get 
        //back to listening for events
        /************************************************************************************************************************/
        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectUIEvent clickTarget = new ConnectUIEvent();
            if(_uiDelegate != null)_uiDelegate.enqueueUIEvent(clickTarget);
        }

        private void flashLEDButton_Click(object sender, RoutedEventArgs e)
        {
            FlashLEDUIEvent clickTarget = new FlashLEDUIEvent();
            if (_uiDelegate != null) _uiDelegate.enqueueUIEvent(clickTarget);
        }

        private void turnOnLEDButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeLEDStateUIEvent clickTarget = new ChangeLEDStateUIEvent(true);
            if (_uiDelegate != null) _uiDelegate.enqueueUIEvent(clickTarget);
        }

        private void turnOffLEDButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeLEDStateUIEvent clickTarget = new ChangeLEDStateUIEvent(false);
            if (_uiDelegate != null) _uiDelegate.enqueueUIEvent(clickTarget);
        }

        private void toggleLEDButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleLEDUIEvent clickTarget = new ToggleLEDUIEvent();
            if (_uiDelegate != null) _uiDelegate.enqueueUIEvent(clickTarget);
        }


        //Uses the combo box to determine what output you want to toggle
        private void toggleOutput_Click(object sender, RoutedEventArgs e)
        {
            String targetOutput = dioSelector.Text;
            DIOPins pinToToggle;
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
                    Console.WriteLine("Do not recognize the selected IO in the combo box");
                    return;
            }
            ToggleOutputUIEvent clickTarget = new ToggleOutputUIEvent(pinToToggle);
            if (_uiDelegate != null) _uiDelegate.enqueueUIEvent(clickTarget);
        }
    }
}
