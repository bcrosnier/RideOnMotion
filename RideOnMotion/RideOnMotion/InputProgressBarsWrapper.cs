using RideOnMotion.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

        internal InputProgressBarsWrapper( double thickness)
        {
            GamepadProgressBarSet = new InputProgressBarSet( thickness, Brushes.Red);
            KeyboardProgressBarSet = new InputProgressBarSet( thickness, Brushes.Yellow);
            KinectProgressBarSet = new InputProgressBarSet( thickness, Brushes.Yellow);
        }

        public void UpdateInputStates( InputState gamepadState, InputState keyboardState, InputState kinectState )
        {
            GamepadProgressBarSet.UpdateInputState( gamepadState );
            KeyboardProgressBarSet.UpdateInputState( keyboardState );
            KinectProgressBarSet.UpdateInputState( kinectState );
        }

        internal void UpdateSpeeds()
        {
            GamepadProgressBarSet.UpdateMaxSpeeds();
            KeyboardProgressBarSet.UpdateMaxSpeeds();
            KinectProgressBarSet.UpdateMaxSpeeds();
        }
    }
}
