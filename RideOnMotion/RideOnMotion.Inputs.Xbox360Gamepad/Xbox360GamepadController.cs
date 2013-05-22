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

		readonly float fastNavigationValue = 0.5f;
		readonly float fastNavigationValue2 = 0.2f;
		readonly float normalNavigationValue = 0.25f;
		readonly float normalNavigationValue2 = 0.1f;
		readonly float slowNavigationValue = 0.1f;
		readonly float slowNavigationValue2 = 0.05f;
		readonly int TriggerDeadZone = 4096;
		readonly int TriggerReactionscale = 2048;
		readonly int TriggerMax = ushort.MaxValue - 4096;
		readonly int TriggerMin = ushort.MinValue + 4096;
		readonly float NumberOfScale = 14f;
		float lastRoll = 0;
		float lastPitch = 0;
		float lastyaw = 0;
		float lastgaz = 0;

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
			if ( _selectedController.IsLeftShoulderPressed)
			{
				_drone.Takeoff();
				return;
			}
			if ( _selectedController.IsStartPressed )
			{
				_drone.FlatTrim();
				return;
			}

			//slow
			if ( _selectedController.LeftTrigger > 20 && _selectedController.RightTrigger < 20 )
			{
				gazAndYawValues = slowNavigationValue;
				rollAndPitchValues = slowNavigationValue2;
			}
			//fast
			else if ( _selectedController.LeftTrigger < 20 && _selectedController.RightTrigger > 20 )
			{
				gazAndYawValues = fastNavigationValue;
				rollAndPitchValues = fastNavigationValue2;
			}
			else
			{
				gazAndYawValues = normalNavigationValue;
				rollAndPitchValues = normalNavigationValue2;
			}



			if ( (_selectedController.IsAPressed || _selectedController.IsYPressed)
				&& !(_selectedController.IsAPressed && _selectedController.IsYPressed) )
			{
				if( _selectedController.IsAPressed)
				{
					gaz = - gazAndYawValues;
				}
				else if ( _selectedController.IsYPressed )
				{
					gaz = gazAndYawValues;
				}
			}
			if ( ( _selectedController.IsXPressed || _selectedController.IsBPressed )
				&& !( _selectedController.IsXPressed && _selectedController.IsBPressed ) )
			{
				if ( _selectedController.IsXPressed )
				{
					yaw = - gazAndYawValues;
				}
				else if ( _selectedController.IsBPressed )
				{
					yaw = gazAndYawValues;
				}
			}

			if ( ( _selectedController.IsDPadDownPressed || _selectedController.IsDPadUpPressed )
				&& !( _selectedController.IsDPadDownPressed && _selectedController.IsDPadUpPressed ) )
			{
				if ( _selectedController.IsDPadDownPressed )
				{
					pitch = rollAndPitchValues;
				}
				else if ( _selectedController.IsDPadUpPressed )
				{
					pitch = -rollAndPitchValues;
				}
			}
			if ( ( _selectedController.IsDPadLeftPressed || _selectedController.IsDPadRightPressed )
				&& !( _selectedController.IsDPadLeftPressed && _selectedController.IsDPadRightPressed ) )
			{
				if ( _selectedController.IsDPadLeftPressed )
				{
					roll = rollAndPitchValues;
				}
				else if ( _selectedController.IsDPadRightPressed )
				{
					roll = -rollAndPitchValues;
				}
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
				if ( _selectedController.LeftThumbStick.Y < 0 )
				{
					pitchScale = ( ( _selectedController.LeftThumbStick.Y + TriggerDeadZone ) - TriggerReactionscale + 1 ) / TriggerReactionscale;
				}
				else
				{
					pitchScale = ( ( _selectedController.LeftThumbStick.Y - TriggerDeadZone ) + TriggerReactionscale - 1 ) / TriggerReactionscale;
				}
				pitch = rollAndPitchValues * ( pitchScale / NumberOfScale );
			}
			if ( _selectedController.RightThumbStick.X > TriggerDeadZone || _selectedController.RightThumbStick.X < - TriggerDeadZone
				|| _selectedController.RightThumbStick.Y > TriggerDeadZone || _selectedController.RightThumbStick.Y < -TriggerDeadZone )
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

				int gazScale = 14;
				if ( _selectedController.RightThumbStick.Y > 0 )
				{
					gazScale = ( ( _selectedController.RightThumbStick.Y - TriggerDeadZone ) + TriggerReactionscale - 1 ) / TriggerReactionscale;
				}
				else
				{
					gazScale = ( ( _selectedController.RightThumbStick.Y + TriggerDeadZone ) - TriggerReactionscale + 1 ) / TriggerReactionscale;
				}
				gaz = gazAndYawValues * ( gazScale / NumberOfScale );
			}

			if(roll !=lastRoll || pitch != lastPitch ||  yaw != lastyaw || gaz != lastgaz)
			{
				_drone.Navigate( roll, pitch, yaw, gaz );
				lastRoll = roll;
				lastPitch = pitch;
				lastyaw = yaw;
				lastgaz = gaz;
			}

			//RideOnMotion.Logger.Instance.NewEntry( CKLogLevel.Trace, CKTraitTags.User, yaw + " " + gaz + " "+ roll + " "+ pitch );
		}
		public void Stop()
		{
			XboxController.StopPolling();
		}
	}
}
