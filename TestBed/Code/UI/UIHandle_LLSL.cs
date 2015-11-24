using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace TestBed
{
    /// <summary>
    /// Any class which wants to receive events from the UI must
    /// implement this interface
    /// </summary>
    public interface EventInterface
    {
        /// <summary>
        /// Receives a new UI event and adds it to the event processing queue
        /// </summary>
        /// <param name="inEvent">The new Event</param>
        void enqueueEvent(Event inEvent);
    }





    /// <summary>
    /// This class is responsible for catching events from the UI in a thread safe queue
    /// and then using a new thread to process the queue
    /// </summary>
    class UIHandle_LLSL : EventInterface
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // Thread safe queue to hold all Events
        private ConcurrentQueue<Event> _eventQueue;
        //Thread that processes the queue
        private Thread _queueProcessingThread;
        //Flag to stop the processing thread;
        private volatile bool _shouldStop;

        //Used to notify the pprocessing thread when there is a new element to process
        private AutoResetEvent _enqueueEvent;

        //Notifies the sequencer that it should start the test sequence
        private AutoResetEvent _startTestSequenceEvent;
        private AutoResetEvent _startTwoOzSequenceEvent;
        private AutoResetEvent _startFourOzSequenceEvent;
        private AutoResetEvent _startEightOzSequenceEvent;

        //Logical layer used to send messages to the device
        private LogicalLayer _logicalLayer;




        /// <summary>
        /// Create the new UIHandle_LLSL and give it a new queue
        /// </summary>
        /// <param name="inLogicalLayer">Logical Layer, used to send UI messages to the device</param>
        public UIHandle_LLSL(LogicalLayer inLogicalLayer)
        {
            log.Debug(string.Format("Creating UIHandle__LLSL with logical layer: {0}", inLogicalLayer));
            _eventQueue = new ConcurrentQueue<Event>();
            _queueProcessingThread = new Thread(this.processQueue);
            _queueProcessingThread.Name = "queueProcessingThread";
            _queueProcessingThread.Start();
            _shouldStop= false;

            _enqueueEvent = new AutoResetEvent(false);
            _startTestSequenceEvent = new AutoResetEvent(false);
            _startTwoOzSequenceEvent = new AutoResetEvent(false);
            _startFourOzSequenceEvent = new AutoResetEvent(false);
            _startEightOzSequenceEvent = new AutoResetEvent(false);

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





        //*************************** EventInterface Start **************************************************//
        void EventInterface.enqueueEvent(Event inEvent)
        {
            log.Info(string.Format("Enqueueing Event {0}", inEvent.getEventIdentifier()));
            _eventQueue.Enqueue(inEvent);
            _enqueueEvent.Set();
        }
        //*************************** EventInterface End **************************************************//







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
                Event newEvent;
                bool dequeueSuccessful = _eventQueue.TryDequeue(out newEvent);
                if (!dequeueSuccessful) continue;
                log.Info(string.Format("UIHandle_LLSL processing queue dequeued event {0}", newEvent.getEventIdentifier()));

                //Decide what to do with the event
                switch (newEvent.getEventIdentifier())
                {
                    case EventIdentifier.GenericEvent:
                        break;
                    case EventIdentifier.ConnectRequest:
                        log.Debug("Handling ConnectRequest ui event");
                        handleConnectEvent();
                        break;
                    case EventIdentifier.ToggleLEDRequest:
                        log.Debug("Handling toggleLED ui event");
                        handleToggleLEDEvent();
                        break;
                    case EventIdentifier.FlashLEDRequest:
                        log.Debug("Handling flashLED ui event");
                        handleFlashLEDEvent();
                        break;
                    case EventIdentifier.ChangeLEDStateRequest:
                        ChangeLEDStateEvent changeLEDEvent = (ChangeLEDStateEvent)newEvent;
                        bool ledIsHigh = changeLEDEvent._isHigh;
                        log.Debug(string.Format("Handling changeLED (setting high: {0}) ui event", changeLEDEvent._isHigh));
                        handleChangeLEDStateEvent(ledIsHigh);
                        break;
                    case EventIdentifier.ToggleOutputRequest:
                        ToggleOutputEvent toggleOutputEvent = (ToggleOutputEvent)newEvent;
                        DIOPins pinToToggle = toggleOutputEvent._pinToToggle;
                        log.Debug(string.Format("Handling toggleOutput (toggle pin: {0}) ui event", toggleOutputEvent._pinToToggle));
                        handleToggleOutputEvent(pinToToggle);
                        break;
                    case EventIdentifier.StartSequencerRequest:
                        log.Debug("Handling startTestSequence ui event");
                        StartSequencerEvent sequencerEvent = (StartSequencerEvent)newEvent;
                        handleStartSequencerEvent(sequencerEvent);
                        break;
                    case EventIdentifier.UpdateOutputRequest:
                        log.Debug("Handling updateOutput UI Event");
                        UpdateOutputEvent updateOutput = (UpdateOutputEvent)newEvent;
                        DIOPins pinToChange = updateOutput._pinToUpdate;
                        bool shouldSetHigh = updateOutput._shouldBeHigh;
                        handleUpdateOutputEvent(pinToChange, shouldSetHigh);
                        break;
                    default:
                        break;
                }
            }
        }

        
        //Handle the events by passing calls onto the logical layer
        private void handleConnectEvent(){ _logicalLayer.connectToDevice_LL(); }
        private void handleToggleLEDEvent(){ _logicalLayer.toggleLED_LL(); }
        private void handleFlashLEDEvent() { _logicalLayer.flashLED_LL(); }
        private void handleChangeLEDStateEvent(bool isHigh) { _logicalLayer.changeLEDState(isHigh); }
        private void handleToggleOutputEvent(DIOPins pinToToggle) { _logicalLayer.toggleOutput(pinToToggle); }
        private void handleUpdateOutputEvent(DIOPins pinToChange, bool shouldSetHigh) { _logicalLayer.controlOutput(pinToChange, shouldSetHigh); }


        private void handleStartSequencerEvent(StartSequencerEvent inEvent)
        {
            switch (inEvent._sequenceID)
            {
                case SequenceID.TestSequence:
                    _startTestSequenceEvent.Set();
                    break;
                case SequenceID.twoOzSequence:
                    _startTwoOzSequenceEvent.Set();
                    break;
                case SequenceID.fourOzSequence:
                    _startFourOzSequenceEvent.Set();
                    break;
                case SequenceID.eightOzSequence:
                    _startEightOzSequenceEvent.Set();
                    break;
                default:
                    log.Error(string.Format("{1} is not a valid sequencer id", inEvent._sequenceID));
                    break;
            }
        }





        //*************************************** Sequencer Methods **********************************************//

        /// <summary>
        /// Stops the sequencer thread until the the startTestSequencerEvent occures.  Or until the 
        /// supplied timeout is reached
        /// </summary>
        /// <param name="inMSToWait">The time in ms to wait before moving on</param>
        /// <returns>True if the even occured, false if the timeout was reached</returns>
        public bool waitForStartTestSequenceRequest(int inMSToWait = 0)
        {
            log.Debug("Waiting for start test to be clicked");
            if (inMSToWait == 0){_startTestSequenceEvent.WaitOne(); return true; }
            else{
                return _startTestSequenceEvent.WaitOne(inMSToWait);
            }
        }


        public bool waitForStartTwoOzSequencerRequest(int inMSToWait = 0)
        {
            log.Debug("Waiting for start two oz to be clicked");
            if (inMSToWait == 0) { _startTwoOzSequenceEvent.WaitOne(); return true; }
            else
            {
                return _startTwoOzSequenceEvent.WaitOne(inMSToWait);
            }
        }
        public bool waitForStartFourOzSequencerRequest(int inMSToWait = 0)
        {
            log.Debug("Waiting for start four oz to be clicked");
            if (inMSToWait == 0) { _startFourOzSequenceEvent.WaitOne(); return true; }
            else
            {
                return _startFourOzSequenceEvent.WaitOne(inMSToWait);
            }
        }
        public bool waitForStartEightOzSequencerRequest(int inMSToWait = 0)
        {
            log.Debug("Waiting for start eight oz to be clicked");
            if (inMSToWait == 0) { _startEightOzSequenceEvent.WaitOne(); return true; }
            else
            {
                return _startEightOzSequenceEvent.WaitOne(inMSToWait);
            }
        }
    }
}
