using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace TestBed
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //config file location
        private string configFile = @"C:\vault\TestBed\Config\testBedConfig.json";


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            log.Info("This is the first log");
            //Get the config file
            SystemConfig systemConfig = new SystemConfig(configFile);

            //Set up all the objects and delegates necesary to run the program
            PhysicalLayer physicalLayer = new PhysicalLayer(systemConfig.getDeviceConfig());
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
