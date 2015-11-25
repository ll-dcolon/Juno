﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = @"C:\vault\TestBed\Config\testBedLoggerConfig.xml", Watch = true)]

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


        private PhysicalLayer _physicalLayer;
        private SequencerLayer _sequencerLayer;
        private UIHandle_LLSL _uiHandle;


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            log.Info("Application Starting");
            log.Info(String.Format("Using config file : {0}", configFile));
            log.Info("----------------------------------------------------");

            //Get the config file
            log.Debug("Setup configuration");
            SystemConfig systemConfig = new SystemConfig(configFile);

            //Set up all the objects and delegates necesary to run the program
            log.Debug("Setup objects");
            _physicalLayer = new PhysicalLayer(systemConfig.getDeviceConfig());
            LogicalLayer logicalLayer = new LogicalLayer(_physicalLayer);
            _physicalLayer.setDelegate(logicalLayer);
            _uiHandle = new UIHandle_LLSL(logicalLayer);
            _sequencerLayer = new SequencerLayer(_uiHandle, logicalLayer, systemConfig.getSequencerConfig());

            //Setup main window
            log.Debug("Setup main window");
            MainWindow wnd = new MainWindow();
            logicalLayer.setUIDelegate(wnd);
            _sequencerLayer.setUIDelegate(wnd);
            wnd.setDelegate(_uiHandle);
            wnd.Show();
        }


        protected override void OnExit(ExitEventArgs e)
        {
            _physicalLayer.turnOffOutputs();
            _sequencerLayer.requestStop();
            //_physicalLayer.stopThreads();
            _uiHandle.requestStop();
            Application.Current.Shutdown();
        }
    }

}
