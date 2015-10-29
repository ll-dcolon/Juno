using System.Collections.Concurrent;
using System.Linq;
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // Thread safe queue to hold all UIEvents
        private ConcurrentQueue<UIEvent> _eventQueue;
        //Thread that processes the queue
        private Thread _queueProcessingThread;
        //Flag to stop the processing thread;
        private volatile bool _shouldStop;

        //Used to notify the pprocessing thread when there is a new element to process
        private AutoResetEvent _enqueueEvent;

        //Notifies the sequencer that it should start the test sequence
        private AutoResetEvent _startTestSequenceEvent;

        //Logical layer used to send messages to the device
        private LogicalLayer _logicalLayer;




        /// <summary>
        /// Create the new UIHandle_LLSL and give it a new queue
        /// </summary>
        /// <param name="inLogicalLayer">Logical Layer, used to send UI messages to the device</param>
        public UIHandle_LLSL(LogicalLayer inLogicalLayer)
        {
            log.Debug(string.Format("Creating UIHandle__LLSL with logical layer: {0}", inLogicalLayer));
            _eventQueue = new ConcurrentQueue<UIEvent>();
            _queueProcessingThread = new Thread(this.processQueue);
            _queueProcessingThread.Name = "queueProcessingThread";
            _queueProcessingThread.Start();
            _shouldStop= false;

            _enqueueEvent = new AutoResetEvent(false);
            _startTestSequenceEvent = new AutoResetEvent(false);

            _logicalLayer = inLogicalLayer;
        }

        /// <summary>
        /// Cleans up the object by requesting the processing thread stop
        /// </summary>
        ~UIHandle_LLSL()
        {
            log.Debug(string.Format("Destroying UIHandle_LLSL: {0}", this));
            this.requestStop();
        }

        /// <summary>
        /// Stops the processing thread and merges it back into the main thread
        /// </summary>
        public void requestStop()
        {
            log.Info(string.Format("UIHandle_LLSL:{} requesting stop", this));
            _shouldStop = true;
            _queueProcessingThread.Join();
        }





        //*************************** UIEventInterface Start **************************************************//
        void UIEventInterface.enqueueUIEvent(UIEvent inEvent)
        {
            log.Info(string.Format("Enqueueing UIEvent {0}", inEvent.getEventIdentifier()));
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
                log.Debug("Waiting for new event in UIHandle_LLSL processing queue");
                _enqueueEvent.WaitOne();
                if (_eventQueue.Count() < 1) continue;
                log.Debug("UIHandle_LLSL processing thread received item enqueued event");
                UIEvent newEvent;
                bool dequeueSuccessful = _eventQueue.TryDequeue(out newEvent);
                if (!dequeueSuccessful) continue;
                log.Info(string.Format("UIHandle_LLSL processing queue dequeued event {0}", newEvent.getEventIdentifier()));

                //Decide what to do with the event
                switch (newEvent.getEventIdentifier())
                {
                    case UIEventIdentifier.GenericEvent:
                        break;
                    case UIEventIdentifier.ConnectClicked:
                        log.Debug("Handling ConnectClicked ui event");
                        handleConnectClickedEvent();
                        break;
                    case UIEventIdentifier.ToggleLEDClicked:
                        log.Debug("Handling toggleLED ui event");
                        handleToggleLEDClickedEvent();
                        break;
                    case UIEventIdentifier.FlashLEDClicked:
                        log.Debug("Handling flashLED ui event");
                        handleFlashLEDClickedEvent();
                        break;
                    case UIEventIdentifier.ChangeLEDStateClicked:
                        ChangeLEDStateUIEvent changeLEDEvent = (ChangeLEDStateUIEvent)newEvent;
                        bool ledIsHigh = changeLEDEvent._isHigh;
                        log.Debug(string.Format("Handling changeLED (setting high: {0}) ui event", changeLEDEvent._isHigh));
                        handleChangeLEDStateClickedEvent(ledIsHigh);
                        break;
                    case UIEventIdentifier.ToggleOutputClicked:
                        ToggleOutputUIEvent toggleOutputEvent = (ToggleOutputUIEvent)newEvent;
                        DIOPins pinToToggle = toggleOutputEvent._pinToToggle;
                        log.Debug(string.Format("Handling toggleOutput (toggle pin: {0}) ui event", toggleOutputEvent._pinToToggle));
                        handleToggleOutputClickedEvent(pinToToggle);
                        break;
                    case UIEventIdentifier.StartTestSequencerClicked:
                        log.Debug("Handling startTestSequence ui event");
                        handleStartTestSequencerClickedEvent();
                        break;
                    default:
                        break;
                }
            }
        }

        
        //Handle the events by passing calls onto the logical layer
        private void handleConnectClickedEvent(){ _logicalLayer.connectToDevice_LL(); }
        private void handleToggleLEDClickedEvent(){ _logicalLayer.toggleLED_LL(); }
        private void handleFlashLEDClickedEvent() { _logicalLayer.flashLED_LL(); }
        private void handleChangeLEDStateClickedEvent(bool isHigh) { _logicalLayer.changeLEDState(isHigh); }
        private void handleToggleOutputClickedEvent(DIOPins pinToToggle) { _logicalLayer.toggleOutput(pinToToggle); }
        private void handleStartTestSequencerClickedEvent() { _startTestSequenceEvent.Set(); }







        //*************************************** Sequencer Methods **********************************************//
        public bool waitForStartTestSequenceClicked(int inMSToWait = 0)
        {
            log.Debug("Waiting for start test to be clicked");
            if (inMSToWait == 0){_startTestSequenceEvent.WaitOne(); return true; }
            else{  return _startTestSequenceEvent.WaitOne(inMSToWait);}
        }

    }
}
