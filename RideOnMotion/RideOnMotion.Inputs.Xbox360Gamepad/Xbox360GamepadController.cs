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
		readonly int TriggerDeadZone = 4096;
		readonly int TriggerReactionscale;
		readonly float NumberOfScale = 1024f;

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
		String InputString;


        public DroneCommand ActiveDrone;

		public Xbox360GamepadController()
		{
			TriggerReactionscale = 28672/1024;
		}

		public void Start()
		{
			_selectedController = XboxController.RetrieveController( 0 );
			XboxController.StartPolling();
		}

		public void Stop()
		{
			XboxController.StopPolling();
		}

		public void StartMappingForDrone()
		{
			_selectedController.StateChanged += SelectedController_StateChanged;
		}

		public void StopMappingForDrone()
		{
			_selectedController.StateChanged -= SelectedController_StateChanged;
		}

		public void StartMappingForSettings()
		{
			_selectedController.StateChanged += SelectedController_SettingsStateChanged;
		}

		public void StopMappingForSettings()
		{
			_selectedController.StateChanged -= SelectedController_SettingsStateChanged;
		}

		private void SelectedController_SettingsStateChanged( object sender, XboxControllerStateChangedEventArgs e )
		{
			InputString = MapSettings();
		}

		private string MapSettings()
		{
			String Input = null;
			if ( _selectedController.IsBackPressed )
			{
				return "Back";
			}
			else if ( _selectedController.IsBackPressed )
			{
				return "Back";
			}
			else if ( _selectedController.IsStartPressed )
			{
				return "Start";
			}
			else if ( _selectedController.IsAPressed )
			{
				return "A";
			}
			else if ( _selectedController.IsBPressed )
			{
				return "B";
			}
			else if ( _selectedController.IsXPressed )
			{
				return "X";
			}
			else if ( _selectedController.IsYPressed )
			{
				return "Y";
			}
			else if ( _selectedController.LeftTrigger > 0 )
			{
				return "Left Trigger";
			}
			else if ( _selectedController.RightTrigger > 0 )
			{
				return "Right Trigger";
			}
			else if ( _selectedController.IsLeftShoulderPressed )
			{
				return "Left Shoulder";
			}
			else if ( _selectedController.IsRightShoulderPressed )
			{
				return "Right Shoulder";
			}
			else if ( _selectedController.LeftThumbStick.Y > TriggerDeadZone )
			{
				return "Left Thumbstick Up";
			}
			else if ( _selectedController.LeftThumbStick.Y < -TriggerDeadZone )
			{
				return "Left Thumbstick Down";
			}
			else if ( _selectedController.LeftThumbStick.X < -TriggerDeadZone )
			{
				return "Left Thumbstick Left";
			}
			else if ( _selectedController.LeftThumbStick.X > TriggerDeadZone )
			{
				return "Left Thumbstick Right";
			}
			else if ( _selectedController.IsLeftStickPressed )
			{
				return "Left Thumbstick Pression";
			}
			else if ( _selectedController.RightThumbStick.Y > TriggerDeadZone )
			{
				return "Right Thumbstick Up";
			}
			else if ( _selectedController.RightThumbStick.Y < -TriggerDeadZone )
			{
				return "Right Thumbstick Down";
			}
			else if ( _selectedController.RightThumbStick.X < -TriggerDeadZone )
			{
				return "Right Thumbstick Left";
			}
			else if ( _selectedController.RightThumbStick.X > TriggerDeadZone )
			{
				return "Right Thumbstick Right";
			}
			else if ( _selectedController.IsRightStickPressed )
			{
				return "Right Thumbstick Pression";
			}
			else if ( _selectedController.IsDPadUpPressed )
			{
				return "Directionnal Pad Up";
			}
			else if ( _selectedController.IsDPadDownPressed )
			{
				return "Directionnal Pad Down";
			}
			else if ( _selectedController.IsDPadLeftPressed )
			{
				return "Directionnal Pad Left";
			}
			else if ( _selectedController.IsDPadRightPressed )
			{
				return "Directionnal Pad Right";
			}
			return Input;
		}

		private void SelectedController_StateChanged( object sender, XboxControllerStateChangedEventArgs e )
		{
			MapInput();
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
            if ( ConvertStringToBoolMapping( Properties.Settings.Default.TakeOff ) )
			{
				takeOff = true;
			}
            if ( ConvertStringToBoolMapping( Properties.Settings.Default.Land ) )
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
			float scale = NumberOfScale;
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
					scale = ( ( ConvertStringToIntMapping( UpOrLeft ) - 1 ) / TriggerReactionscale ) / NumberOfScale;
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
					scale = ( ( ConvertStringToIntMapping( DownOrRight ) + 1 ) / TriggerReactionscale ) / NumberOfScale;
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
