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
        public static readonly double ACTIVE_OPACITY = 0.8;
        public static readonly double INACTIVE_OPACITY = 0.3;
        public InputProgressBarSet GamepadProgressBarSet
        {
            get;
            private set;
        }
        public double GamepadProgressBarOpacity
        {
            get;
            private set;
        }

        public InputProgressBarSet KeyboardProgressBarSet
        {
            get;
            private set;
        }
        public double KeyboardProgressBarOpacity
        {
            get;
            private set;
        }

        public InputProgressBarSet KinectProgressBarSet
        {
            get;
            private set;
        }
        public double KinectProgressBarOpacity
        {
            get;
            private set;
        }

        internal InputProgressBarsWrapper( double thickness )
        {
            GamepadProgressBarSet = new InputProgressBarSet( thickness, Brushes.Red, System.Windows.VerticalAlignment.Top );
            KeyboardProgressBarSet = new InputProgressBarSet( thickness, Brushes.Yellow, System.Windows.VerticalAlignment.Bottom );
            KinectProgressBarSet = new InputProgressBarSet( thickness, Brushes.AliceBlue, System.Windows.VerticalAlignment.Center );
        }

        public void UpdateInputStates( InputState gamepadState, InputState keyboardState, InputState kinectState )
        {
            GamepadProgressBarSet.UpdateInputState( gamepadState );
            KeyboardProgressBarSet.UpdateInputState( keyboardState );
            KinectProgressBarSet.UpdateInputState( kinectState );

            // GAZ
            if ( gamepadState.Gaz != 0 && ( keyboardState.Gaz != 0 || kinectState.Gaz != 0 ) )
            {
                GamepadProgressBarSet.PositiveGazProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.NegativeGazProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.PositiveGazProgressBar.Opacity = INACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativeGazProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.PositiveGazProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.NegativeGazProgressBar.Opacity = INACTIVE_OPACITY;
            }
            else if ( keyboardState.Gaz != 0 && kinectState.Gaz != 0 )
            {
                KeyboardProgressBarSet.PositiveGazProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativeGazProgressBar.Opacity = ACTIVE_OPACITY;
                KinectProgressBarSet.PositiveGazProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.NegativeGazProgressBar.Opacity = INACTIVE_OPACITY;
                GamepadProgressBarSet.PositiveGazProgressBar.Opacity = INACTIVE_OPACITY;
                GamepadProgressBarSet.NegativeGazProgressBar.Opacity = INACTIVE_OPACITY;
            }
            else
            {
                KinectProgressBarSet.PositiveGazProgressBar.Opacity = ACTIVE_OPACITY;
                KinectProgressBarSet.NegativeGazProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.PositiveGazProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativeGazProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.PositiveGazProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.NegativeGazProgressBar.Opacity = ACTIVE_OPACITY;
            }

            // PITCH
            if ( gamepadState.Pitch != 0 && ( keyboardState.Pitch != 0 || kinectState.Pitch != 0 ) )
            {
                GamepadProgressBarSet.PositivePitchProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.NegativePitchProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.PositivePitchProgressBar.Opacity = INACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativePitchProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.PositivePitchProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.NegativePitchProgressBar.Opacity = INACTIVE_OPACITY;
            }
            else if ( keyboardState.Pitch != 0 && kinectState.Pitch != 0 )
            {
                KeyboardProgressBarSet.PositivePitchProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativePitchProgressBar.Opacity = ACTIVE_OPACITY;
                KinectProgressBarSet.PositivePitchProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.NegativePitchProgressBar.Opacity = INACTIVE_OPACITY;
                GamepadProgressBarSet.PositivePitchProgressBar.Opacity = INACTIVE_OPACITY;
                GamepadProgressBarSet.NegativePitchProgressBar.Opacity = INACTIVE_OPACITY;
            }
            else
            {
                KinectProgressBarSet.PositivePitchProgressBar.Opacity = ACTIVE_OPACITY;
                KinectProgressBarSet.NegativePitchProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.PositivePitchProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativePitchProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.PositivePitchProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.NegativePitchProgressBar.Opacity = ACTIVE_OPACITY;
            }

            // YAW
            if ( gamepadState.Yaw != 0 && ( keyboardState.Yaw != 0 || kinectState.Yaw != 0 ) )
            {
                GamepadProgressBarSet.PositiveYawProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.NegativeYawProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.PositiveYawProgressBar.Opacity = INACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativeYawProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.PositiveYawProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.NegativeYawProgressBar.Opacity = INACTIVE_OPACITY;
            }
            else if ( keyboardState.Yaw != 0 && kinectState.Yaw != 0 )
            {
                KeyboardProgressBarSet.PositiveYawProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativeYawProgressBar.Opacity = ACTIVE_OPACITY;
                KinectProgressBarSet.PositiveYawProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.NegativeYawProgressBar.Opacity = INACTIVE_OPACITY;
                GamepadProgressBarSet.PositiveYawProgressBar.Opacity = INACTIVE_OPACITY;
                GamepadProgressBarSet.NegativeYawProgressBar.Opacity = INACTIVE_OPACITY;
            }
            else
            {
                KinectProgressBarSet.PositiveYawProgressBar.Opacity = ACTIVE_OPACITY;
                KinectProgressBarSet.NegativeYawProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.PositiveYawProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativeYawProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.PositiveYawProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.NegativeYawProgressBar.Opacity = ACTIVE_OPACITY;
            }

            //ROLL
            if ( gamepadState.Roll != 0 && ( keyboardState.Roll != 0 || kinectState.Roll != 0 ) )
            {
                GamepadProgressBarSet.PositiveRollProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.NegativeRollProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.PositiveRollProgressBar.Opacity = INACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativeRollProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.PositiveRollProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.NegativeRollProgressBar.Opacity = INACTIVE_OPACITY;
            }
            else if ( keyboardState.Roll != 0 && kinectState.Roll != 0 )
            {
                KeyboardProgressBarSet.PositiveRollProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativeRollProgressBar.Opacity = ACTIVE_OPACITY;
                KinectProgressBarSet.PositiveRollProgressBar.Opacity = INACTIVE_OPACITY;
                KinectProgressBarSet.NegativeRollProgressBar.Opacity = INACTIVE_OPACITY;
                GamepadProgressBarSet.PositiveRollProgressBar.Opacity = INACTIVE_OPACITY;
                GamepadProgressBarSet.NegativeRollProgressBar.Opacity = INACTIVE_OPACITY;
            }
            else
            {
                KinectProgressBarSet.PositiveRollProgressBar.Opacity = ACTIVE_OPACITY;
                KinectProgressBarSet.NegativeRollProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.PositiveRollProgressBar.Opacity = ACTIVE_OPACITY;
                KeyboardProgressBarSet.NegativeRollProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.PositiveRollProgressBar.Opacity = ACTIVE_OPACITY;
                GamepadProgressBarSet.NegativeRollProgressBar.Opacity = ACTIVE_OPACITY;
            }
        }

        internal void UpdateSpeeds()
        {
            GamepadProgressBarSet.UpdateMaxSpeeds();
            KeyboardProgressBarSet.UpdateMaxSpeeds();
            KinectProgressBarSet.UpdateMaxSpeeds();
        }
    }
}
