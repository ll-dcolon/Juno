using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TestBed
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //Set up all the objects and delegates necesary to run the program
            PhysicalLayer physicalLayer = new PhysicalLayer();
            LogicalLayer logicalLayer = new LogicalLayer(physicalLayer);
            physicalLayer.setDelegate(logicalLayer);
            UIHandle_LLSL uiHandle = new UIHandle_LLSL(logicalLayer);
            SequencerLayer sequencer = new SequencerLayer(uiHandle, logicalLayer);
            MainWindow wnd = new MainWindow();
            wnd.setDelegate(uiHandle);
            wnd.Show();
        }
    }
}
