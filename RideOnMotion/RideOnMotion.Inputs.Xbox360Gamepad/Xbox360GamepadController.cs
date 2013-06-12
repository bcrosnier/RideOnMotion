using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Xbox360Gamepad
{
	public class Xbox360GamepadController
	{
		XboxController _selectedController;

		readonly float NavigationValue = 1f;
		readonly float NavigationValue2 = 1f;
		readonly int TriggerDeadZone = 4096;
		readonly int TriggerReactionscale = 2048;
		readonly float NumberOfScale = 14f;

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
		bool specialActionButton = false;

		float rollAndPitchValues;
		float gazAndYawValues;


        public DroneCommand ActiveDrone;

		public Xbox360GamepadController()
		{
		}

		public void Start()
		{
			_selectedController = XboxController.RetrieveController( 0 );
			_selectedController.StateChanged += _selectedController_StateChanged;
			XboxController.StartPolling();
		}

		private void _selectedController_StateChanged( object sender, XboxControllerStateChangedEventArgs e )
		{
			MapInput();
		}

		public InputState GetCurrentControlInput(InputState _lastInputState)
		{
			if ( roll != _lastInputState.Roll || pitch != _lastInputState.Pitch || yaw != _lastInputState.Yaw || gaz != _lastInputState.Gaz || cameraSwap != _lastInputState.CameraSwap || takeOff != _lastInputState.TakeOff ||
				land != _lastInputState.Land || hover != _lastInputState.Hover || emergency != _lastInputState.Emergency || flatTrim != _lastInputState.FlatTrim || specialActionButton != _lastInputState.SpecialAction )
			{
				InputState newInputState = new InputState( roll, pitch, yaw, gaz, cameraSwap, takeOff, land, hover, emergency, flatTrim, specialActionButton );
				return newInputState;
			}
			else
			{
				return null;
			}
		}
			
		internal void MapInput()
		{
			roll = 0;
			pitch = 0;
			yaw = 0;
			gaz = 0;

			cameraSwap = false;
			takeOff = false;
			land = false;
			hover = false;
			emergency = false;

			flatTrim = false;
			specialActionButton = false;


			if ( _selectedController.IsBackPressed )
			{
				cameraSwap = true;
			}
			if ( _selectedController.IsRightShoulderPressed )
			{
				takeOff = true;
			}
			if ( _selectedController.IsLeftShoulderPressed )
			{
				land = true;
			}
			if ( _selectedController.IsRightStickPressed )
			{
				hover = true;
			}
			if ( _selectedController.IsBPressed )
			{
				emergency = true;
			}
			if ( _selectedController.IsStartPressed )
			{
				flatTrim = true;
			}
			if ( _selectedController.IsXPressed )
			{
				specialActionButton = true;
			}

			gazAndYawValues = NavigationValue;
			rollAndPitchValues = NavigationValue2;

			if ( _selectedController.LeftTrigger > 0 || _selectedController.RightTrigger > 0 )
			{
				float gazScale = 0;
				if ( _selectedController.LeftTrigger > 0 && _selectedController.RightTrigger > 0 )
				{
					gazScale = 0;
				}
				else if ( _selectedController.RightTrigger > 0 )
				{
					gazScale = ( _selectedController.RightTrigger ) / 255f;
				}
				else if ( _selectedController.LeftTrigger > 0 )
				{
					gazScale = ( _selectedController.LeftTrigger ) / -255f;
				}
				gaz = gazAndYawValues * gazScale;
			}

			if ( _selectedController.LeftThumbStick.X > TriggerDeadZone || _selectedController.LeftThumbStick.X < -TriggerDeadZone
				|| _selectedController.LeftThumbStick.Y > TriggerDeadZone || _selectedController.LeftThumbStick.Y < -TriggerDeadZone )
			{
				int rollScale = 14;
				if ( _selectedController.LeftThumbStick.X > 0 )
				{
					rollScale = ( ( _selectedController.LeftThumbStick.X - TriggerDeadZone ) + TriggerReactionscale - 1 ) / TriggerReactionscale;
				}
				else
				{
					rollScale = ( ( _selectedController.LeftThumbStick.X + TriggerDeadZone ) - TriggerReactionscale + 1 ) / TriggerReactionscale;
				}
				roll = rollAndPitchValues * ( rollScale / NumberOfScale );

				int pitchScale = 14;

				//reverse from the others
				if ( _selectedController.LeftThumbStick.Y > 0 )
				{
					pitchScale = ( ( _selectedController.LeftThumbStick.Y - TriggerDeadZone ) + TriggerReactionscale - 1 ) / TriggerReactionscale;
				}
				else
				{
					pitchScale = ( ( _selectedController.LeftThumbStick.Y + TriggerDeadZone ) - TriggerReactionscale + 1 ) / TriggerReactionscale;
				}
				pitch = -(rollAndPitchValues * ( pitchScale / NumberOfScale ));
			}
			if ( _selectedController.RightThumbStick.X > TriggerDeadZone || _selectedController.RightThumbStick.X < - TriggerDeadZone )
			{
				int yawScale = 14;
				if ( _selectedController.RightThumbStick.X > 0 )
				{
					yawScale = ( ( _selectedController.RightThumbStick.X - TriggerDeadZone ) + TriggerReactionscale - 1 ) / TriggerReactionscale;
				}
				else
				{
					yawScale = ( ( _selectedController.RightThumbStick.X + TriggerDeadZone ) - TriggerReactionscale + 1 ) / TriggerReactionscale;
				}
				yaw = gazAndYawValues * ( yawScale / NumberOfScale );
			}
		}
		public void Stop()
		{
			XboxController.StopPolling();
		}
	}
}
