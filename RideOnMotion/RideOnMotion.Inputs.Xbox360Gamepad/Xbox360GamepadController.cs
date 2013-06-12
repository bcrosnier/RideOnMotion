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

		readonly float MaxSpeed = 1f;
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

		bool usingTrigger;
		bool usingStick;


		public Xbox360GamepadController()
		{
		}

		public void Start()
		{
			_selectedController = XboxController.RetrieveController( 0 );
			_selectedController.StateChanged += SelectedController_StateChanged;
			XboxController.StartPolling();
		}

		private void SelectedController_StateChanged( object sender, XboxControllerStateChangedEventArgs e )
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


			if ( ConvertStringToBoolMapping(Properties.Settings.Default.CameraSwap ) )
			{
				cameraSwap = true;
			}
			if ( ConvertStringToBoolMapping(Properties.Settings.Default.TakeOff) )
			{
				takeOff = true;
			}
			if ( ConvertStringToBoolMapping(Properties.Settings.Default.Land) )
			{
				land = true;
			}
			if ( ConvertStringToBoolMapping(Properties.Settings.Default.Hover) )
			{
				hover = true;
			}
			if ( ConvertStringToBoolMapping(Properties.Settings.Default.Emergency) )
			{
				emergency = true;
			}
			if ( ConvertStringToBoolMapping(Properties.Settings.Default.FlatTrim) )
			{
				flatTrim = true;
			}
			if ( ConvertStringToBoolMapping(Properties.Settings.Default.SpecialAction) )
			{
				specialActionButton = true;
			}
			//Properties.Settings.Default.
			gaz = SetValueBasedOnInput( Properties.Settings.Default.GazUp, Properties.Settings.Default.GazDown );
			roll = SetValueBasedOnInput( Properties.Settings.Default.RollLeft, Properties.Settings.Default.RollRight );
			//Pitch is reversed
			pitch = -SetValueBasedOnInput( Properties.Settings.Default.PitchDown, Properties.Settings.Default.PitchUp );
			yaw = SetValueBasedOnInput( Properties.Settings.Default.YawLeft, Properties.Settings.Default.YawRight );
		}

		public float SetValueBasedOnInput(String UpOrLeft, String DownOrRight)
		{
			float scale = 14;
			ResetUsingStickAndTrigger();
			if ( ConvertStringToBoolMapping( UpOrLeft ) && ConvertStringToBoolMapping(  DownOrRight ) )
			{
				scale = 0;
				ResetUsingStickAndTrigger();
			}
			else if ( ConvertStringToBoolMapping( UpOrLeft ) )
			{
				if ( usingStick )
				{
					scale = ( ( ConvertStringToIntMapping( UpOrLeft ) - 1 ) / TriggerReactionscale ) / 14f;
					ResetUsingStickAndTrigger();
				}
				else
				{
					float maxValue = MaxValue();
					scale = ( ConvertStringToIntMapping( UpOrLeft ) ) / maxValue;
				}
			}
			else if ( ConvertStringToBoolMapping( DownOrRight ) )
			{
				if ( usingStick )
				{
					scale = ( ( ConvertStringToIntMapping( DownOrRight ) + 1 ) / TriggerReactionscale ) / 14f;
					ResetUsingStickAndTrigger();
				}
				else
				{
					float maxValue = MaxValue();
					scale = ( ConvertStringToIntMapping( DownOrRight ) ) / -maxValue;
				}
			}
			else
			{
				scale = 0;
			}
			return MaxSpeed * scale;
		}
		public void Stop()
		{
			XboxController.StopPolling();
		}

		float MaxValue()
		{
			float max = 1;
			if ( usingTrigger )
			{
				max = 255f;
				usingTrigger = false;
			}
			else if ( usingStick )
			{
				max = ushort.MaxValue - TriggerDeadZone/2;
				usingStick = false;
			}
			return max;
		}

		void ResetUsingStickAndTrigger()
		{
			usingTrigger = false;
			usingStick = false;
		}

		internal bool ConvertStringToBoolMapping( String Input )
		{
			switch ( Input )
			{
				case "Back": ;
					return _selectedController.IsBackPressed;
				case "Start": ;
					return _selectedController.IsStartPressed;
				case "A": ;
					return _selectedController.IsAPressed;
				case "B": ;
					return _selectedController.IsBPressed;
				case "X": ;
					return _selectedController.IsXPressed;
				case "Y": ;
					return _selectedController.IsYPressed;
				case "Left Trigger": ;
					usingTrigger = true;
					return _selectedController.LeftTrigger > 0;
				case "Right Trigger": ;
					usingTrigger = true;
					return _selectedController.RightTrigger > 0;
				case "Left Shoulder": ;
					return _selectedController.IsLeftShoulderPressed;
				case "Right Shoulder": ;
					return _selectedController.IsRightShoulderPressed;
				case "Left Thumbstick Up": ;
					usingStick = true;
					return _selectedController.LeftThumbStick.Y > TriggerDeadZone;
				case "Left Thumbstick Down": ;
					usingStick = true;
					return _selectedController.LeftThumbStick.Y < -TriggerDeadZone;
				case "Left Thumbstick Left": ;
					usingStick = true;
					return _selectedController.LeftThumbStick.X < -TriggerDeadZone;
				case "Left Thumbstick Right": ;
					usingStick = true;
					return _selectedController.LeftThumbStick.X > TriggerDeadZone;
				case "Left Thumbstick Pression": ;
					return _selectedController.IsLeftStickPressed;
				case "Right Thumbstick Up": ;
					usingStick = true;
					return _selectedController.RightThumbStick.Y > TriggerDeadZone;
				case "Right Thumbstick Down": ;
					usingStick = true;
					return _selectedController.RightThumbStick.Y < -TriggerDeadZone;
				case "Right Thumbstick Left": ;
					usingStick = true;
					return _selectedController.RightThumbStick.X < -TriggerDeadZone;
				case "Right Thumbstick Right": ;
					usingStick = true;
					return _selectedController.RightThumbStick.X > TriggerDeadZone;
				case "Right Thumbstick Pression": ;
					return _selectedController.IsRightStickPressed;
				case "Directionnal Pad Up": ;
					return _selectedController.IsDPadUpPressed;
				case "Directionnal Pad Down": ;
					return _selectedController.IsDPadDownPressed;
				case "Directionnal Pad Left": ;
					return _selectedController.IsDPadLeftPressed;
				case "Directionnal Pad Right": ;
					return _selectedController.IsDPadRightPressed;
				default:
					return false;
			}
		}
		internal int ConvertStringToIntMapping( String Input )
		{
			switch ( Input )
			{
				case "Back": ;
					return _selectedController.IsBackPressed ? 1 : 0;
				case "Start": ;
					return _selectedController.IsStartPressed ? 1 : 0;
				case "A": ;
					return _selectedController.IsAPressed ? 1 : 0;
				case "B": ;
					return _selectedController.IsBPressed ? 1 : 0;
				case "X": ;
					return _selectedController.IsXPressed ? 1 : 0;
				case "Y": ;
					return _selectedController.IsYPressed ? 1 : 0;
				case "Left Trigger": ;
					usingTrigger = true;
					return _selectedController.LeftTrigger;
				case "Right Trigger": ;
					usingTrigger = true;
					return _selectedController.RightTrigger;
				case "Left Shoulder": ;
					return _selectedController.IsLeftShoulderPressed ? 1 : 0;
				case "Right Shoulder": ;
					return _selectedController.IsRightShoulderPressed ? 1 : 0;
				case "Left Thumbstick Up": ;
					usingStick = true;
					return _selectedController.LeftThumbStick.Y > TriggerDeadZone ? _selectedController.LeftThumbStick.Y - TriggerDeadZone : 0;
				case "Left Thumbstick Down": ;
					usingStick = true;
					return _selectedController.LeftThumbStick.Y < -TriggerDeadZone ? _selectedController.LeftThumbStick.Y + TriggerDeadZone : 0;
				case "Left Thumbstick Left": ;
					usingStick = true;
					return _selectedController.LeftThumbStick.X < -TriggerDeadZone ? _selectedController.LeftThumbStick.X + TriggerDeadZone : 0;
				case "Left Thumbstick Right": ;
					usingStick = true;
					return _selectedController.LeftThumbStick.X > TriggerDeadZone ? _selectedController.LeftThumbStick.X - TriggerDeadZone : 0;
				case "Left Thumbstick Pression": ;
					return _selectedController.IsLeftStickPressed ? 1 : 0;
				case "Right Thumbstick Up": ;
					usingStick = true;
					return _selectedController.RightThumbStick.Y > TriggerDeadZone ? _selectedController.RightThumbStick.Y - TriggerDeadZone : 0;
				case "Right Thumbstick Down": ;
					usingStick = true;
					return _selectedController.RightThumbStick.Y < -TriggerDeadZone ? _selectedController.RightThumbStick.Y + TriggerDeadZone : 0;
				case "Right Thumbstick Left": ;
					usingStick = true;
					return _selectedController.RightThumbStick.X < -TriggerDeadZone ? _selectedController.RightThumbStick.X + TriggerDeadZone : 0;
				case "Right Thumbstick Right": ;
					usingStick = true;
					return _selectedController.RightThumbStick.X > TriggerDeadZone ? _selectedController.RightThumbStick.X - TriggerDeadZone : 0;
				case "Right Thumbstick Pression": ;
					return _selectedController.IsRightStickPressed ? 1 : 0;
				case "Directionnal Pad Up": ;
					return _selectedController.IsDPadUpPressed ? 1 : 0;
				case "Directionnal Pad Down": ;
					return _selectedController.IsDPadDownPressed ? 1 : 0;
				case "Directionnal Pad Left": ;
					return _selectedController.IsDPadLeftPressed ? 1 : 0;
				case "Directionnal Pad Right": ;
					return _selectedController.IsDPadRightPressed ? 1 : 0;
				default:
					return 0;
			}
		}
	}
}
