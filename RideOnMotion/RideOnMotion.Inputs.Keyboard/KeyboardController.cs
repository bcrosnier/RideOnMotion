using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RideOnMotion.Inputs.Keyboard
{
    public class KeyboardController // : IDroneInputController // Fired until interface is fixed and/or valid.
    {
        DroneCommand _drone;
		bool[] _heldDown;
		Key _lastKey;

        public KeyboardController()
		{
			_heldDown = new bool[256];
        }

        public DroneCommand ActiveDrone
        {
            set
            {
                this._drone = value;
            }
        }

        /// <summary>
        /// Event triggered on key press.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Currently, it's only valid for an AZERTY keyboard, at the character level.
        /// To use virtual keys or scancodes instead, we might want to check KeyInterop.VirtualKeyFromKey.
        /// </remarks>
        public void ProcessKeyDown( KeyEventArgs e )
        {
			Key currentKey = e.Key;
			_heldDown[(int)e.Key] = true;

			_lastKey = currentKey;
        }

        /// <summary>
        /// Event triggered on key press.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Currently, it's only valid for an AZERTY keyboard, at the character level.
        /// To use virtual keys or scancodes instead, we might want to check KeyInterop.VirtualKeyFromKey.
        /// </remarks>
		public void ProcessKeyUp( KeyEventArgs e )
		{
			_heldDown[(int)e.Key] = false;
		}
	}
}
