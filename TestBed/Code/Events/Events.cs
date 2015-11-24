using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace TestBed
{
    public class Event
    {
        public EventIdentifier identifier;
        /// <summary>
        /// Builds a Event and sets its identifier to the correct vale
        /// </summary>
        public Event()
        {
            identifier = EventIdentifier.GenericEvent;
        }


        /// <summary>
        /// Returns the Event type identifier
        /// </summary>
        public EventIdentifier getEventIdentifier()
        {
            return identifier;
        }
    }





    /// <summary>
    /// Represents an event that commands the device to toggle the led
    /// </summary>
    public class ToggleLEDEvent : Event
    {
        /// <summary>
        /// Creates a ToggleEvent and sets its identifier to the correct value
        /// </summary>
        public ToggleLEDEvent()
        {
            identifier = EventIdentifier.ToggleLEDRequest;
        }    
    }


    /// <summary>
    /// Represents an event that commands the program to connect to the device
    /// </summary>
    public class ConnectEvent : Event
    {
        /// <summary>
        /// Creates a ConnectEvent and sets its identifier to the correct value
        /// </summary>
        public ConnectEvent()
        {
            identifier = EventIdentifier.ConnectRequest;
        }
    }


    /// <summary>
    /// Represents an event that commands the device to flash its led
    /// </summary>
    public class FlashLEDEvent : Event
    {
        /// <summary>
        /// Creates the FlashLEDEvent and sets its identifier
        /// </summary>
        public FlashLEDEvent()
        {
            identifier = EventIdentifier.FlashLEDRequest;
        }
    }


    /// <summary>
    /// Represents an event that commands the device to change the LED state
    /// isHigh represents the state we want the led to take(On, Off)
    /// </summary>
    public class ChangeLEDStateEvent : Event
    {
        // The state we want to the LED to be in
        //true = high, false = low
        public bool _isHigh;

        /// <summary>
        /// Creates the object, sets the identifier, and remembers the desired state
        /// </summary>
        /// <param name="inIsHigh">The state we want the LED to be in</param>
        public ChangeLEDStateEvent(bool inIsHigh)
        {
            identifier = EventIdentifier.ChangeLEDStateRequest;
            _isHigh = inIsHigh;
        }
    }



    /// <summary>
    /// Represents an event that commands the device to toggle an output
    /// Also holds the output we want to toggle
    /// </summary>
    public class ToggleOutputEvent : Event
    {
        //The output we want to toggle
        public DIOPins _pinToToggle;

        /// <summary>
        /// Creates a ToggleOutputEvent and sets its identifier to the correct value
        /// Also saves the pin id to toggle
        /// </summary>
        public ToggleOutputEvent(DIOPins inPinToToggle)
        {
            identifier = EventIdentifier.ToggleOutputRequest;
            _pinToToggle = inPinToToggle;
        }
    }



    /// <summary>
    /// Represents an event that commands the device to update and output
    /// by setting it to a specific state
    /// </summary>
    public class UpdateOutputEvent : Event
    {
        //The pin to update
        public DIOPins _pinToUpdate;
        //High if we want the pin high, false if we want it low
        public bool _shouldBeHigh;

        /// <summary>
        /// Creates the event with the correct identifier and setting the
        /// pin to change and the sate to set
        /// </summary>
        /// <param name="inPinToChange">The pin to change</param>
        /// <param name="inSetPinHigh">The state to set</param>
        public UpdateOutputEvent(DIOPins inPinToChange, bool inSetPinHigh)
        {
            identifier = EventIdentifier.UpdateOutputRequest;
            _pinToUpdate = inPinToChange;
            _shouldBeHigh = inSetPinHigh;
        }
    }


    /// <summary>
    /// Represents an event that commands the device to query a specified analog input
    /// </summary>
    public class QueryAnalogInputEvent : Event
    {
        //The analog input to query
        public AnalogPins _pinToQuery;

        /// <summary>
        /// Creats the event and initializes all its properties
        /// </summary>
        /// <param name="inPinToQuery">The pin to query</param>
        public QueryAnalogInputEvent(AnalogPins inPinToQuery)
        {
            _pinToQuery = inPinToQuery;
            identifier = EventIdentifier.AnalogPinQueryRequest;
        }
    }


    /// <summary>
    /// Represents an event that commands the device to return the count on a 
    /// specified counter
    /// </summary>
    public class QueryCounterEvent : Event
    {
        //The counter pin to query
        public CounterPins _pinToQuery;

        /// <summary>
        /// Creats the event and initalizes
        /// </summary>
        /// <param name="inPinToQuery">The counter pin to query</param>
        public QueryCounterEvent(CounterPins inPinToQuery)
        {
            _pinToQuery = inPinToQuery;
            identifier = EventIdentifier.QueryCounterRequest;
        }
    }





    /// <summary>
    /// Represents an event that tell the test sequencer to start running
    /// </summary>
    public class StartSequencerEvent : Event
    {
        public SequenceID _sequenceID;

        public StartSequencerEvent(SequenceID inSequenceID)
        {
            _sequenceID = inSequenceID;
            identifier = EventIdentifier.StartSequencerRequest;
        }
    }
}
