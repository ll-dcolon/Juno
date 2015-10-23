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



    class FlashLEDUIEvent : UIEvent
    {
        public FlashLEDUIEvent()
        {
            identifier = UIEventIdentifier.FlashLEDClicked;
        }
    }

    class ChangeLEDStateUIEvent : UIEvent
    {
        public bool _isHigh;

        public ChangeLEDStateUIEvent(bool inIsHigh)
        {
            identifier = UIEventIdentifier.ChangeLEDStateClicked;
            _isHigh = inIsHigh;
        }
    }
}
