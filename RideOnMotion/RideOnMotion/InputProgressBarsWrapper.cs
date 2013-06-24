using RideOnMotion.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.UI
{
    internal class InputProgressBarsWrapper
    {
        public InputProgressBarSet GamepadProgressBarSet
        {
            get;
            private set;
        }

        public InputProgressBarSet KeyboardProgressBarSet
        {
            get;
            private set;
        }

        public InputProgressBarSet KinectProgressBarSet
        {
            get;
            private set;
        }

        public InputProgressBarSet ActiveSet
        {
            get;
            private set;
        }

        internal InputProgressBarsWrapper( double thickness )
        {
            GamepadProgressBarSet = new InputProgressBarSet( thickness, null );
            KeyboardProgressBarSet = new InputProgressBarSet( thickness, null );
            KinectProgressBarSet = new InputProgressBarSet( thickness, null );
        }

        public void UpdateInputStates( InputState gamepadState, InputState keyboardState, InputState kinectState )
        {
            GamepadProgressBarSet.UpdateInputState( gamepadState );
            KeyboardProgressBarSet.UpdateInputState( keyboardState );
            KinectProgressBarSet.UpdateInputState( kinectState );
        }
    }
}
