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
		SendDroneCommand _sendDroneCommand;
		bool[] _heldDown;
		Key _lastKey;
		InputState _lastInputState;

        public KeyboardController()
		{
			_heldDown = new bool[256];
			_sendDroneCommand = new SendDroneCommand();
			_lastInputState = new InputState();
        }

        public DroneCommand ActiveDrone
        {
            set
            {
                this._sendDroneCommand.ActiveDrone = value;
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
			ProcessAction();
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
			ProcessAction();
		}

		public void ProcessAction()
		{
			InputState currentInput = GetCurrentControlInput();
			if ( currentInput != null )
			{
				_sendDroneCommand.process( currentInput );
			}
		}

		public InputState GetCurrentControlInput()
		{
			float roll = 0;
			float pitch = 0;
			float yaw = 0;
			float gaz = 0;

			bool cameraSwap = false;
			bool takeOff = false;
			bool land = false;
			bool hover = false;
			bool emergency = false;

			bool flatTrim = false;

			if ( _heldDown[(int)Key.Q] && !_heldDown[(int)Key.D])
			{
				roll = -1;
			}
			else if ( _heldDown[(int)Key.D] && !_heldDown[(int)Key.Q] )
			{
				roll = 1;
			}

			if ( _heldDown[(int)Key.Z] && !_heldDown[(int)Key.S] )
			{
				pitch = -1;
			}
			else if ( _heldDown[(int)Key.S] && !_heldDown[(int)Key.Z] )
			{
				pitch = 1;
			}

			if ( _heldDown[(int)Key.Down] && !_heldDown[(int)Key.Up] )
			{
				gaz = -1;
			}
			else if ( _heldDown[(int)Key.Up] && !_heldDown[(int)Key.Down] )
			{
				gaz = 1;
			}

			if ( _heldDown[(int)Key.Left] && !_heldDown[(int)Key.Right] )
			{
				yaw = -1;
			}
			else if ( _heldDown[(int)Key.Right] && !_heldDown[(int)Key.Left] )
			{
				yaw = 1;
			}

			if ( _heldDown[(int)Key.C] )
			{
				cameraSwap = true;
			}
			if ( _heldDown[(int)Key.Return] )
			{
				takeOff = true;
			}
			if ( _heldDown[(int)Key.Space] )
			{
				land = true;
			}
			if ( _heldDown[(int)Key.LeftCtrl] || _heldDown[(int)Key.RightCtrl] )
			{
				hover = true;
			}
			if ( _heldDown[(int)Key.End] )
			{
				emergency = true;
			}
			
			// TODO test

			if ( roll != _lastInputState.Roll || pitch != _lastInputState.Pitch || yaw != _lastInputState.Yaw || gaz != _lastInputState.Gaz || cameraSwap != _lastInputState.CameraSwap || takeOff != _lastInputState.TakeOff ||
				land != _lastInputState.Land || hover != _lastInputState.Hover || emergency != _lastInputState.Emergency || flatTrim != _lastInputState.FlatTrim )
			{
				InputState newInputState = new InputState( roll, pitch, yaw, gaz, cameraSwap, takeOff, land, hover, emergency, flatTrim, false );
				_lastInputState = newInputState;
				return newInputState;
			}
			else
			{
				return null;
			}
		}

	}
}
