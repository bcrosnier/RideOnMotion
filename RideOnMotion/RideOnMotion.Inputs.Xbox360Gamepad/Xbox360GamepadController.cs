using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Xbox360Gamepad
{
	public class Xbox360GamepadController
	{
        DroneCommand _drone;

		XboxController _selectedController;

		readonly float NavigationValue = 0.33f;
		readonly float NavigationValue2 = 0.25f;
		readonly int TriggerDeadZone = 4096;
		readonly int TriggerReactionscale = 2048;
		readonly float NumberOfScale = 14f;
		float lastRoll = 0;
		float lastPitch = 0;
		float lastyaw = 0;
		float lastgaz = 0;
		bool Hover = false;
		bool BackPressed = false;

        public DroneCommand ActiveDrone
        {
            set
            {
                this._drone = value;
            }
        }
		public void Start()
		{
			_selectedController = XboxController.RetrieveController( 0 );
			_selectedController.StateChanged += _selectedController_StateChanged;
			XboxController.StartPolling();
		}

		private void _selectedController_StateChanged( object sender, XboxControllerStateChangedEventArgs e )
		{
			SendCommand();
		}

		internal void SendCommand()
		{
			//mist be bound to rollAndPitchValues
			float roll = 0;
			float pitch = 0;

			//must be bound to gazAndYawValues
			float yaw = 0;
			float gaz = 0;

			float rollAndPitchValues;
			float gazAndYawValues;

			if ( _selectedController.IsLeftShoulderPressed )
			{
				_drone.Land();
				return;
			}
			if ( _selectedController.IsRightShoulderPressed)
			{
				_drone.Takeoff();
				return;
			}
			if ( _selectedController.IsStartPressed )
			{
				_drone.FlatTrim();
				return;
			}
			if ( !_selectedController.IsBackPressed )
			{
				BackPressed = false;
			}
			if ( _selectedController.IsBackPressed && !BackPressed)
			{
				_drone.ChangeCamera();
				BackPressed = true;
				return;
			}
			if ( _selectedController.IsLeftStickPressed )
			{
				if ( Hover )
				{
					_drone.LeaveHoverMode();
					Hover = false;
				}
			}

			if ( _selectedController.IsRightStickPressed )
			{
				if (!Hover)
				{
					_drone.EnterHoverMode();
					Hover = true;
				}
			}

			if ( _selectedController.IsXPressed )
			{
				_drone.PlayLED();
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

			if(roll !=lastRoll || pitch != lastPitch ||  yaw != lastyaw || gaz != lastgaz)
			{
				_drone.Navigate( roll, pitch, yaw, gaz );
				lastRoll = roll;
				lastPitch = pitch;
				lastyaw = yaw;
				lastgaz = gaz;
			}

			RideOnMotion.Logger.Instance.NewEntry( CKLogLevel.Trace, CKTraitTags.User, yaw + " " + gaz + " "+ roll + " "+ pitch );
		}
		public void Stop()
		{
			XboxController.StopPolling();
		}
	}
}
