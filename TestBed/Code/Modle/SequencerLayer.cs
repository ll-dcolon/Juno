using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;



namespace TestBed
{
    class SequencerLayer
    {
        private UIHandle_LLSL _uiHandler;
        private LogicalLayer _logicalLayer;



        //Flag to stop the processing thread;
        private volatile bool _shouldStop;




        public SequencerLayer(UIHandle_LLSL inUIHandler, LogicalLayer inLogicalLayer)
        {
            _uiHandler = inUIHandler;
            _logicalLayer = inLogicalLayer;
            _shouldStop = false;
            startSequencerThreads();
        }

        /// <summary>
        /// Cleans up the object by requesting the processing thread stop
        /// </summary>
        ~SequencerLayer()
        {
            this.requestStop();
        }

        private void startSequencerThreads()
        {

        }

        /// <summary>
        /// Stops the processing thread and merges it back into the main thread
        /// </summary>
        public void requestStop()
        {
            _shouldStop = true;
        }



    }
}
