using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace TestBed
{
    /// <summary>
    /// Any class which wants to receive events from the UI must
    /// implement this interface
    /// </summary>
    public interface UIEventInterface
    {
        /// <summary>
        /// Receives a new UI event and adds it to the event processing queue
        /// </summary>
        /// <param name="inEvent">The new UIEvent</param>
        void enqueueUIEvent(UIEvent inEvent);
    }





    /// <summary>
    /// This class is responsible for catching events from the UI in a thread safe queue
    /// and then using a new thread to process the queue
    /// </summary>
    class UIHandle_LLSL : UIEventInterface
    {
        // Thread same queue to hold all UIEvents
        private ConcurrentQueue<UIEvent> _eventQueue;
        //Thread that processes the queue
        private Thread _queueProcessingThread;
        //Flag to stop the processing thread;
        private volatile bool _shouldStop;

        //Used to notify the pprocessing thread when there is a new element to process
        private AutoResetEvent _enqueueEvent;

        //Logical layer used to send messages to the device
        private LogicalLayer _logicalLayer;
        



        /// <summary>
        /// Create the new UIHandle_LLSL and give it a new queue
        /// </summary>
        public UIHandle_LLSL(LogicalLayer inLogicalLayer)
        {
            _eventQueue = new ConcurrentQueue<UIEvent>();
            _queueProcessingThread = new Thread(this.processQueue);
            _queueProcessingThread.Name = "queueProcessingThread";
            _queueProcessingThread.Start();
            _shouldStop= false;

            _enqueueEvent = new AutoResetEvent(false);

            _logicalLayer = inLogicalLayer;
        }

        /// <summary>
        /// Cleans up the object by requesting the processing thread stop
        /// </summary>
        ~UIHandle_LLSL()
        {
            this.requestStop();
        }

        /// <summary>
        /// Stops the processing thread and merges it back into the main thread
        /// </summary>
        public void requestStop()
        {
            _shouldStop = true;
            _queueProcessingThread.Join();
        }





        //*************************** UIEventInterface Start **************************************************//
        void UIEventInterface.enqueueUIEvent(UIEvent inEvent)
        {
            _eventQueue.Enqueue(inEvent);
            _enqueueEvent.Set();
        }
        //*************************** UIEventInterface End **************************************************//







        /// <summary>
        /// Method which the processing thread will execute in order to
        /// process the ui event queue.  
        /// 
        /// Will stop when the _shouldStop flag is set
        /// </summary>
        private void processQueue()
        {
            while (!_shouldStop)
            {
                //Get out the newest event
                _enqueueEvent.WaitOne();
                if (_eventQueue.Count() < 1) continue;
                UIEvent newEvent;
                bool dequeueSuccessful = _eventQueue.TryDequeue(out newEvent);
                if (!dequeueSuccessful) continue;

                //Decide what to do with the event
                switch (newEvent.getEventIdentifier())
                {
                    case UIEventIdentifier.GenericEvent:
                        break;
                    case UIEventIdentifier.ConnectClicked:
                        handleConnectClickedEvent();
                        break;
                    case UIEventIdentifier.ToggleLEDClicked:
                        handleToggleLEDClickedEvent();
                        break;
                    case UIEventIdentifier.FlashLEDClicked:
                        handleFlashLEDClickedEvent();
                        break;
                    case UIEventIdentifier.ChangeLEDStateClicked:
                        ChangeLEDStateUIEvent changeLEDEvent = (ChangeLEDStateUIEvent)newEvent;

                        handleChangeLEDStateClickedEvent((ChangeLEDStateUIEvent)newEvent._isHigh);
                        break;
                    default:
                        break;
                }
            }
        }




        private void handleConnectClickedEvent(){ _logicalLayer.connectToDevice_LL(); }
        private void handleToggleLEDClickedEvent(){ _logicalLayer.toggleLED_LL(); }
        private void handleFlashLEDClickedEvent() { _logicalLayer.flashLED_LL(); }
        private void handleCHangeLEDStateClickedEvent(bool isHigh) { _logicalLayer.turnLEDOff_LL(); }







        //*************************************** Sequencer Methods **********************************************//
    }
}
