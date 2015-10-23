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
        
        private UIEventInterface uiDelegate;


        public MainWindow()
        {
            InitializeComponent();
        }


        public void setDelegate(UIEventInterface inDelegate)
        {
            uiDelegate = inDelegate;
        }


        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectUIEvent clickTarget = new ConnectUIEvent();
            uiDelegate.enqueueUIEvent(clickTarget);
        }

        private void flashLEDButton_Click(object sender, RoutedEventArgs e)
        {
            FlashLEDUIEvent clickTarget = new FlashLEDUIEvent();
            uiDelegate.enqueueUIEvent(clickTarget);
        }

        private void turnOnLEDButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeLEDStateUIEvent clickTarget = new ChangeLEDStateUIEvent(true);
            uiDelegate.enqueueUIEvent(clickTarget);
        }

        private void turnOffLEDButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeLEDStateUIEvent clickTarget = new ChangeLEDStateUIEvent(false);
            uiDelegate.enqueueUIEvent(clickTarget);
        }

        private void toggleLEDButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleLEDUIEvent clickTarget = new ToggleLEDUIEvent();
            uiDelegate.enqueueUIEvent(clickTarget);
        }
    }
}
