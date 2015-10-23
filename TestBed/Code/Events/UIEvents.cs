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
    class ToggleLEDUIEvent : UIEvent
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
    class ConnectUIEvent : UIEvent
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
    class FlashLEDUIEvent : UIEvent
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
    class ChangeLEDStateUIEvent : UIEvent
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
}
