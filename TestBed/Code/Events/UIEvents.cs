using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace TestBed
{
    public class UIEvent
    {
        public UIEventIdentifier identifier;
        /// <summary>
        /// Builds a UIEvent and sets its identifier to the correct vale
        /// </summary>
        public UIEvent()
        {
            identifier = UIEventIdentifier.GenericEvent;
        }


        /// <summary>
        /// Returns the UIEvent type identifier
        /// </summary>
        public UIEventIdentifier getEventIdentifier()
        {
            return identifier;
        }
    }





    /// <summary>
    /// Represents the event when the toggle button is clicked
    /// </summary>
    public class ToggleLEDUIEvent : UIEvent
    {
        /// <summary>
        /// Creates a ToggleUIEvent and sets its identifier to the correct value
        /// </summary>
        public ToggleLEDUIEvent()
        {
            identifier = UIEventIdentifier.ToggleLEDClicked;
        }    
    }


    /// <summary>
    /// Represents the event when the connect button is clicked
    /// </summary>
    public class ConnectUIEvent : UIEvent
    {
        /// <summary>
        /// Creates a ConnectUIEvent and sets its identifier to the correct value
        /// </summary>
        public ConnectUIEvent()
        {
            identifier = UIEventIdentifier.ConnectClicked;
        }
    }


    /// <summary>
    /// Represents the event when the Flash LED button is clicked
    /// </summary>
    public class FlashLEDUIEvent : UIEvent
    {
        /// <summary>
        /// Creates the FlashLEDUIEvent and sets its identifier
        /// </summary>
        public FlashLEDUIEvent()
        {
            identifier = UIEventIdentifier.FlashLEDClicked;
        }
    }


    /// <summary>
    /// Represents the event when the user clicks one of the 
    /// LED change state buttons (On, Off)
    /// </summary>
    public class ChangeLEDStateUIEvent : UIEvent
    {
        // The state we want to the LED to be in
        //true = high, false = low
        public bool _isHigh;

        /// <summary>
        /// Creates the object, sets the identifier, and remembers the desired state
        /// </summary>
        /// <param name="inIsHigh">The state we want the LED to be in</param>
        public ChangeLEDStateUIEvent(bool inIsHigh)
        {
            identifier = UIEventIdentifier.ChangeLEDStateClicked;
            _isHigh = inIsHigh;
        }
    }



    /// <summary>
    /// Represents the event when the toggle output button is clicked
    /// Also holds the output we want to toggle
    /// </summary>
    public class ToggleOutputUIEvent : UIEvent
    {
        //The output we want to toggle
        public DIOPins _pinToToggle;

        /// <summary>
        /// Creates a ToggleOutputUIEvent and sets its identifier to the correct value
        /// Also saves the pin id to toggle
        /// </summary>
        public ToggleOutputUIEvent(DIOPins inPinToToggle)
        {
            identifier = UIEventIdentifier.ToggleOutputClicked;
            _pinToToggle = inPinToToggle;
        }
    }



    /// <summary>
    /// Represents the event when a user clicks to change an output value
    /// </summary>
    public class UpdateOutputUIEvent : UIEvent
    {
        public DIOPins _pinToUpdate;
        public bool _shouldBeHigh;

        public UpdateOutputUIEvent(DIOPins inPinToChange, bool inSetPinHigh)
        {
            identifier = UIEventIdentifier.UpdateOutput;
            _pinToUpdate = inPinToChange;
            _shouldBeHigh = inSetPinHigh;
        }
    }


    public class QueryAnalogInputEvent : UIEvent
    {
        public AnalogPins _pinToQuery;
        public QueryAnalogInputEvent(AnalogPins inPinToQuery)
        {
            _pinToQuery = inPinToQuery;
            identifier = UIEventIdentifier.AnalogPinQuery;
        }

    }





    /// <summary>
    /// Represents the event which starts the test sequencer
    /// </summary>
    public class StartTestSequencerUIEvent : UIEvent
    {
        public StartTestSequencerUIEvent()
        {
            identifier = UIEventIdentifier.StartTestSequencerClicked;
        }
    }
}
